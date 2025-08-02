namespace CrossIntelligence;

public interface IIntelligenceSessionImplementation
{
    Task<string> RespondAsync(string prompt);
}

public class IntelligenceSession
{
    readonly IIntelligenceSessionImplementation implementation;

    public IntelligenceSession(IIntelligenceSessionImplementation implementation)
    {
        this.implementation = implementation;
    }

    public IntelligenceSession(IIntelligenceModel model, IIntelligenceTool[]? tools = null, string instructions = "")
        : this(model.CreateSessionImplementation(tools: tools, instructions: instructions))
    {
    }

#if __IOS__ || __MACOS__ || __MACCATALYST__
    public IntelligenceSession(IIntelligenceTool[]? tools = null, string instructions = "")
        : this(IntelligenceModel.AppleIntelligence, tools, instructions)
    {
    }
#endif

#if __IOS__ || __MACOS__ || __MACCATALYST__
    public static bool IsAppleIntelligenceAvailable => AppleIntelligenceSessionNative.IsAppleIntelligenceAvailable;
#else
    public static bool IsAppleIntelligenceAvailable => false;
#endif

    public Task<string> RespondAsync(string prompt)
    {
        // Call the implementation's method to get a response
        return implementation.RespondAsync(prompt);
    }
}
