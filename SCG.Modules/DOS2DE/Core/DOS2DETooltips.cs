using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Core
{
	public static class DOS2DETooltips
	{
		public static string AvailableProjects = "Detected (unmanaged) mod projects will show up here.\nCTRL + Left Click to deselect projects.";
		public static string AvailableProjects_Availability_New = "New Projects Available";
		public static string AvailableProjects_Availability_None = "No New Projects Found";

		public static string DataGridHeaderShortcuts = "Project Shortcuts";

		public static string Button_ToggleAvailableProjects = "Available Projects";
		public static string Button_RefreshAvailableProjects = "Refresh Available Projects";
		public static string Button_ManageProjects_Single = "Manage Selected Project";
		public static string Button_ManageProjects_Multi = "Manage Selected Projects";
		public static string Button_ManageProjects_None = "Select a Project";
		public static string Button_MarkdownConverter = "Convert Readme with Markdown Converter...";
		public static string Button_PackageMod = "Export selected projects as packages to the Local Mods folder.";
		public static string Button_BackupFolder = "Open Backup Folder...";
		public static string Button_GitFolder = "Open Git Repo Folder...";
		public static string Button_GitFolderDisabled = "Git Repo Not Detected";
		public static string Button_DataFolderParent = "Open Data Folder...";
		public static string Button_DataFolderEditor = "Open Editor Folder...";
		public static string Button_DataFolderMods = "Open Mods Folder...";
		public static string Button_DataFolderProject = "Open Project Folder...";
		public static string Button_DataFolderPublic = "Open Public Folder...";

		public static string Button_BackupSelected = "Backup selected projects to auto-generated project folders.";
		public static string Button_BackupSelectedTo = "Backup selected projects to folder...";
		public static string Button_StartGitGenerator = "Open the Git Generation window for selected projects...";

		//Localization Editor
		public static string Button_Locale_GenerateHandles = "Generate Handles for Selected Entries...\nOnly unset handles will be replaced.";
		public static string Button_Locale_ExportToXML = "Generate Language LSX from Selected...";
		public static string Button_Locale_SaveAll = "Save All Localization Files";
		public static string Button_Locale_SaveCurrent = "Save";
		public static string Button_Locale_SaveCurrent_Disabled = "Save (Disabled - Select a File Tab)";

		public static string Button_Locale_AddFile = "Add Locale File";
		public static string Button_Locale_AddFile_Disabled = "Add Locale File (Disabled for Tabs All & Dialog)";

		public static string Button_Locale_AddKey = "Add Key";
		public static string Button_Locale_AddKey_Disabled = "Add Key (Disabled for Tabs All & Dialog)";

		public static string Button_Locale_DeleteKeys = "Delete Selected Keys";

		public static string Checkbox_Locale_ExportXML_Keys = "Export key names as attributes in each xml node.\nEnable this if you want to keep track of what each handle is for.";
		public static string Checkbox_Locale_ExportXML_Source = "Export the source filenames of handles as attributes in each xml node.\nEnable this if you want to keep track of what each handle is for.";
	}
}
