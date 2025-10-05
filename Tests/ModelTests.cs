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

    [Fact]
    public void AppleIntelligenceId()
    {
        Assert.Equal("appleIntelligence", new AppleIntelligenceModel().Id);
    }

    [Fact]
    public void AppleIntelligenceFromId()
    {
        var model = IntelligenceModels.FromId("appleIntelligence");
        Assert.IsType<AppleIntelligenceModel>(model);
    }

    [Fact]
    public void OpenAIId()
    {
        Assert.Equal("openai:gpt-4.1", new OpenAIModel("gpt-4.1").Id);
    }

    [Fact]
    public void OpenAIFromId()
    {
        var model = IntelligenceModels.FromId("openai:gpt-4o");
        Assert.IsType<OpenAIModel>(model);
        var omodel = model as OpenAIModel;
        Assert.Equal("gpt-4o", omodel?.Model);
    }
}
