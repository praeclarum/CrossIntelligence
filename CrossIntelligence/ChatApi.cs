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
    private readonly List<Message> transcript = new();
    private bool disposed = false;

    public ChatApiSessionImplementation(string baseUrl, string model, string apiKey, IIntelligenceTool[]? tools, string instructions, HttpClient? httpClient = null)
    {
        this.baseUrl = baseUrl;
        this.model = model;
        this.apiKey = apiKey;
        this.tools = tools ?? Array.Empty<IIntelligenceTool>();
        this.toolDefinitions = this.tools.Select(tool => ToolDefinition.FromTool(tool)).ToArray();
        this.httpClient = httpClient ?? new HttpClient();
        this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        if (!string.IsNullOrEmpty(instructions))
        {
            transcript.Add(new Message
            {
                Role = "developer",
                Content = instructions
            });
        }
    }

    ~ChatApiSessionImplementation()
    {
        Dispose(false);
    }

    public Task<string> RespondAsync(string prompt)
    {
        return InternalRespondAsync(prompt, null);
    }

    public Task<string> RespondAsync(string prompt, Type responseType)
    {
        return InternalRespondAsync(prompt, responseType);
    }

    async Task<string> InternalRespondAsync(string prompt, Type? responseType)
    {
        var userMessage = new Message
        {
            Role = "user",
            Content = prompt
        };
        transcript.Add(userMessage);
        ResponseFormat? responseFormat = null;
        if (responseType is not null)
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
        var response = await GetResponseAsync(initialRequest).ConfigureAwait(false);
        var toolResults = new List<FunctionCallOutputMessage>();
        do
        {
            toolResults.Clear();
            foreach (var output in response.Choices)
            {/*
                transcript.Add(output);
                if (false)
                {
                    var toolName = output.Name;
                    var result = "";
                    var tool = tools.FirstOrDefault(t => t.Name == toolName);
                    try
                    {
                        if (tool is not null)
                        {
                            result = await tool.ExecuteAsync(output.Arguments ?? "").ConfigureAwait(false);
                        }
                        else
                        {
                            result = $"Function '{toolName}' not found.";
                        }
                    }
                    catch (Exception ex)
                    {
                        result = $"Error: {ex.Message}";
                    }
                    var m = new FunctionCallOutputMessage
                    {
                        CallId = output.CallId ?? "",
                        Output = result
                    };
                    toolResults.Add(m);
                }*/
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
                response = await GetResponseAsync(toolOutputRequest).ConfigureAwait(false);
            }
        } while (toolResults.Count > 0);
        var allOutput = response.Choices
            .Where(t => !string.IsNullOrEmpty(t.Message?.Content))
            .Select(m => m.Message?.Content)
            .FirstOrDefault() ?? "";
        return allOutput;
    }

    async Task<ChatCompletionsResponse> GetResponseAsync(ChatCompletionsRequest request)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None
        };
        var requestBody = JsonConvert.SerializeObject(request, settings);
        // System.Diagnostics.Debug.WriteLine(requestBody);
        var requestContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{baseUrl}/chat/completions", requestContent).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Chat API request failed with status code {response.StatusCode} ({(int)response.StatusCode}): {responseBody}");
        }
        // System.Diagnostics.Debug.WriteLine(responseBody);

        var responseData = JsonConvert.DeserializeObject<ChatCompletionsResponse>(responseBody);
        if (responseData == null || responseData.Choices.Length == 0)
        {
            throw new InvalidOperationException("Invalid response from Chat API.");
        }
        return responseData;
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
                // Dispose managed resources
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

    class FunctionCallOutputMessage : Message
    {
        [JsonProperty("call_id")]
        public string? CallId { get; set; } = null;
        [JsonProperty("output")]
        public string? Output { get; set; } = null;
    }

    class OutputMessage : Message
    {
        [JsonProperty("status")]
        public string? Status { get; set; } = null;
        [JsonProperty("call_id")]
        public string? CallId { get; set; } = null;
        [JsonProperty("name")]
        public string? Name { get; set; } = null;
        [JsonProperty("arguments")]
        public string? Arguments { get; set; } = null;
        [JsonProperty("summary")]
        public string[]? Summary { get; set; } = null;
    }

    class ToolDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("strict")]
        public bool Strict { get; set; } = true;

        [JsonProperty("type")]
        public string Type { get; set; } = "function";

        [JsonProperty("parameters")]
        public JSchema? Parameters { get; set; }

        public static ToolDefinition FromTool(IIntelligenceTool tool)
        {
            var definition = new ToolDefinition
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
