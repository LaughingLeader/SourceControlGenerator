using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SCG.Controls;

/// <summary>
/// Interaction logic for LoadingLoadingSpinner.xaml
/// Source: https://www.engineeringsolutions.de/creating-a-wpf-spinner-control/
/// </summary>
public partial class LoadingSpinner : UserControl
{
	#region Dependency Properties
	#region BallBrush
	public static DependencyProperty BallBrushProperty = DependencyProperty.Register(nameof(BallBrush), typeof(Brush), typeof(LoadingSpinner), new UIPropertyMetadata(Brushes.Blue, PropertyChangedCallback));
	public Brush BallBrush
	{
		get { return (Brush)GetValue(BallBrushProperty); }
		set { SetValue(BallBrushProperty, value); }
	}
	#endregion
	#region Balls
	public static DependencyProperty BallsProperty = DependencyProperty.Register(nameof(Balls), typeof(int), typeof(LoadingSpinner), new UIPropertyMetadata(8, PropertyChangedCallback, CoerceBallsValue));
	public int Balls
	{
		get { return (int)GetValue(BallsProperty); }
		set { SetValue(BallsProperty, value); }
	}
	private static object CoerceBallsValue(DependencyObject d, object baseValue)
	{
		var spinner = (LoadingSpinner)d;
		var value = Convert.ToInt32(baseValue);
		value = Math.Max(1, value);
		value = Math.Min(100, value);
		return value;
	}
	#endregion
	#region BallSize
	public static DependencyProperty BallSizeProperty = DependencyProperty.Register(nameof(BallSize), typeof(double), typeof(LoadingSpinner), new UIPropertyMetadata(20d, PropertyChangedCallback, CoerceBallSizeValue));
	public double BallSize
	{
		get { return (double)GetValue(BallSizeProperty); }
		set { SetValue(BallSizeProperty, value); }
	}
	private static object CoerceBallSizeValue(DependencyObject d, object baseValue)
	{
		var spinner = (LoadingSpinner)d;
		var value = Convert.ToDouble(baseValue);
		value = Math.Max(1, value);
		value = Math.Min(100, value);
		return value;
	}
	#endregion
	private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var spinner = (LoadingSpinner)d;
		spinner.Refresh();
	}
	#endregion

	public LoadingSpinner()
	{
		InitializeComponent();
		SizeChanged += LoadingSpinner_SizeChanged;
		Refresh();
	}

	#region Event Handlers
	private void LoadingSpinner_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		Transform.CenterX = ActualWidth / 2;
		Transform.CenterY = ActualHeight / 2;
		Refresh();
	}

	private void Refresh()
	{
		var n = Balls;
		var size = BallSize;
		canvas.Children.Clear();
		var x = ActualWidth / 2;
		var y = ActualHeight / 2;
		var r = Math.Min(x, y) - size / 2;
		var doubleN = Convert.ToDouble(n);
		for (var i = 1; i <= n; i++)
		{
			var doubleI = Convert.ToDouble(i);
			var x1 = x + Math.Cos(doubleI / doubleN * 2d * Math.PI) * r - size / 2;
			var y1 = y + Math.Sin(doubleI / doubleN * 2d * Math.PI) * r - size / 2;
			var e = new Ellipse
			{
				Fill = BallBrush,
				Opacity = doubleI / doubleN,
				Height = size,
				Width = size
			};
			Canvas.SetLeft(e, x1);
			Canvas.SetTop(e, y1);
			canvas.Children.Add(e);
		};
	}
}
#endregion