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
    public override string ArgumentsJsonSchema => Tool.GetArgumentsJsonSchema();
    public override void Execute(string input, Action<string> onDone)
    {
        // Execute the tool asynchronously and invoke the callback with the result
        Task.Run(async () =>
        {
            try
            {
                var result = await Tool.ExecuteAsync(input);
                onDone(result);
            }
            catch (Exception ex)
            {
                // Handle exceptions and pass an error message if needed
                onDone($"Error executing tool: {ex.Message}");
            }
        });
    }
}

public class AppleIntelligenceSessionImplementation : IIntelligenceSessionImplementation
{
    private readonly AppleIntelligenceSessionNative sessionNative;
    private bool disposed = false;

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
            if (error is not null)
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

    public Task<string> RespondAsync(string prompt, Type responseType)
    {
        var tcs = new TaskCompletionSource<string>();
        try
        {
            var schema = responseType.GetJsonSchema();

            sessionNative.Respond(prompt, schema, includeSchemaInPrompt: true, (response, error) =>
            {
                if (error is not null)
                {
                    tcs.SetException(new Exception(error.LocalizedDescription));
                }
                else
                {
                    tcs.SetResult(response);
                }
            });
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }
        return tcs.Task;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                sessionNative?.FreeTools();
            }
            // Free unmanaged resources if any
            sessionNative?.FreeTools();
            disposed = true;
        }
    }

    ~AppleIntelligenceSessionImplementation()
    {
        Dispose(false);
    }
}
#endif
