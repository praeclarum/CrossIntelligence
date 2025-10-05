using CrossIntelligence;

namespace Tests;

public class StringResponseTests
{
    const string OpenAIModelId = "openai:gpt-5-mini";
    // const string OpenRouterModelId = "openrouter:google/gemma-3-27b-it:free";
    const string OpenRouterModelId = "openrouter:mistralai/mistral-small-3.1-24b-instruct:free";

    private readonly ITestOutputHelper output;

    public StringResponseTests(ITestOutputHelper testOutputHelper)
    {
        output = testOutputHelper;
    }

    [Theory]
    [InlineData("appleIntelligence")]
    [InlineData(OpenAIModelId)]
    [InlineData(OpenRouterModelId)]
    public async Task SayThisIsATest(string modelId)
    {
        SkipUnlessKeySet(modelId);

        using var session = new IntelligenceSession(modelId);
        var response = await session.RespondAsync("Say \"this is a test\"");
        Assert.NotNull(response);
        Assert.Contains("test", response, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("appleIntelligence")]
    [InlineData(OpenAIModelId)]
    [InlineData(OpenRouterModelId)]
    public async Task GenerateNpc(string modelId)
    {
        SkipUnlessKeySet(modelId);

        var session = new IntelligenceSession(modelId);
        var response = await session.RespondAsync<NonPlayerCharacter>("Generate a random NPC with a name, age, and occupation.");
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Name));
        Assert.InRange(response.Age, 0, 10_000);
        Assert.False(string.IsNullOrWhiteSpace(response.Occupation));
    }

    [Theory]
    [InlineData("appleIntelligence")]
    [InlineData(OpenAIModelId)]
    [InlineData(OpenRouterModelId)]
    public async Task AddNpcs(string modelId)
    {
        SkipUnlessKeySet(modelId);

        var gameDatabase = new GameDatabase();

        var session = new IntelligenceSession(modelId, tools: [new AddPlayerTool(gameDatabase)]);
        Assert.Equal(0, gameDatabase.NpcCount);
        var response = await session.RespondAsync("Add 3 new NPCs to the game. Just make up their information, don't ask me anything.");
        Assert.NotNull(response);
        Assert.Equal(3, gameDatabase.NpcCount);
        foreach (var npc in gameDatabase.Npcs)
        {
            Assert.False(string.IsNullOrWhiteSpace(npc.Name));
            Assert.InRange(npc.Age, 0, 10_000);
            Assert.False(string.IsNullOrWhiteSpace(npc.Occupation));
        }
    }

    [Theory]
    [InlineData("appleIntelligence")]
    [InlineData(OpenAIModelId)]
    [InlineData(OpenRouterModelId)]
    public async Task AddNpcsWithStructuredOutput(string modelId)
    {
        SkipUnlessKeySet(modelId);

        var gameDatabase = new GameDatabase();

        var session = new IntelligenceSession(modelId, tools: [new AddPlayerWithStructuredOutputTool(gameDatabase)]);
        Assert.Equal(0, gameDatabase.NpcCount);
        var response = await session.RespondAsync("Add 3 new NPCs to the game. Just make up their information, don't ask me anything.");
        Assert.NotNull(response);
        Assert.Equal(3, gameDatabase.NpcCount);
        foreach (var npc in gameDatabase.Npcs)
        {
            Assert.False(string.IsNullOrWhiteSpace(npc.Name));
            Assert.InRange(npc.Age, 0, 10_000);
            Assert.False(string.IsNullOrWhiteSpace(npc.Occupation));
        }
    }

    static void SkipUnlessKeySet(string modelId)
    {
        if (modelId.StartsWith("appleIntelligence", StringComparison.OrdinalIgnoreCase))
        {
#if __IOS__ || __MACOS__ || __MACCATALYST__ || __TVOS__
#else
            Assert.Skip("Apple Intelligence is not supported on this platform.");
#endif
            return;
        }
        var parts = modelId.Split(':', 2);
        if (parts.Length != 2)
            throw new ArgumentException("Invalid model ID format. Expected format: 'provider:modelName'");
        var keyPrefix = parts[0].ToUpperInvariant();
        var keyName = keyPrefix + "_API_KEY";
        Assert.SkipUnless(Environment.GetEnvironmentVariable(keyName) is { Length: > 0 }, $"{keyName} environment variable not set");
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

    class AddResults
    {
        public int NumNpcsInDatabase { get; set; }
    }

    class AddPlayerWithStructuredOutputTool(GameDatabase gameDatabase) : IntelligenceTool<NonPlayerCharacter, AddResults>
    {
        public override string Name => "AddPlayer";
        public override string Description => "Adds a new non-player character (NPC) to the game.";
        public override async Task<AddResults> ExecuteAsync(NonPlayerCharacter npc)
        {
            await gameDatabase.AddPlayerAsync(npc);
            return new AddResults { NumNpcsInDatabase = gameDatabase.NpcCount  };
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
