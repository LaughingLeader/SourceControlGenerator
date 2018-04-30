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
			this.Hide();
			generationSettings.ExportProjects.Clear();
			mainWindow.OnGitWindowCanceled();
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			this.Hide();
			mainWindow.StartGitGeneration(generationSettings);
		}

		private void OptionsCheckBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (sender is CheckBox checkBox)
			{
				StackPanel parent = (StackPanel)this.FindName("OptionsPanel");

				var checkBoxes = parent.Children.OfType<CheckBox>().ToList();

				if(checkBox.IsChecked == true)
				{
					bool targetVal = false;
					//All true
					if(!checkBoxes.Any(c => c.IsChecked == false))
					{
						targetVal = false;
					}
					else
					{
						targetVal = true;
					}

					foreach (var child in checkBoxes)
					{
						if (child != checkBox) child.IsChecked = targetVal;
					}
				}
				else if (checkBox.IsChecked == false)
				{
					checkBox.IsChecked = true;
					foreach (var child in checkBoxes)
					{
						if (child != checkBox) child.IsChecked = false;
					}
				}
			}
		}
	}
}
