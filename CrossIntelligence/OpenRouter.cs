namespace CrossIntelligence;

public class OpenRouterModel : IIntelligenceModel
{
    public readonly string Model;
    
    public string Id => $"openrouter:{Model}";

    public OpenRouterModel(string model)
    {
        Model = model;
    }

    public IIntelligenceSessionImplementation CreateSessionImplementation(IIntelligenceTool[]? tools, string instructions)
    {
        return new OpenRouterSessionImplementation(model: Model, tools: tools, instructions: instructions);
    }
}

class OpenRouterSessionImplementation : ChatApiSessionImplementation
{
    public OpenRouterSessionImplementation(string model, IIntelligenceTool[]? tools, string instructions, HttpClient? httpClient = null)
        : base("https://openrouter.ai/api/v1", model, ApiKeys.OpenRouter, tools, instructions, httpClient)
    {
    }
}
