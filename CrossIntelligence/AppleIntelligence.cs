namespace CrossIntelligence;

#if __IOS__ || __MACOS__ || __MACCATALYST__
public class AppleIntelligenceModel : IIntelligenceModel
{
    public IIntelligenceSessionImplementation CreateSessionImplementation(IIntelligenceTool[]? tools, string instructions)
    {
        // Create and return an instance of a class that implements IIntelligenceSessionImplementation
        return new AppleIntelligenceSessionImplementation(instructions, tools);
    }
}

class DotnetToolProxy : DotnetTool
{
    public IIntelligenceTool Tool { get; set; }

    public DotnetToolProxy(IIntelligenceTool tool)
    {
        Tool = tool;
    }

    public override string ToolName => Tool.Name;
    public override string ToolDescription => Tool.Description;
}

public class AppleIntelligenceSessionImplementation : IIntelligenceSessionImplementation
{
    private readonly AppleIntelligenceSessionNative sessionNative;

    public AppleIntelligenceSessionImplementation(string instructions, IIntelligenceTool[]? tools)
    {
        var dotnetTools = tools?.Select((tool, index) => new DotnetToolProxy(tool)).ToArray() ?? Array.Empty<DotnetToolProxy>();
        sessionNative = new AppleIntelligenceSessionNative(instructions, dotnetTools);
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
