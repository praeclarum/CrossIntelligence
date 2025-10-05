using CrossIntelligence;

public interface IIntelligenceModel
{
    string Id { get; }
    IIntelligenceSessionImplementation CreateSessionImplementation(IIntelligenceTool[]? tools, string instructions);
}

public abstract class IntelligenceModel : IIntelligenceModel
{
    public abstract string Id { get; }
    public abstract IIntelligenceSessionImplementation CreateSessionImplementation(IIntelligenceTool[]? tools, string instructions);
}

public static class IntelligenceModels
{
#if __IOS__ || __MACOS__ || __MACCATALYST__
    public static AppleIntelligenceModel AppleIntelligence { get; } = new AppleIntelligenceModel();
    public static IIntelligenceModel Default { get; set; } = AppleIntelligence;
#else
    public static IIntelligenceModel Default { get; set; } = OpenAI("gpt-5-mini");
#endif

    public static OpenAIModel OpenAI(string model, string? apiKey = null)
    {
        return new OpenAIModel(model, apiKey: apiKey);
    }

    public static IIntelligenceModel? FromId(string id)
    {
        if (string.Equals(id, "appleIntelligence", StringComparison.OrdinalIgnoreCase))
        {
            return new AppleIntelligenceModel();
        }
        if (id.StartsWith("openai:", StringComparison.OrdinalIgnoreCase))
        {
            var model = id.Substring("openai:".Length);
            return new OpenAIModel(model);
        }
        if (id.StartsWith("openrouter:", StringComparison.OrdinalIgnoreCase))
        {
            var model = id.Substring("openrouter:".Length);
            return new OpenRouterModel(model);
        }
        return null;
    }
}
