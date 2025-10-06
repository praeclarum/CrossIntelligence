// ReSharper disable InconsistentNaming

namespace CrossIntelligence;

public class OpenAIModel : IIntelligenceModel
{
    public const string DefaultModel = "gpt-5-mini";

    public readonly string Model;
    
    public string Id => $"openai:{Model}";

    public OpenAIModel(string model)
    {
        Model = model;
    }

    public IIntelligenceSessionImplementation CreateSessionImplementation(IIntelligenceTool[]? tools, string instructions)
    {
        return new OpenAISessionImplementation(model: Model, tools: tools, instructions: instructions);
    }
}

class OpenAISessionImplementation : ResponsesApiSessionImplementation
{
    public OpenAISessionImplementation(string model, IIntelligenceTool[]? tools, string instructions, HttpClient? httpClient = null)
        : base("https://api.openai.com/v1", model, ApiKeys.OpenAI, tools, instructions, httpClient)
    {
    }
}
