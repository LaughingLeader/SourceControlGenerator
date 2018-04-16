using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LL.SCG.Data.View
{
	public class ControlColors : PropertyChangedBase
	{
		private SolidColorBrush background;

		public SolidColorBrush Background
		{
			get { return background; }
			set
			{
				background = value;
				RaisePropertyChanged("Background");
			}
		}

		private SolidColorBrush foreground;

		public SolidColorBrush Foreground
		{
			get { return foreground; }
			set
			{
				foreground = value;
				RaisePropertyChanged("Foreground");
			}
		}

		private SolidColorBrush border;

		public SolidColorBrush Border
		{
			get { return border; }
			set
			{
				border = value;
				RaisePropertyChanged("Border");
			}
		}

		public static ControlColors FromColor(Color? bg = null, Color? fg = null, Color? border = null)
		{
			var bgColor = bg != null ? new SolidColorBrush(bg.Value) : null;
			var fgColor = fg != null ? new SolidColorBrush(fg.Value) : null;
			var borderColor = border != null ? new SolidColorBrush(border.Value) : null;

			return new ControlColors()
			{
				Background = bgColor,
				Foreground = fgColor,
				Border = borderColor
			};
		}

		public static ControlColors FromString(string backgroundString = null, string foregroundString = null, string borderString = null)
		{
			var bgColor = backgroundString != null ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundString)) : null;
			var fgColor = foregroundString != null ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(foregroundString)) : null;
			var borderColor = borderString != null ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderString)) : null;

			return new ControlColors()
			{
				Background = bgColor,
				Foreground = fgColor,
				Border = borderColor
			};
		}
	}

	public class ThemeColorData : PropertyChangedBase
	{
		//GridViewColumnHeader
		//Label
		//ContentControl
		//StackPanel
		//DataGridColumnHeader
		//DataGridRow
		//DataGridCell

		public ControlColors MainWindow { get; set; }

		public ControlColors Border { get; set; }

		public ControlColors TextControl { get; set; }

		public ControlColors Button { get; set; }

		//public ControlColors Grid { get; set; }

		public ControlColors DataGrid { get; set; }

		public ControlColors UserControl { get; set; }

		public ControlColors TabItem { get; set; }

		public ControlColors MenuItem { get; set; }

		public ControlColors ListBox { get; set; }

		public ControlColors ListBoxItem { get; set; }

		public void RefreshTheme()
		{
			RaisePropertyChanged(String.Empty);
		}

		public static ThemeColorData DefaultLight()
		{
			return new ThemeColorData()
			{
				MainWindow = ControlColors.FromString("#FFFFFFFF"),
				//Grid = ControlColors.FromColor(Colors.Transparent),
				DataGrid = ControlColors.FromString("#FFF0F0F0", "#FF000000"),
				Button = ControlColors.FromString("#FFDDDDDD", "#FF000000", "#FF707070"),
				UserControl = ControlColors.FromString(null, "#FF000000")
			};
		}

		public static ThemeColorData DefaultDark()
		{
			string border = "#FFF8F8F2";
			string text = "#FFF8F8F2";
			string background = "#FF2c393e";


			return new ThemeColorData()
			{
				MainWindow = ControlColors.FromString("#FF282A36"),
				DataGrid = ControlColors.FromString(background, text),
				ListBox = ControlColors.FromString(background, text),
				TextControl = ControlColors.FromString(null, text),
				Border = ControlColors.FromString(background, text, border),
				Button = ControlColors.FromString(background, text, border),
				TabItem = ControlColors.FromString(background, text, border),
				MenuItem = ControlColors.FromString(background, text, border),
				ListBoxItem = ControlColors.FromString(null, text),
				UserControl = ControlColors.FromString(null, text)
			};
		}
	}
}
