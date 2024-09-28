namespace SCG.Modules.DOS2DE.Data.View;

public class PakExtractionEntry : ReactiveObject
{
	[Reactive] public string Name { get; set; }
	[Reactive] public string FullPath { get; set; }
	[Reactive] public bool IsChecked { get; set; }
}
