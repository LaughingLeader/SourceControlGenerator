using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Alphaleonis.Win32.Filesystem;

using LSLib.LS.Enums;

using SCG.Data.View;
using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.LocalizationEditor.Views;
using SCG.Modules.DOS2DE.LocalizationEditor.Models;
using SCG.Modules.DOS2DE.LocalizationEditor.Utilities;
using SCG.Modules.DOS2DE.LocalizationEditor.Models.Design;

namespace SCG.Modules.DOS2DE.LocalizationEditor.ViewModels
{
	public class LocaleEditorDesignData : LocaleViewModel
	{

		public LocaleEditorDesignData() : base()
		{
			//var metaFile = new FileInfo(@"G:\Divinity Original Sin 2\DefEd\Data\Mods\Nemesis_627c8d3a-7e6b-4fd2-8ce5-610d553fdbe9\meta.lsx");
			//ModProjectData testData = new ModProjectData(metaFile, @"G:\Divinity Original Sin 2\DefEd\Data\Projects");

			//var result = LoadStringKeyData(@"G:\Divinity Original Sin 2\DefEd\Data\", testData);
			//Data = result.Data;
			//Name = result.Error;

			ModsGroup.DataFiles.Add(new LocaleTestFileData(ModsGroup, "Skills") { ChangesUnsaved = true });
			ModsGroup.DataFiles.Add(new LocaleTestFileData(ModsGroup, "Statuses"));
			for (var i = 1; i < 4; i++)
			{
				ModsGroup.DataFiles.Add(new LocaleTestFileData(ModsGroup, "Potionslalalalala" + i));
			}

			PublicGroup.DataFiles.Add(new LocaleTestFileData(ModsGroup, "Skills"));
			PublicGroup.DataFiles.Add(new LocaleTestFileData(ModsGroup, "Statuses"));
			PublicGroup.DataFiles.Add(new LocaleTestFileData(ModsGroup, "Potions"));

			for (var i = 1; i < 5; i++)
			{
				CustomGroup.DataFiles.Add(new LocaleTestFileData(ModsGroup, "Custom" + i));
			}

			foreach (var d in ModsGroup.DataFiles)
			{
				LocaleEditorCommands.Debug_CreateEntries(d, d.Entries);
			}

			foreach (var d in PublicGroup.DataFiles)
			{
				LocaleEditorCommands.Debug_CreateEntries(d, d.Entries);
				d.ChangesUnsaved = true;
				d.Entries.First().ChangesUnsaved = true;
			}

			foreach (var d in CustomGroup.DataFiles)
			{
				d.CanClose = d.CanRename = true;
				LocaleEditorCommands.Debug_CreateCustomEntries(d, d.Entries);
			}

			UpdateCombinedGroup(true);

			//SelectedGroupIndex = 1;
			//SelectedGroup.SelectedFileIndex = 2;

			System.Diagnostics.Trace.WriteLine("Design data test");

			SelectedGroupIndex = Groups.IndexOf(PublicGroup);
			SelectedEntry = CombinedGroup.Tabs.First().Entries.First();

			LinkedProjects.Add(new ModProjectData() { DisplayName = "TestMod1" });
			LinkedProjects.Add(new ModProjectData() { DisplayName = "TestMod2" });
		}
	}
}
