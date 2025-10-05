using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

// ReSharper disable InconsistentNaming

namespace CrossIntelligence;

public class OpenRouterModel : IIntelligenceModel
{
    public readonly string Model;
    public readonly string? ApiKey;
    
    public string Id => $"openrouter:{Model}";

    public OpenRouterModel(string model, string? apiKey = null)
    {
        Model = model;
        ApiKey = apiKey;
    }

    public IIntelligenceSessionImplementation CreateSessionImplementation(IIntelligenceTool[]? tools, string instructions)
    {
        var apiKey = ApiKey ?? (Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "");
        return new OpenRouterSessionImplementation(model: Model, apiKey: apiKey, tools: tools, instructions: instructions);
    }
}

class OpenRouterSessionImplementation : ChatApiSessionImplementation
{
    public OpenRouterSessionImplementation(string model, string apiKey, IIntelligenceTool[]? tools, string instructions, HttpClient? httpClient = null)
        : base("https://openrouter.ai/api/v1", model, apiKey, tools, instructions, httpClient)
    {
    }
}
