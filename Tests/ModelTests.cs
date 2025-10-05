using CrossIntelligence;

namespace Tests;

public class ModelTests
{
#if __MACOS__ || __IOS__ || __MACCATALYST__ || __TVOS__
    [Fact]
    public void DefaultModelIsAppleIntelligence()
    {
        var model = IntelligenceModels.Default as AppleIntelligenceModel;
        Assert.NotNull(model);
    }
#else
    [Fact]
    public void DefaultModelIsGPT5Mini()
    {
        var model = IntelligenceModels.Default as OpenAIModel;
        Assert.NotNull(model);
    }
#endif
}
