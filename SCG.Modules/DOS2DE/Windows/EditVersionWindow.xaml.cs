using SCG.Modules.DOS2DE.Data.View;
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
using System.Xml;
using System.Xml.Linq;

namespace SCG.Modules.DOS2DE.Windows
{
	public static class TestProjectData
	{
		public static ModProjectData Data => new ModProjectData(new Alphaleonis.Win32.Filesystem.FileInfo(@"G:\Divinity Original Sin 2\DefEd\Data\Mods\Nemesis_627c8d3a-7e6b-4fd2-8ce5-610d553fdbe9\meta.lsx"), @"G:\Divinity Original Sin 2\DefEd\Data\Projects");
	}
	/// <summary>
	/// Interaction logic for EditVersionWindow.xaml
	/// </summary>
	public partial class EditVersionWindow : HideWindowBase
	{
		public ModProjectData SelectedProject { get; private set; }

		private ProjectVersionData initialVersionData { get; set; }

		public EditVersionWindow()
		{
			InitializeComponent();
		}

		public void LoadData(ModProjectData projectData)
		{
			initialVersionData = new ProjectVersionData(projectData.ModuleInfo.Version);
			SelectedProject = projectData;
			DataContext = SelectedProject;
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			//XPath= /save/region/node/children/node/attribute[id="Version"]
			try
			{
				if(SelectedProject != null)
				{
					var metaDoc = XDocument.Load(SelectedProject.ModMetaFilePath, LoadOptions.PreserveWhitespace);
					var moduleInfoNode = metaDoc.Descendants("node").Where(att => att.Attribute("id").Value == "ModuleInfo").FirstOrDefault();

					if(moduleInfoNode != null)
					{
						var versionNode = moduleInfoNode.Descendants("attribute").Where(att => att.Attribute("id").Value == "Version").FirstOrDefault();
						if (versionNode != null)
						{
							var versionIntString = SelectedProject.VersionData.ToInt().ToString();
							versionNode.Attribute("value").Value = versionIntString;
							Log.Here().Important($"Updated project {SelectedProject.ProjectName} version to {versionIntString} ({SelectedProject.VersionData.ToString()})");

							using (var writer = new XmlTextWriter(SelectedProject.ModMetaFilePath, new UTF8Encoding(false)))
							{
								metaDoc.Save(writer);
							}

							Log.Here().Important($"Saved meta.lsx at \"{SelectedProject.ModMetaFilePath}\"");
						}
					}

					SelectedProject.RaisePropertyChanged("Version");
				}
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error updating project version: {ex.ToString()}");
			}

			Hide();

			SelectedProject = null;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedProject.VersionData.ParseInt(initialVersionData.VersionInt);
			SelectedProject = null;
			Hide();
		}
	}
}
