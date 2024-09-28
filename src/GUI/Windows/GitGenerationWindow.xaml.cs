using SCG.Collections;
using SCG.Data;
using SCG.Interfaces;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCG.Windows;

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
			var list = new ObservableImmutableList<IProjectData>();
			GenerationSettings.SetExportProjects(list);
		}
		else
		{
			GenerationSettings.ExportProjects.Clear();
		}

		SelectedProjects.ForEach((IProjectData project) => GenerationSettings.ExportProjects.Add(project));
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
			var parent = (StackPanel)this.FindName("OptionsPanel");

			var checkBoxes = parent.Children.OfType<CheckBox>().ToList();

			if (checkBox.IsChecked == true)
			{
				var targetVal = false;
				//All true
				if (!checkBoxes.Any(c => c.IsChecked == false))
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
