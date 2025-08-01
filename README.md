# CrossIntelligence

![NuGet Version](https://img.shields.io/nuget/v/CrossIntelligence)

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
- [ ] Access to OpenAI Models
- [x] Text input and output
- [x] Chat functionality (session history)
- [ ] Tool support
- [ ] Structured output

## Contributing

If you'd like to contribute to the CrossIntelligence library, please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
