using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

// ReSharper disable InconsistentNaming

namespace CrossIntelligence;

public class OpenAIModel : IIntelligenceModel
{
    public readonly string Model;
    public readonly string? ApiKey;
    
    public string Id => $"openai:{Model}";

    public OpenAIModel(string model, string? apiKey = null)
    {
        Model = model;
        ApiKey = apiKey;
    }

    public IIntelligenceSessionImplementation CreateSessionImplementation(IIntelligenceTool[]? tools, string instructions)
    {
        var apiKey = ApiKey ?? (Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "");
        return new OpenAISessionImplementation(model: Model, apiKey: apiKey, tools: tools, instructions: instructions);
    }
}

class OpenAISessionImplementation : ResponsesApiSessionImplementation
{
    public OpenAISessionImplementation(string model, string apiKey, IIntelligenceTool[]? tools, string instructions, HttpClient? httpClient = null)
        : base("https://api.openai.com/v1", model, apiKey, tools, instructions, httpClient)
    {
    }
}
