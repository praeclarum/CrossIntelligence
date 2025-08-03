namespace CrossIntelligence;

public interface IIntelligenceTool
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string input);
}
