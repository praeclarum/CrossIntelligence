namespace MauiIntelligence;

public class OpenAIModel
{
    public readonly string Model;
    public readonly string ApiKey;

    public OpenAIModel(string model, string apiKey)
    {
        Model = model;
        ApiKey = apiKey;
    }

    public IIntelligenceSessionImplementation CreateSessionImplementation(string instructions)
    {
        return new OpenAIIntelligenceSessionImplementation(model: Model, apiKey: ApiKey, instructions: instructions);
    }
}

class OpenAIIntelligenceSessionImplementation : IIntelligenceSessionImplementation
{
    private readonly string model;
    private readonly string apiKey;
    private readonly string instructions;

    public OpenAIIntelligenceSessionImplementation(string model, string apiKey, string instructions)
    {
        this.model = model;
        this.apiKey = apiKey;
        this.instructions = instructions;
    }

    public Task<string> RespondAsync(string prompt)
    {
        // Implement the logic to call OpenAI API with the model and instructions
        // For now, returning a dummy response
        return Task.FromResult($"Response from {model} for prompt: {prompt}");
    }
}
