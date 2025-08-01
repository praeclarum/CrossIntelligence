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

public partial class MainPage : ContentPage
{
	IntelligenceSession session = new IntelligenceSession();

	readonly ObservableCollection<TranscriptEntry> transcript = new ObservableCollection<TranscriptEntry>();

	public MainPage()
	{
		InitializeComponent();
		TranscriptList.ItemsSource = transcript;
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
			transcript.Add(new TranscriptEntry { Text = response, IsUser = false });
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
