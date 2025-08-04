namespace CrossIntelligence;

public interface IIntelligenceTool
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string input);

    string GetArgumentsJsonSchema();
    string GetOutputJsonSchema();
}

public abstract class IntelligenceTool<T> : IIntelligenceTool
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public Task<string> ExecuteAsync(string input)
    {
        var arguments = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input);
        if (arguments == null)
        {
            throw new ArgumentException("Invalid input for tool execution.");
        }
        return ExecuteAsync(arguments);
    }

    public abstract Task<string> ExecuteAsync(T arguments);

    public virtual string GetArgumentsJsonSchema()
    {
        return typeof(T).GetJsonSchema();
    }

    public virtual string GetOutputJsonSchema()
    {
        return "{\"type\":\"string\"}";
    }
}
