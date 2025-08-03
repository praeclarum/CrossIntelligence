using Newtonsoft.Json;

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
    private readonly List<Message> transcript = new List<Message>();

    public OpenAIIntelligenceSessionImplementation(string model, string apiKey, IIntelligenceTool[]? tools, string instructions, HttpClient? httpClient = null)
    {
        this.model = model;
        this.apiKey = apiKey;
        this.tools = tools ?? Array.Empty<IIntelligenceTool>();
        this.httpClient = httpClient ?? new HttpClient();
        this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        if (!string.IsNullOrEmpty(instructions))
        {
            transcript.Add(new Message
            {
                Role = "developer",
                Content = [new Content { Type = "input_text", Text = instructions }]
            });
        }
    }

    public async Task<string> RespondAsync(string prompt)
    {
        var userMessage = new Message
        {
            Role = "user",
            Content = [new Content { Type = "input_text", Text = prompt }]
        };
        var request = new ResponsesRequest
        {
            Model = model,
            Input = transcript.Concat(new[] { userMessage }).ToArray()
        };
        var response = await GetResponseAsync(request);
        transcript.Add(userMessage);
        transcript.AddRange(response.Output);
        var allOutput = string.Join("\n\n", response.Output.SelectMany(m => m.Content).Select(c => c.Text).Where(t => !string.IsNullOrEmpty(t)));
        return allOutput;
    }

    class ResponsesRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
        [JsonProperty("input")]
        public Message[] Input { get; set; } = Array.Empty<Message>();
    }

    class ResponsesResponse
    {
        [JsonProperty("output")]
        public Message[] Output { get; set; } = Array.Empty<Message>();
    }

    class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public Content[] Content { get; set; } = Array.Empty<Content>();
    }

    class Content
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string? Text { get; set; } = null;
    }

    async Task<ResponsesResponse> GetResponseAsync(ResponsesRequest request)
    {
        var json = JsonConvert.SerializeObject(request);
        var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"https://api.openai.com/v1/responses", requestContent).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var responseData = JsonConvert.DeserializeObject<ResponsesResponse>(responseBody);
        if (responseData == null || responseData.Output.Length == 0)
        {
            throw new InvalidOperationException("Invalid response from OpenAI API.");
        }
        return responseData;
    }
}
