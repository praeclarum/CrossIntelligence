using CrossIntelligence;

namespace Tests;

public class StringResponseTests
{
    private readonly ITestOutputHelper output;

    public StringResponseTests(ITestOutputHelper testOutputHelper)
    {
        output = testOutputHelper;
    }

    [Fact]
    public async Task SayThisIsATest()
    {
        Assert.SkipUnless(Environment.GetEnvironmentVariable("OPENAI_API_KEY") is { Length: > 0 }, "OPENAI_API_KEY environment variable not set");

        using var session = new IntelligenceSession();
        var response = await session.RespondAsync("Say \"this is a test\"");
        Assert.NotNull(response);
        Assert.Contains("test", response, StringComparison.OrdinalIgnoreCase);
    }
}
