# MAUI Intelligence

A library to provide access to Apple Intelligence for .NET iOS and macOS applications.

## Installation

You can install the MAUI Intelligence library via NuGet:

```
dotnet add package MauiIntelligence
```

## Usage

To use the MAUI Intelligence library, you need to create an instance of the `IntelligenceSession` class and call its methods.

```csharp
using MauiIntelligence;

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

If you'd like to contribute to the MAUI Intelligence library, please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
