namespace Sample;

using System.ComponentModel;

using AudioToolbox;

using CrossIntelligence;

class Example : INotifyPropertyChanged
{
    private string _output = "";
    private string _prompt = "";
    private Color _backgroundColor = Colors.Transparent;

    public string Instructions { get; set; } = "";
    public string Prompt
    {
        get => _prompt;
        set
        {
            _prompt = value;
            OnPropertyChanged(nameof(Prompt));
        }
    }
    public string Output
    {
        get => _output;
        set
        {
            _output = value;
            OnPropertyChanged(nameof(Output));
        }
    }
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            OnPropertyChanged(nameof(BackgroundColor));
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

class CharacterGeneratorExample : Example
{
    public CharacterGeneratorExample()
    {
        Instructions = "You are an RPG character generator for a fantasy game. You generate non-player characters (NPCs) with a name, age, and occupation. Make sure they get a fun fantasy name.";
        Prompt = "Generate a fantasy character with a fun name.";
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

class GuidGeneratorExample : Example
{
    public GuidGeneratorExample()
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
            await Task.Delay(1000); // Simulate some async work
            var guid = Guid.NewGuid();
            return $"Guid Generator finished with output: {guid}";
        }
    }

    class GuidGeneratorArguments
    {
        public string Input { get; set; } = string.Empty;
    }
}
class ColorSettingExample : Example
{
    public ColorSettingExample()
    {
        Instructions = "You are a UI designer capable of changing the style of UI elements. Use the tool setColor whenever the user asks you to change a color. The color should be specified as a standard html color starting with #";
        Prompt = "Set the color to some nice shade of blue";
        Tools = [new ColorSetter(this)];
    }

    class ColorSetter : IntelligenceTool<ColorSetterArguments>
    {
        private readonly ColorSettingExample example;
        public ColorSetter(ColorSettingExample example)
        {
            this.example = example;
        }
        public override string Name => "colorSetter";
        public override string Description => "A tool that sets the color of UI elements.";
        public override async Task<string> ExecuteAsync(ColorSetterArguments input)
        {
            await Task.Delay(30); // Simulate some async work
            try
            {
                var color = Color.Parse(input.HtmlColorString);
                example.BackgroundColor = color;
                return $"Color Setter finished with output: {color}";
            }
            catch (Exception ex)
            {
                return $"Color Setter failed with error: {ex.Message}";
            }
        }
    }

    class ColorSetterArguments
    {
        public string HtmlColorString { get; set; } = string.Empty;
    }
}
