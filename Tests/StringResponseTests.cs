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
        foreach (var env in System.Environment.GetEnvironmentVariables())
        {
            output.WriteLine(env.ToString());
        }

        using var session = new IntelligenceSession();
        var response = await session.RespondAsync("Say \"this is a test\"");
        Assert.NotNull(response);
        Assert.Contains("test", response, StringComparison.OrdinalIgnoreCase);
    }
}
