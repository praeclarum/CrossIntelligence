using CrossIntelligence;

namespace Tests;

public class ApiKeysTests
{
    [Theory]
    [InlineData("openai", "OPENAI_API_KEY")]
    [InlineData("openai:gpt-5-mini", "OPENAI_API_KEY")]
    [InlineData("openrouter", "OPENROUTER_API_KEY")]
    [InlineData("openrouter:meta-llama/llama-4-scout:free", "OPENROUTER_API_KEY")]
    public void ApiKeysInitToEnv(string modelId, string env)
    {
        var envValue = Environment.GetEnvironmentVariable(env) ?? "";
        var apiKey = ApiKeys.GetApiKey(modelId);
        Assert.Equal(envValue, apiKey);
    }


    [Theory]
    [InlineData("openai", "OPENAI_API_KEY")]
    [InlineData("openai:gpt-5-mini", "OPENAI_API_KEY")]
    [InlineData("openrouter", "OPENROUTER_API_KEY")]
    [InlineData("openrouter:meta-llama/llama-4-scout:free", "OPENROUTER_API_KEY")]
    public void ChangingApiKeyDoesntAffectEnv(string modelId, string env)
    {
        var envValueInit = Environment.GetEnvironmentVariable(env) ?? "";
        var apiKeyInit = ApiKeys.GetApiKey(modelId);
        try
        {
            ApiKeys.SetApiKey(modelId, "TEST");
            Assert.Equal("TEST", ApiKeys.GetApiKey(modelId));
            var envValueAfter = Environment.GetEnvironmentVariable(env) ?? "";
            Assert.Equal(envValueInit, envValueAfter);
            Assert.NotEqual(envValueAfter, ApiKeys.GetApiKey(modelId));
        }
        finally
        {
            // Restore to initial state
            ApiKeys.SetApiKey(modelId, apiKeyInit);
        }
    }

    [Fact]
    public void CustomApiKeys()
    {
        Environment.SetEnvironmentVariable("TEST_API_KEY", "test-value");
        Assert.Equal("test-value", ApiKeys.GetApiKey("test"));
    }

    [Fact]
    public void NamedKeysExist()
    {
        Assert.False(ApiKeys.OpenAI is null);
        Assert.False(ApiKeys.OpenRouter is null);
    }
}
