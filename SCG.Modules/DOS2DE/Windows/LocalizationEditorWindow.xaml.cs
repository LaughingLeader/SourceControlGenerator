using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.Utilities;
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
using Alphaleonis.Win32.Filesystem;
using SCG.Windows;
using DataGridExtensions;

namespace SCG.Modules.DOS2DE.Windows
{
	/// <summary>
	/// Interaction logic for LocalizationEditorWindow.xaml
	/// </summary>
	public partial class LocalizationEditorWindow : HideWindowBase
	{

		public DOS2DELocalizationViewData Data { get; set; }

		public LocalizationEditorWindow()
		{
			InitializeComponent();
		}

		public void LoadData(DOS2DELocalizationViewData data)
		{
			//Data = data;
			//DataContext = Data;

			if(DataContext is DOS2DELocalizationViewData currentdata)
			{
				currentdata.Groups = new System.Collections.ObjectModel.ObservableCollection<DOS2DELocalizationGroup>(data.Groups);
				currentdata.UpdateCombinedGroup(true);
				Log.Here().Important($"DataContext is {DataContext.GetType()}");
				Log.Here().Important($"Count {currentdata.Groups.Count}");
			}
			else
			{
				Log.Here().Error($"DataContext is {DataContext.GetType()}");
			}
		}

		private void Entries_SelectAll(object sender, RoutedEventArgs e)
		{
			
		}

		private void Entries_SelectNone(object sender, RoutedEventArgs e)
		{

		}

		private void DataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if(sender is DataGrid grid)
			{
				foreach (var column in grid.Columns)
				{
					column.MinWidth = column.ActualWidth;
					column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
				}
			}
		}
	}
}
