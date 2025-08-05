namespace Sample;

using System.ComponentModel;

using AudioToolbox;

using CrossIntelligence;

class Example : INotifyPropertyChanged
{
    private string _output = "";

    public string Instructions { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string Output
    {
        get => _output;
        set
        {
            _output = value;
            OnPropertyChanged(nameof(Output));
        }
    }
    public IIntelligenceTool[] Tools { get; set; } = Array.Empty<IIntelligenceTool>();
    public Type? ResponseType { get; set; } = null;
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public async Task<string> RunAsync()
    {
        Output = "Running...";
        
        using var session = new IntelligenceSession(
            tools: Tools);

        var response = ResponseType is null
            ? await session.RespondAsync(Prompt)
            : await session.RespondAsync(Prompt, ResponseType);

        Output = response.ToString() ?? "";
        return Output;
    }
    public Command RunCommand { get; set; }
    protected Example()
    {
        RunCommand = new Command(async () => await RunAsync());
    }
}

class ToolCallExample : Example
{
    public ToolCallExample()
    {
        Instructions = "You are a helpful assistant that can generate GUIDs.";
        Prompt = "Call the guidGenerator tool with the input Hello World! and report its output";
        Tools = [new GuidGenerator()];
    }

    class GuidGenerator : IntelligenceTool<GuidGeneratorArguments>
    {
        public override string Name => "guidGenerator";
        public override string Description => "A tool that generates GUIDs.";
        public override async Task<string> ExecuteAsync(GuidGeneratorArguments input)
        {
            await Task.Delay(30); // Simulate some async work
            var guid = Guid.NewGuid();
            return $"Guid Generator finished with output: {guid}";
        }
    }

    class GuidGeneratorArguments
    {
        public string Input { get; set; } = string.Empty;
    }
}

class StructuredOutputExample : Example
{
    public StructuredOutputExample()
    {
        Instructions = "You are an RPG character generator for a fantasy game. You generate non-player characters (NPCs) with a name, age, and occupation.";
        Prompt = "Generate a character.";
        ResponseType = typeof(NonPlayerCharacter);
    }

    class NonPlayerCharacter
    {
        public required string Name { get; set; }
        public required int Age { get; set; }
        public required string Occupation { get; set; }
        public override string ToString() => $"NPC: Name={Name}, Age={Age}, Occupation={Occupation}";
    }
}
