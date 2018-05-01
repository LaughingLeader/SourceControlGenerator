using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LL.SCG.Core;
using LL.SCG.Data.View;

namespace LL.SCG.Windows
{
	/// <summary>
	/// Interaction logic for MarkdownConverterWindow.xaml
	/// </summary>
	public partial class MarkdownConverterWindow : HideWindowBase, IToolWindow
	{
		public MarkdownConverterViewData ViewData
		{
			get { return (MarkdownConverterViewData)GetValue(MyPropertyProperty); }
			set { SetValue(MyPropertyProperty, value); }
		}

		public static readonly DependencyProperty MyPropertyProperty =
			DependencyProperty.Register("ViewData", typeof(MarkdownConverterViewData), typeof(MarkdownConverterWindow), new PropertyMetadata(null));

		public MarkdownConverterWindow()
		{
			InitializeComponent();
		}

		public void Init(AppController controller)
		{
			var markdownMenu = controller.Data.MenuBarData.FindByID(MenuID.Markdown);
			if (markdownMenu != null && markdownMenu.InputBinding != null)
			{
				InputBindings.Add(markdownMenu.InputBinding);
			}
		}

		public void SetData(MarkdownConverterViewData viewData = null)
		{
			if(viewData == null)
			{
				viewData = new MarkdownConverterViewData();
			}
			ViewData = viewData;
			ViewData.InitSettings();
			DataContext = ViewData;
		}

		private void Control_InvokeDataSaving(object sender, EventArgs e)
		{
			ViewData.StartSavingAsync();
		}
	}
}
