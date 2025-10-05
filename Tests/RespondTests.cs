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

    [Fact]
    public async Task AddNpcs()
    {
        SkipOpenAIUnlessKeySet();

        var gameDatabase = new GameDatabase();

        var session = new IntelligenceSession(tools: [new AddPlayerTool(gameDatabase)]);
        Assert.Equal(0, gameDatabase.NpcCount);
        var response = await session.RespondAsync("Add 3 new NPCs to the game.");
        Assert.NotNull(response);
        Assert.Equal(3, gameDatabase.NpcCount);
        foreach (var npc in gameDatabase.Npcs)
        {
            Assert.False(string.IsNullOrWhiteSpace(npc.Name));
            Assert.InRange(npc.Age, 0, 120);
            Assert.False(string.IsNullOrWhiteSpace(npc.Occupation));
        }
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

    class AddPlayerTool(GameDatabase gameDatabase) : IntelligenceTool<NonPlayerCharacter>
    {
        public override string Name => "AddPlayer";
        public override string Description => "Adds a new non-player character (NPC) to the game.";
        public override async Task<string> ExecuteAsync(NonPlayerCharacter npc)
        {
            await gameDatabase.AddPlayerAsync(npc);
            return $"Added NPC: {npc.Name}.";
        }
    }

    class GameDatabase
    {
        private readonly List<NonPlayerCharacter> _npcs = [];
        public Task AddPlayerAsync(NonPlayerCharacter npc)
        {
            _npcs.Add(npc);
            return Task.CompletedTask;
        }
        public int NpcCount => _npcs.Count;
        public IReadOnlyCollection<NonPlayerCharacter> Npcs => _npcs.AsReadOnly();
    }
}
