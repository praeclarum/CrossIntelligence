namespace CrossIntelligence;

public interface IIntelligenceTool
{
    string Name { get; }
    string Description { get; }
    string Execute(string input);
}
