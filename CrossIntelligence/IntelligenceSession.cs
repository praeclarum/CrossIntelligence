using Newtonsoft.Json;

namespace CrossIntelligence;

public interface IIntelligenceSessionImplementation : IDisposable
{
    Task<string> RespondAsync(string prompt);
    Task<string> RespondAsync(string prompt, Type responseType);
    // Default implementations delegate to the non-CT overloads for backward compatibility
    // with existing implementations that have not yet been updated.
    Task<string> RespondAsync(string prompt, CancellationToken cancellationToken) => RespondAsync(prompt);
    Task<string> RespondAsync(string prompt, Type responseType, CancellationToken cancellationToken) => RespondAsync(prompt, responseType);
}

public class IntelligenceSession : IDisposable
{
    readonly IIntelligenceSessionImplementation implementation;
    private bool disposed = false;

    public IntelligenceSession(IIntelligenceSessionImplementation implementation)
    {
        this.implementation = implementation;
    }

    public IntelligenceSession(IIntelligenceModel model, IIntelligenceTool[]? tools = null, string instructions = "")
        : this(model.CreateSessionImplementation(tools: tools, instructions: instructions))
    {
    }

    public IntelligenceSession(string modelId, IIntelligenceTool[]? tools = null, string instructions = "")
        : this(IntelligenceModels.FromId(modelId) ?? throw new ArgumentException($"Model ID not found: {modelId}"), tools, instructions)
    {
    }

    public IntelligenceSession(IIntelligenceTool[]? tools = null, string instructions = "")
        : this(IntelligenceModels.Default, tools, instructions)
    {
    }

#if __IOS__ || __MACOS__ || __MACCATALYST__
    public static bool IsAppleIntelligenceAvailable => AppleIntelligenceSessionNative.IsAppleIntelligenceAvailable;
    public static AppleIntelligenceAvailability AppleIntelligenceAvailability => AppleIntelligenceSessionNative.AppleIntelligenceAvailability;
#else
    public static bool IsAppleIntelligenceAvailable => false;
    public static AppleIntelligenceAvailability AppleIntelligenceAvailability => AppleIntelligenceAvailability.PlatformNotSupported;
#endif

    public Task<string> RespondAsync(string prompt)
    {
        return implementation.RespondAsync(prompt);
    }

    public Task<string> RespondAsync(string prompt, CancellationToken cancellationToken)
    {
        return implementation.RespondAsync(prompt, cancellationToken);
    }

    public async Task<object> RespondAsync(string prompt, Type responseType)
    {
        var json = await implementation.RespondAsync(prompt, responseType).ConfigureAwait(false);
        if (JsonConvert.DeserializeObject(json, responseType) is { } result)
        {
            return result;
        }
        throw new Exception($"Failed to deserialize response to type: {responseType.Name}. Response: {json}");
    }

    public async Task<object> RespondAsync(string prompt, Type responseType, CancellationToken cancellationToken)
    {
        var json = await implementation.RespondAsync(prompt, responseType, cancellationToken).ConfigureAwait(false);
        if (JsonConvert.DeserializeObject(json, responseType) is { } result)
        {
            return result;
        }
        throw new Exception($"Failed to deserialize response to type: {responseType.Name}. Response: {json}");
    }

    public async Task<T> RespondAsync<T>(string prompt)
    {
        var r = await RespondAsync(prompt, typeof(T)).ConfigureAwait(false);
        return (T)r;
    }

    public async Task<T> RespondAsync<T>(string prompt, CancellationToken cancellationToken)
    {
        var r = await RespondAsync(prompt, typeof(T), cancellationToken).ConfigureAwait(false);
        return (T)r;
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
                implementation?.Dispose();
            }
            disposed = true;
        }
    }

    ~IntelligenceSession()
    {
        Dispose(false);
    }
}
