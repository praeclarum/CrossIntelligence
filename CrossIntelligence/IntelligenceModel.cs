using CrossIntelligence;

public interface IIntelligenceModel
{
    IIntelligenceSessionImplementation CreateSessionImplementation(IIntelligenceTool[]? tools, string instructions);
}

public abstract class IntelligenceModel : IIntelligenceModel
{
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
        apiKey ??= Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        return new OpenAIModel(model, apiKey ?? "");
    }
}
