namespace Sample;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

using CrossIntelligence;

class TranscriptEntry
{
	public required string Text { get; set; }
	public required bool IsUser { get; set; }
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

class StructuredOutput
{
	public required string Response { get; set; }
	public required string Guid { get; set; }
	public override string ToString() => $"Structured: Response={Response}, Guid={Guid}";
}

public partial class MainPage : ContentPage
{
	const string openAIApiKey = "OPENAI_API_KEY"; // Replace with your OpenAI API key
	IntelligenceSession session;
	readonly ObservableCollection<TranscriptEntry> transcript = new ObservableCollection<TranscriptEntry>();
	bool useAppleIntelligence = IntelligenceSession.IsAppleIntelligenceAvailable;

	public bool UseAppleIntelligence
	{
		get => useAppleIntelligence;
		set
		{
			useAppleIntelligence = value;
			OnPropertyChanged(nameof(UseAppleIntelligence));
			var oldSession = session;
			session = new(
				model: useAppleIntelligence ? IntelligenceModel.AppleIntelligence : IntelligenceModel.OpenAI("gpt-4.1", apiKey: openAIApiKey),
				tools: [
					new GuidGenerator()
				]);
			oldSession.Dispose();
		}
	}

	public MainPage()
	{
		InitializeComponent();
		session = new(
			model: useAppleIntelligence ? IntelligenceModel.AppleIntelligence : IntelligenceModel.OpenAI("gpt-4.1", apiKey: openAIApiKey),
			tools: [
				new GuidGenerator()
			]);
		TranscriptList.ItemsSource = transcript;
		BindingContext = this;
	}

	private async void OnIntelligenceClicked(object? sender, EventArgs e)
	{
		if (!IntelligenceSession.IsAppleIntelligenceAvailable)
		{
			transcript.Add(new TranscriptEntry { Text = $"Apple Intelligence not available :-(", IsUser = false });
			return;
		}

		var prompt = PromptEditor.Text?.Trim() ?? string.Empty;
		if (string.IsNullOrEmpty(prompt))
		{
			return;
		}
		PromptEditor.IsEnabled = false;
		IntelligenceBtn.IsEnabled = false;
		try
		{
			transcript.Add(new TranscriptEntry { Text = prompt, IsUser = true });
			var response = await session.RespondAsync<StructuredOutput>(prompt);
			transcript.Add(new TranscriptEntry { Text = response.ToString(), IsUser = false });
			PromptEditor.Text = string.Empty;
		}
		catch (Exception ex)
		{
			transcript.Add(new TranscriptEntry { Text = $"Error: {ex.Message}", IsUser = false });
		}
		finally
		{
			PromptEditor.IsEnabled = true;
			IntelligenceBtn.IsEnabled = true;
			TranscriptList.ScrollTo(transcript.Count - 1);
		}
	}
}
