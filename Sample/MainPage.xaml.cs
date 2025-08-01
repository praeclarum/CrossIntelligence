namespace Sample;

using System.Threading.Tasks;

using MauiIntelligence;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnIntelligenceClicked(object? sender, EventArgs e)
	{
		IntelligenceBtn.Text = $"Apple Intelligence Available: {IntelligenceSession.IsAppleIntelligenceAvailable}...";
		var session = new IntelligenceSession();
		try
		{
			var response = await session.RespondAsync("What is the meaning of life?");
			OutputLabel.Text = $"Response: {response}";
		}
		catch (Exception ex)
		{
			OutputLabel.Text = $"Error: {ex.Message}";
		}
    }

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}
