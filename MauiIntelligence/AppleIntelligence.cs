namespace MauiIntelligence;

#if __IOS__ || __MACOS__ || __MACCATALYST__
public class AppleIntelligenceModel : IIntelligenceModel
{
    public IIntelligenceSessionImplementation CreateSessionImplementation(string instructions)
    {
        // Create and return an instance of a class that implements IIntelligenceSessionImplementation
        return new AppleIntelligenceSessionImplementation(instructions);
    }
}

public class AppleIntelligenceSessionImplementation : IIntelligenceSessionImplementation
{
    private readonly AppleIntelligenceSessionNative sessionNative;

    public AppleIntelligenceSessionImplementation(string instructions)
    {
        sessionNative = new AppleIntelligenceSessionNative(instructions);
        // Additional initialization with instructions if needed
    }

    public Task<string> RespondAsync(string prompt)
    {
        var tcs = new TaskCompletionSource<string>();

        sessionNative.Respond(prompt, (response, error) =>
        {
            if (error != null)
            {
                tcs.SetException(new Exception(error.LocalizedDescription));
            }
            else
            {
                tcs.SetResult(response);
            }
        });

        return tcs.Task;
    }
}
#endif
