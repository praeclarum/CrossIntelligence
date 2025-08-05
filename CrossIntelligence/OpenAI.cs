using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace CrossIntelligence;

public class OpenAIModel : IIntelligenceModel
{
    public readonly string Model;
    public readonly string ApiKey;

    public OpenAIModel(string model, string apiKey)
    {
        Model = model;
        ApiKey = apiKey;
    }

    public IIntelligenceSessionImplementation CreateSessionImplementation(IIntelligenceTool[]? tools, string instructions)
    {
        return new OpenAIIntelligenceSessionImplementation(model: Model, apiKey: ApiKey, tools: tools, instructions: instructions);
    }
}

class OpenAIIntelligenceSessionImplementation : IIntelligenceSessionImplementation
{
    private readonly string model;
    private readonly string apiKey;
    private readonly IIntelligenceTool[] tools;
    private readonly ToolDefinition[] toolDefinitions;
    private readonly HttpClient httpClient;
    private readonly List<Message> transcript = new();
    private bool disposed = false;

    public OpenAIIntelligenceSessionImplementation(string model, string apiKey, IIntelligenceTool[]? tools, string instructions, HttpClient? httpClient = null)
    {
        this.model = model;
        this.apiKey = apiKey;
        this.tools = tools ?? Array.Empty<IIntelligenceTool>();
        this.toolDefinitions = this.tools.Select(tool => ToolDefinition.FromTool(tool)).ToArray();
        this.httpClient = httpClient ?? new HttpClient();
        this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        if (!string.IsNullOrEmpty(instructions))
        {
            transcript.Add(new InputContentMessage
            {
                Role = "developer",
                Content = [new Content { Type = "input_text", Text = instructions }]
            });
        }
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
        var userMessage = new InputContentMessage
        {
            Role = "user",
            Content = [new Content { Type = "input_text", Text = prompt }]
        };
        transcript.Add(userMessage);
        TextOptions? textOptions = null;
        if (responseType is not null)
        {
            var schema = responseType.GetJsonSchemaObject();
            textOptions = new TextOptions
            {
                Format = new TextFormat
                {
                    Type = "json_schema",
                    Name = responseType.Name,
                    Schema = schema
                }
            };
        }
        var initialRequest = new ResponsesRequest
        {
            Model = model,
            Input = transcript.ToArray(),
            Tools = toolDefinitions,
            TextOptions = textOptions
        };
        var response = await GetResponseAsync(initialRequest).ConfigureAwait(false);
        var toolResults = new List<FunctionCallOutputMessage>();
        do
        {
            toolResults.Clear();
            foreach (var output in response.Output)
            {
                transcript.Add(output);
                if (output.Type == "message")
                {
                    var m = new InputContentMessage
                    {
                        Role = output.Role,
                        Content = output.Content
                    };
                }
                else if (output.Type == "function_call")
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
                }
            }
            foreach (var toolResult in toolResults)
            {
                transcript.Add(toolResult);
            }
            if (toolResults.Count > 0)
            {
                var toolOutputRequest = new ResponsesRequest
                {
                    Model = model,
                    Input = transcript.ToArray(),
                    Tools = toolDefinitions,
                    TextOptions = textOptions
                };
                response = await GetResponseAsync(toolOutputRequest).ConfigureAwait(false);
            }
        } while (toolResults.Count > 0);
        var allOutput = string.Join("\n\n", response.Output.Where(t => t.Content != null).SelectMany(m => m.Content ?? Array.Empty<Content>()).Select(c => c.Text));
        return allOutput;
    }

    async Task<ResponsesResponse> GetResponseAsync(ResponsesRequest request)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
        var json = JsonConvert.SerializeObject(request, settings);
        var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"https://api.openai.com/v1/responses", requestContent).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"OpenAI API request failed with status code {response.StatusCode} ({(int)response.StatusCode}): {responseBody}");
        }

        var responseData = JsonConvert.DeserializeObject<ResponsesResponse>(responseBody);
        if (responseData == null || responseData.Output.Length == 0)
        {
            throw new InvalidOperationException("Invalid response from OpenAI API.");
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

    ~OpenAIIntelligenceSessionImplementation()
    {
        Dispose(false);
    }

    class ResponsesRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
        [JsonProperty("input")]
        public Message[] Input { get; set; } = Array.Empty<Message>();
        [JsonProperty("tools")]
        public ToolDefinition[] Tools { get; set; } = Array.Empty<ToolDefinition>();
        [JsonProperty("text")]
        public TextOptions? TextOptions { get; set; } = null;
    }

    class ResponsesResponse
    {
        [JsonProperty("output")]
        public OutputMessage[] Output { get; set; } = Array.Empty<OutputMessage>();
    }

    class Message
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "message";
    }

    class InputMessage : Message
    {
    }

    class InputContentMessage : InputMessage
    {
        [JsonProperty("role")]
        public string? Role { get; set; } = null;
        [JsonProperty("content")]
        public Content[]? Content { get; set; } = null;
    }

    class FunctionCallOutputMessage : InputMessage
    {
        [JsonProperty("call_id")]
        public string? CallId { get; set; } = null;
        [JsonProperty("output")]
        public string? Output { get; set; } = null;
        public FunctionCallOutputMessage()
        {
            Type = "function_call_output";
        }
    }

    class OutputMessage : Message
    {
        [JsonProperty("role")]
        public string? Role { get; set; } = null;
        [JsonProperty("content")]
        public Content[]? Content { get; set; } = null;
        [JsonProperty("status")]
        public string? Status { get; set; } = null;
        [JsonProperty("call_id")]
        public string? CallId { get; set; } = null;
        [JsonProperty("name")]
        public string? Name { get; set; } = null;
        [JsonProperty("arguments")]
        public string? Arguments { get; set; } = null;
    }

    class Content
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string? Text { get; set; } = null;
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

    class TextOptions
    {
        [JsonProperty("format")]
        public TextFormat? Format { get; set; } = null;
    }

    class TextFormat
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "json_schema";
        [JsonProperty("name")]
        public string? Name { get; set; } = null;
        [JsonProperty("schema")]
        public JSchema? Schema { get; set; } = null;
    }
}
