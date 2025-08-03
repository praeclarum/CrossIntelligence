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
- [ ] Structured output

## Contributing

If you'd like to contribute to the CrossIntelligence library, please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
