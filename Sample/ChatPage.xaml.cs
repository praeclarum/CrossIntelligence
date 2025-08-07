namespace Sample;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

using CrossIntelligence;

class TranscriptEntry
{
	public required string Text { get; set; }
	public required bool IsUser { get; set; }

	public LayoutOptions HorizontalOptions => IsUser ? LayoutOptions.End : LayoutOptions.Start;
	public Color BackgroundColor => IsUser ? Colors.CornflowerBlue : Color.FromRgb(0x11, 0x11, 0x11);
	public Color TextColor => IsUser ? Colors.White : Colors.White;
}

public partial class ChatPage : ContentPage
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
			if (useAppleIntelligence == value)
				return;
			useAppleIntelligence = value;
			OnPropertyChanged(nameof(UseAppleIntelligence));
			var oldSession = session;
			session = new(
				model: useAppleIntelligence ? IntelligenceModel.AppleIntelligence : IntelligenceModel.OpenAI("gpt-4.1", apiKey: openAIApiKey));
			oldSession.Dispose();
		}
	}

	public ChatPage()
	{
		var s = new Label();
		InitializeComponent();
		session = new(
			model: useAppleIntelligence ? IntelligenceModel.AppleIntelligence : IntelligenceModel.OpenAI("gpt-4.1", apiKey: openAIApiKey));
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
			var response = await session.RespondAsync(prompt);
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
