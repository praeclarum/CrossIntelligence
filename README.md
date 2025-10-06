# CrossIntelligence

[![Build](https://github.com/praeclarum/CrossIntelligence/actions/workflows/build.yml/badge.svg)](https://github.com/praeclarum/CrossIntelligence/actions/workflows/build.yml) [![NuGet Version](https://img.shields.io/nuget/v/CrossIntelligence)](https://www.nuget.org/packages/CrossIntelligence)

A library to provide access to Apple Intelligence and other LLMs for .NET and MAUI applications.


## Installation

You can install the CrossIntelligence library via NuGet:

```bash
dotnet add package CrossIntelligence
```


## Usage

To use the CrossIntelligence library, you need to create an instance of the `IntelligenceSession` class and call its methods.

```csharp
using CrossIntelligence;

var session = new IntelligenceSession();
var response = await session.RespondAsync("What is the meaning of life?");
Console.WriteLine(response);
```

## Features

- [x] Access to Apple Intelligence System Language Model
- [x] Access to OpenAI Models
- [x] Prompt text input
- [x] Text output
- [x] System instructions
- [x] Chat functionality (session history)
- [x] Tool/function support
- [x] Structured output

### Structured Output

You can define a structured output by creating a class with properties that match the expected output format. The library will automatically deserialize the response into your class.

```csharp
class NonPlayerCharacter
{
    public required string Name { get; set; }
    public required int Age { get; set; }
    public required string Occupation { get; set; }
}

var session = new IntelligenceSession();
var response = await session.RespondAsync<NonPlayerCharacter>("Generate a random NPC with a name, age, and occupation.");
Console.WriteLine($"Name: {response.Name}, Age: {response.Age}, Occupation: {response.Occupation}");
```

### Tools (Functions)

You can define tools (functions) that the LLM can call to perform specific tasks. Define a class the inherits from `IntelligenceTool` and implement the `ExecuteAsync` method. Tools take arguments that are specified using a generic type parameter.

```csharp
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

var gameDatabase = new GameDatabase();
var session = new IntelligenceSession(tools: [new AddPlayerTool(gameDatabase)]);
var response = await session.RespondAsync("Add 3 new NPCs to the game.");
Console.WriteLine(response);
```

### Calling External Models

You can use other models than the default system model by passing in the `model` parameter when creating the `IntelligenceSession`.

```csharp
ApiKeys.OpenAI = "OPENAI_API_KEY"; // Set your OpenAI API key here
var session = new IntelligenceSession(IntelligenceModels.OpenAI("gpt-5-mini"));
var response = await session.RespondAsync("What is the meaning of life?");
Console.WriteLine(response);
```

### API Keys

API keys can be set using the static `ApiKeys` class. These are in-memory only and not persisted.


## Testing

Some tests require API keys to run. Pass the OpenAI API and OpenRouter keys as environment variables before running the tests.

```bash
dotnet test -c Release -e OPENAI_API_KEY=$OPENAI_API_KEY -e OPENROUTER_API_KEY=$OPENROUTER_API_KEY Tests/Tests.csproj
```


## Contributing

If you'd like to contribute to the CrossIntelligence library, please fork the repository and submit a pull request.


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
