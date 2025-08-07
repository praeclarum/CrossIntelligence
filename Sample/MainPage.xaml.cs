namespace Sample;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

using CrossIntelligence;

public partial class MainPage : ContentPage
{
	readonly ObservableCollection<Example> examples = new ObservableCollection<Example>();

	public MainPage()
	{
		InitializeComponent();
		examples.Add(new CharacterGeneratorExample());
		examples.Add(new GuidGeneratorExample());
		examples.Add(new ColorSettingExample());
		ExampleList.ItemsSource = examples;
	}

}
