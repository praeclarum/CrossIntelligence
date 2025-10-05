namespace CrossIntelligence;

public interface IIntelligenceTool
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string input);

    string GetArgumentsJsonSchema();
    string GetOutputJsonSchema();
}

public abstract class IntelligenceTool<TArguments> : IIntelligenceTool
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public Task<string> ExecuteAsync(string input)
    {
        var arguments = Newtonsoft.Json.JsonConvert.DeserializeObject<TArguments>(input);
        if (arguments == null)
        {
            throw new ArgumentException("Invalid input for tool execution.");
        }
        return ExecuteAsync(arguments);
    }

    public abstract Task<string> ExecuteAsync(TArguments arguments);

    public virtual string GetArgumentsJsonSchema()
    {
        return typeof(TArguments).GetJsonSchema();
    }

    public virtual string GetOutputJsonSchema()
    {
        return "{\"type\":\"string\"}";
    }
}

public abstract class IntelligenceTool<TArguments, TOutput> : IIntelligenceTool
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public async Task<string> ExecuteAsync(string input)
    {
        var arguments = Newtonsoft.Json.JsonConvert.DeserializeObject<TArguments>(input);
        if (arguments == null)
        {
            throw new ArgumentException("Invalid input for tool execution.");
        }
        var output = await ExecuteAsync(arguments).ConfigureAwait(false);
        return Newtonsoft.Json.JsonConvert.SerializeObject(output, Newtonsoft.Json.Formatting.None);
    }

    public abstract Task<TOutput> ExecuteAsync(TArguments arguments);

    public virtual string GetArgumentsJsonSchema()
    {
        return typeof(TArguments).GetJsonSchema();
    }

    public virtual string GetOutputJsonSchema()
    {
        return typeof(TOutput).GetJsonSchema();
    }
}
