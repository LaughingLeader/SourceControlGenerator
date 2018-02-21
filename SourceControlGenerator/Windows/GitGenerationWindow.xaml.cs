using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using LL.SCG.Data;
using LL.SCG.Data.View;
using LL.SCG.Interfaces;

namespace LL.SCG.Windows
{
	/// <summary>
	/// Interaction logic for GitGenerationWindow.xaml
	/// </summary>
	public partial class GitGenerationWindow : Window
	{
		private MainWindow mainWindow;
		private GitGenerationSettings generationSettings;

		public GitGenerationWindow()
		{
			InitializeComponent();
		}

		public void Init(MainWindow ParentWindow, GitGenerationSettings GenerationSettings, List<IProjectData> SelectedProjects)
		{
			mainWindow = ParentWindow;
			generationSettings = GenerationSettings;
			this.DataContext = generationSettings;

			if (GenerationSettings.ExportProjects == null)
			{
				GenerationSettings.ExportProjects = new ObservableCollection<IProjectData>();
			}
			else
			{
				GenerationSettings.ExportProjects.Clear();
			}

			SelectedProjects.ForEach(GenerationSettings.ExportProjects.Add);
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			generationSettings.ExportProjects.Clear();
			this.Close();
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.StartGitGeneration();
			this.Close();
		}
	}
}
