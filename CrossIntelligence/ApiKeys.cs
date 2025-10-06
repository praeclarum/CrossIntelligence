namespace CrossIntelligence;

/// <summary>
/// In-memory API key storage and retrieval.
/// Values can be set programmatically or read from environment variables.
/// </summary>
public static class ApiKeys
{
    private static readonly Dictionary<string, string> inMemoryKeys = new();

    public static string GetApiKey(string? modelId)
    {
        if (GetKeyName(modelId) is not string keyName)
        {
            return "";
        }
        if (inMemoryKeys.TryGetValue(keyName, out var key))
        {
            return key;
        }
        return Environment.GetEnvironmentVariable(keyName) ?? "";
    }

    public static void SetApiKey(string modelId, string apiKey)
    {
        if (GetKeyName(modelId) is not string keyName)
        {
            throw new ArgumentException("Invalid model ID format. Expected format: 'provider:modelName'");
        }
        inMemoryKeys[keyName] = apiKey;
    }

    public static string OpenAI
    {
        get => GetApiKey("openai");
        set => SetApiKey("openai", value);
    }

    public static string OpenRouter
    {
        get => GetApiKey("openrouter");
        set => SetApiKey("openrouter", value);
    }

    private static string? GetKeyName(string? modelId)
    {
        var prefix = (modelId ?? "").Split(':')[0].ToUpperInvariant();
        if (string.IsNullOrEmpty(prefix))
        {
            return null;
        }
        return prefix + "_API_KEY";
    }
}
