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
    private readonly HttpClient httpClient;
    private readonly List<InputMessage> transcript = new List<InputMessage>();

    public OpenAIIntelligenceSessionImplementation(string model, string apiKey, IIntelligenceTool[]? tools, string instructions, HttpClient? httpClient = null)
    {
        this.model = model;
        this.apiKey = apiKey;
        this.tools = tools ?? Array.Empty<IIntelligenceTool>();
        this.httpClient = httpClient ?? new HttpClient();
        this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        if (!string.IsNullOrEmpty(instructions))
        {
            transcript.Add(new InputMessage
            {
                Role = "developer",
                Content = [new Content { Type = "input_text", Text = instructions }]
            });
        }
    }

    public async Task<string> RespondAsync(string prompt)
    {
        var userMessage = new InputMessage
        {
            Role = "user",
            Content = [new Content { Type = "input_text", Text = prompt }]
        };
        var rtools = new List<ToolDefinition>();
        foreach (var tool in tools)
        {
            rtools.Add(await ToolDefinition.FromToolAsync(tool).ConfigureAwait(false));
        }
        var request = new ResponsesRequest
        {
            Model = model,
            Input = transcript.Concat(new[] { userMessage }).ToArray(),
            Tools = rtools.ToArray()
        };
        var response = await GetResponseAsync(request).ConfigureAwait(false);
        transcript.Add(userMessage);
        foreach (var output in response.Output)
        {
            transcript.Add(new InputMessage
            {
                Role = output.Role,
                Content = output.Content
            });
        }
        var allOutput = string.Join("\n\n", response.Output.SelectMany(m => m.Content).Select(c => c.Text).Where(t => !string.IsNullOrEmpty(t)));
        return allOutput;
    }

    class ResponsesRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
        [JsonProperty("input")]
        public InputMessage[] Input { get; set; } = Array.Empty<InputMessage>();
        [JsonProperty("tools")]
        public ToolDefinition[] Tools { get; set; } = Array.Empty<ToolDefinition>();
    }

    class ResponsesResponse
    {
        [JsonProperty("output")]
        public OutputMessage[] Output { get; set; } = Array.Empty<OutputMessage>();
    }

    class InputMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;
        [JsonProperty("content")]
        public Content[] Content { get; set; } = Array.Empty<Content>();
    }

    class OutputMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;
        [JsonProperty("content")]
        public Content[] Content { get; set; } = Array.Empty<Content>();
        [JsonProperty("status")]
        public string? Status { get; set; } = null;
        [JsonProperty("call_id")]
        public string? CallId { get; set; } = null;
        [JsonProperty("name")]
        public string? Name { get; set; } = null;
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

        public static async Task<ToolDefinition> FromToolAsync(IIntelligenceTool tool)
        {
            var definition = new ToolDefinition
            {
                Name = tool.Name,
                Description = tool.Description,
            };
            var schemaString = tool.GetArgumentsJsonSchema();
            Console.WriteLine($"Tool {tool.Name} schema: {schemaString}");
            var ps = JSchema.Parse(schemaString);
            definition.Parameters = ps;
            return definition;
        }
    }

    async Task<ResponsesResponse> GetResponseAsync(ResponsesRequest request)
    {
        var json = JsonConvert.SerializeObject(request, Formatting.Indented);
        Console.WriteLine($"Request JSON: {json}");
        var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"https://api.openai.com/v1/responses", requestContent).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        System.Console.WriteLine(responseBody);
        response.EnsureSuccessStatusCode();

        var responseData = JsonConvert.DeserializeObject<ResponsesResponse>(responseBody);
        if (responseData == null || responseData.Output.Length == 0)
        {
            throw new InvalidOperationException("Invalid response from OpenAI API.");
        }
        return responseData;
    }
}
