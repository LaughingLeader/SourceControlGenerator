using System.Windows;
using System.Windows.Media;

namespace SCG.Data.View;

public class LogData : ReactiveObject
{
	[Reactive] public DateTimeOffset DateTime { get; set; }
	[Reactive] public int Index { get; set; }
	[Reactive] public string? Message { get; set; }

	[Reactive] public bool IsVisible { get; set; }
	[Reactive] public LogType MessageType { get; set; }

	[ObservableAsProperty] public Brush? BackgroundColor { get; }
	[ObservableAsProperty] public string? Output { get; }

	public static readonly Brush ImportantBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#880080FF"));
	public static readonly Brush ErrorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88FA8072"));
	public static readonly Brush WarningBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88F0E68C"));

	private static Brush LogTypeToBrush(LogType logType)
	{
		return logType switch
		{
			LogType.Important => ImportantBrush,
			LogType.Error => ErrorBrush,
			LogType.Warning => WarningBrush,
			_ => Brushes.Transparent,
		};
	}

	private static string FormatMessage(ValueTuple<int, string?> x)
	{
		var index = x.Item1;
		var message = x.Item2;
		return $"[{index}]: {message ?? string.Empty}";
	}

	public LogData()
	{
		IsVisible = true;
		this.WhenAnyValue(x => x.MessageType).Select(LogTypeToBrush).ToUIProperty(this, x => x.BackgroundColor);
		this.WhenAnyValue(x => x.Index, x => x.Message).Select(FormatMessage).ToUIProperty(this, x => x.Output);
	}
}

public class CollapsibleLogEntry : LogData
{
	public List<LogData> Contents { get; set; }
}
