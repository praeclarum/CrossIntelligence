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
        SkipOpenAIUnlessKeySet();

        using var session = new IntelligenceSession();
        var response = await session.RespondAsync("Say \"this is a test\"");
        Assert.NotNull(response);
        Assert.Contains("test", response, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateNpc()
    {
        SkipOpenAIUnlessKeySet();

        var session = new IntelligenceSession();
        var response = await session.RespondAsync<NonPlayerCharacter>("Generate a random NPC with a name, age, and occupation.");
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Name));
        Assert.InRange(response.Age, 0, 120);
        Assert.False(string.IsNullOrWhiteSpace(response.Occupation));
    }

    static void SkipOpenAIUnlessKeySet()
    {
        Assert.SkipUnless(Environment.GetEnvironmentVariable("OPENAI_API_KEY") is { Length: > 0 }, "OPENAI_API_KEY environment variable not set");
    }

    class NonPlayerCharacter
    {
        public required string Name { get; set; }
        public required int Age { get; set; }
        public required string Occupation { get; set; }
    }
}
