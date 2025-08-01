using CrossIntelligence;

public interface IIntelligenceModel
{
    IIntelligenceSessionImplementation CreateSessionImplementation(string instructions);
}

public abstract class IntelligenceModel : IIntelligenceModel
{
    public abstract IIntelligenceSessionImplementation CreateSessionImplementation(string instructions);

    public static OpenAIModel OpenAI(string model, string apiKey)
    {
        return new OpenAIModel(model, apiKey);
    }

#if __IOS__ || __MACOS__ || __MACCATALYST__
    public static AppleIntelligenceModel AppleIntelligence { get; } = new AppleIntelligenceModel();
#endif
}
