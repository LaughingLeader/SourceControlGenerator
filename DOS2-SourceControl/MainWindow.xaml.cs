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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LL.DOS2.SourceControl.Core;

namespace LL.DOS2.SourceControl
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SettingsController _settingsController;

		public SettingsController SettingsController
		{
			get
			{
				return _settingsController;
			}
			set
			{
				_settingsController = value;
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			_settingsController = new SettingsController();

			this.DataContext = SettingsController;

			CollectionViewSource managedProjectsViewSource;
			managedProjectsViewSource = (CollectionViewSource)(FindResource("ManagedProjectsViewSource"));
			managedProjectsViewSource.Source = SettingsController.ManagedProjects;
		}

		private void HandleColumnHeaderSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			if (sizeChangedEventArgs.NewSize.Width <= 60)
			{
				sizeChangedEventArgs.Handled = true;
				//((GridViewColumnHeader)sender).Column.Width = 60;
			}
		}
	}
}
