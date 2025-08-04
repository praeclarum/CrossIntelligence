using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace CrossIntelligence;

public interface IIntelligenceSessionImplementation : IDisposable
{
    Task<string> RespondAsync(string prompt);
    Task<string> RespondAsync(string prompt, Type responseType);
}

public class IntelligenceSession : IDisposable
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
        return implementation.RespondAsync(prompt);
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

    public async Task<T> RespondAsync<T>(string prompt)
    {
        var r = await RespondAsync(prompt, typeof(T)).ConfigureAwait(false);
        return (T)r;
    }

    public void Dispose()
    {
        implementation?.Dispose();
    }
}
