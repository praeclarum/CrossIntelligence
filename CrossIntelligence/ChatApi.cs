using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace CrossIntelligence;

class ChatApiSessionImplementation : IIntelligenceSessionImplementation
{
    private readonly string baseUrl;
    private readonly string model;
    private readonly string apiKey;
    private readonly IIntelligenceTool[] tools;
    private readonly ToolDefinition[] toolDefinitions;
    private readonly HttpClient httpClient;
    // True when this instance created the HttpClient; false when the caller injected it.
    // We must not dispose an externally owned HttpClient.
    private readonly bool ownsHttpClient;
    private readonly List<Message> transcript = new();
    // Retained reference so we can patch the role in-place on provider fallback.
    private readonly Message? instructionMessage;
    private bool disposed = false;

    // Preferred instruction role; falls back to "system" the first time a provider rejects "developer".
    private string instructionRole = "developer";
    // Set to false after the first provider rejection of json_schema response_format.
    private bool supportsJsonSchemaFormat = true;

    public ChatApiSessionImplementation(string baseUrl, string model, string apiKey, IIntelligenceTool[]? tools, string instructions, HttpClient? httpClient = null)
    {
        this.baseUrl = baseUrl;
        this.model = model;
        this.apiKey = apiKey;
        this.tools = tools ?? Array.Empty<IIntelligenceTool>();
        this.toolDefinitions = this.tools.Select(tool => ToolDefinition.FromTool(tool)).ToArray();
        // Track ownership so Dispose only releases what this instance allocated.
        ownsHttpClient = httpClient == null;
        this.httpClient = httpClient ?? new HttpClient();
        this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        if (!string.IsNullOrEmpty(instructions))
        {
            instructionMessage = new Message
            {
                Role = instructionRole,
                Content = instructions
            };
            transcript.Add(instructionMessage);
        }
    }

    ~ChatApiSessionImplementation()
    {
        Dispose(false);
    }

    // IIntelligenceSessionImplementation – backward-compatible signatures delegate to CT overloads.
    public Task<string> RespondAsync(string prompt)
        => RespondAsync(prompt, CancellationToken.None);

    public Task<string> RespondAsync(string prompt, Type responseType)
        => RespondAsync(prompt, responseType, CancellationToken.None);

    // Extended overloads that expose CancellationToken support to callers.
    public Task<string> RespondAsync(string prompt, CancellationToken cancellationToken)
        => InternalRespondAsync(prompt, null, cancellationToken);

    public Task<string> RespondAsync(string prompt, Type responseType, CancellationToken cancellationToken)
        => InternalRespondAsync(prompt, responseType, cancellationToken);

    async Task<string> InternalRespondAsync(string prompt, Type? responseType, CancellationToken cancellationToken)
    {
        var userMessage = new Message
        {
            Role = "user",
            Content = prompt
        };
        transcript.Add(userMessage);
        ResponseFormat? responseFormat = null;
        if (responseType is not null && supportsJsonSchemaFormat)
        {
            var schema = responseType.GetJsonSchemaObject();
            responseFormat = new ResponseFormat
            {
                Type = "json_schema",
                JsonSchema = new ResponseJsonSchema
                {
                    Name = responseType.Name,
                    Strict = true,
                    Schema = schema
                }
            };
        }
        var initialRequest = new ChatCompletionsRequest
        {
            Model = model,
            Messages = transcript.ToArray(),
            Tools = toolDefinitions,
            ResponseFormat = responseFormat
        };
        var response = await GetResponseAsync(initialRequest, cancellationToken).ConfigureAwait(false);
        // If the provider rejected json_schema on the initial call, clear responseFormat so
        // subsequent tool-loop requests don't waste an attempt trying it again.
        if (!supportsJsonSchemaFormat)
            responseFormat = null;
        var toolResults = new List<ToolCallOutputMessage>();
        do
        {
            toolResults.Clear();
            var choice = response.Choices.FirstOrDefault(x => x.Message is not null);
            if (choice?.Message is OutputMessage message)
            {
                transcript.Add(message);
                foreach (var toolCall in message.ToolCalls ?? [])
                {
                    var result = await CallToolAsync(toolCall.Function?.Name ?? "", toolCall.Function?.Arguments ?? "", cancellationToken).ConfigureAwait(false);
                    toolResults.Add(new ToolCallOutputMessage
                    {
                        ToolCallId = toolCall.Id ?? "",
                        Content = result
                    });
                }
            }
            foreach (var toolResult in toolResults)
            {
                transcript.Add(toolResult);
            }
            if (toolResults.Count > 0)
            {
                var toolOutputRequest = new ChatCompletionsRequest
                {
                    Model = model,
                    Messages = transcript.ToArray(),
                    Tools = toolDefinitions,
                    ResponseFormat = responseFormat
                };
                response = await GetResponseAsync(toolOutputRequest, cancellationToken).ConfigureAwait(false);
            }
        } while (toolResults.Count > 0);
        var allOutput = response.Choices
            .Where(t => !string.IsNullOrEmpty(t.Message?.Content))
            .Select(m => m.Message?.Content)
            .FirstOrDefault() ?? "";
        return allOutput;
    }

    async Task<ChatCompletionsResponse> GetResponseAsync(ChatCompletionsRequest request, CancellationToken cancellationToken)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None
        };

        // Up to 3 attempts with deterministic compatibility fallbacks (no infinite retry):
        //   attempt 0 – original request
        //   attempt 1 – if 4xx: switch instruction role "developer" -> "system"
        //   attempt 2 – if still 4xx: drop unsupported json_schema response_format
        for (int attempt = 0; attempt < 3; attempt++)
        {
            var requestBody = JsonConvert.SerializeObject(request, settings);
            // System.Diagnostics.Debug.WriteLine(requestBody);
            var requestContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            var httpResponse = await httpClient.PostAsync($"{baseUrl}/chat/completions", requestContent, cancellationToken).ConfigureAwait(false);
            var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (httpResponse.IsSuccessStatusCode)
            {
                // System.Diagnostics.Debug.WriteLine(responseBody);
                var responseData = JsonConvert.DeserializeObject<ChatCompletionsResponse>(responseBody);
                if (responseData == null || responseData.Choices.Length == 0)
                    throw new InvalidOperationException("Invalid response from Chat API.");
                return responseData;
            }

            int statusCode = (int)httpResponse.StatusCode;

            // Only apply compatibility fallbacks for 4xx client errors; server errors are not retried.
            if (statusCode >= 400 && statusCode < 500)
            {
                // Fallback 1: some providers/models don't recognise the "developer" role; retry with "system".
                if (instructionRole == "developer")
                {
                    instructionRole = "system";
                    // Patch the retained instruction message in-place so the transcript stays consistent.
                    if (instructionMessage != null)
                        instructionMessage.Role = "system";
                    continue;
                }

                // Fallback 2: provider/model doesn't support json_schema response_format; retry without it.
                if (request.ResponseFormat != null)
                {
                    supportsJsonSchemaFormat = false;
                    request = new ChatCompletionsRequest
                    {
                        Model = request.Model,
                        Messages = request.Messages,
                        Tools = request.Tools,
                        ResponseFormat = null // omit unsupported structured-output format
                    };
                    continue;
                }
            }

            // No applicable fallback – surface the error immediately.
            throw new HttpRequestException($"Chat API request failed with status code {httpResponse.StatusCode} ({statusCode}): {responseBody}");
        }

        // Unreachable in practice; all code paths above either return or throw.
        throw new InvalidOperationException("Unexpected exit from compatibility retry loop.");
    }

    async Task<string> CallToolAsync(string toolName, string arguments, CancellationToken cancellationToken)
    {
        try
        {
            // Check cancellation before starting potentially slow tool work.
            cancellationToken.ThrowIfCancellationRequested();
            if (tools.FirstOrDefault(t => t.Name == toolName) is IIntelligenceTool tool)
            {
                // IIntelligenceTool.ExecuteAsync does not accept a CancellationToken;
                // propagation ends here for now.
                return await tool.ExecuteAsync(arguments).ConfigureAwait(false);
            }
            else
            {
                return $"Error: Tool \"{toolName}\" not found.";
            }
        }
        catch (OperationCanceledException)
        {
            throw; // Never swallow cancellation – let it propagate to the caller.
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Only dispose the HttpClient when this instance owns it (i.e. created it internally).
                // An externally injected HttpClient is the caller's responsibility.
                if (ownsHttpClient)
                    httpClient?.Dispose();
            }
            disposed = true;
        }
    }

    class ChatCompletionsRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
        [JsonProperty("messages")]
        public Message[] Messages { get; set; } = Array.Empty<Message>();
        [JsonProperty("tools")]
        public ToolDefinition[] Tools { get; set; } = Array.Empty<ToolDefinition>();
        [JsonProperty("response_format")]
        public ResponseFormat? ResponseFormat { get; set; } = null;
    }

    class ChatCompletionsResponse
    {
        [JsonProperty("choices")]
        public Choice[] Choices { get; set; } = Array.Empty<Choice>();
    }

    class Choice
    {
        [JsonProperty("message")]
        public OutputMessage? Message { get; set; } = null;
        [JsonProperty("finish_reason")]
        public string? FinishReason { get; set; } = null;
        [JsonProperty("index")]
        public int Index { get; set; } = 0;
    }

    class Message
    {
        [JsonProperty("role")]
        public string? Role { get; set; } = null;
        [JsonProperty("content")]
        public string? Content { get; set; } = null;
    }

    class ToolCallOutputMessage : Message
    {
        [JsonProperty("tool_call_id")]
        public string? ToolCallId { get; set; } = null;
        public ToolCallOutputMessage()
        {
            Role = "tool";
        }
    }

    class OutputMessage : Message
    {
        [JsonProperty("tool_calls")]
        public ToolCall[]? ToolCalls { get; set; } = null;
        [JsonProperty("refusal")]
        public string? Refusal { get; set; } = null;
        [JsonProperty("reasoning")]
        public string? Reasoning { get; set; } = null;
    }

    class ToolCall
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";
        [JsonProperty("type")]
        public string Type { get; set; } = "";
        [JsonProperty("index")]
        public int Index { get; set; } = 0;
        [JsonProperty("function")]
        public ToolCallFunction? Function { get; set; } = null;
    }

    class ToolCallFunction
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("arguments")]
        public string? Arguments { get; set; } = null;
    }

    class ToolDefinition
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "function";
        [JsonProperty("function")]
        public ToolFunction? Function { get; set; } = null;
        public static ToolDefinition FromTool(IIntelligenceTool tool)
        {
            return new ToolDefinition
            {
                Function = ToolFunction.FromTool(tool)
            };
        }
    }

    class ToolFunction
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("parameters")]
        public JSchema? Parameters { get; set; }

        public static ToolFunction FromTool(IIntelligenceTool tool)
        {
            var definition = new ToolFunction
            {
                Name = tool.Name,
                Description = tool.Description,
            };
            var schemaString = tool.GetArgumentsJsonSchema();
            var ps = JSchema.Parse(schemaString);
            definition.Parameters = ps;
            return definition;
        }
    }

    class ResponseFormat
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "json_schema";
        [JsonProperty("json_schema")]
        public ResponseJsonSchema? JsonSchema { get; set; } = null;
    }
    
    class ResponseJsonSchema
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "Object";
        [JsonProperty("strict")]
        public bool Strict { get; set; } = true;
        [JsonProperty("schema")]
        public JSchema? Schema { get; set; } = null;
    }
}
