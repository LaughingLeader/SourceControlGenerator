using SCG.Modules.DOS2DE.Data;
using SCG.Windows;
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

namespace SCG.Modules.DOS2DE.Windows
{
	/// <summary>
	/// Interaction logic for LocaleOptionsWindow.xaml
	/// </summary>
	public partial class LocaleOptionsWindow : HideWindowBase
	{
		public LocaleOptionsWindow() : base()
		{
			
		}

		public void LoadData(LocaleEditorSettingsData data)
		{
			DataContext = data;
		}
	}
}
