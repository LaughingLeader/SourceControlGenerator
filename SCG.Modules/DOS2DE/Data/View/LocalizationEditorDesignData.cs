﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Alphaleonis.Win32.Filesystem;
using LSLib.LS.Enums;
using SCG.Modules.DOS2DE.Utilities;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class LocalizationEditorDesignData
	{
		public DOS2DELocalizationViewData Data { get; set; }

		public string Name { get; set; } = "Test";

		public ObservableCollection<DOS2DELocalizationGroup> Groups { get; set; }

		public LocalizationEditorDesignData()
		{
			Data = new DOS2DELocalizationViewData();

			var metaFile = new FileInfo(@"G:\Divinity Original Sin 2\DefEd\Data\Mods\Nemesis_627c8d3a-7e6b-4fd2-8ce5-610d553fdbe9\meta.lsx");
			ModProjectData testData = new ModProjectData(metaFile, @"G:\Divinity Original Sin 2\DefEd\Data\Projects");

			//var result = LoadStringKeyData(@"G:\Divinity Original Sin 2\DefEd\Data\", testData);
			//Data = result.Data;
			//Name = result.Error;

			Name = "Test";
			Data.ModsGroup.DataFiles = new ObservableCollection<DOS2DEStringKeyFileData>();
			Data.ModsGroup.DataFiles.Add(new DOS2DEStringKeyFileData(null, "Skills"));
			Data.ModsGroup.DataFiles.Add(new DOS2DEStringKeyFileData(null, "Statuses"));
			Data.ModsGroup.DataFiles.Add(new DOS2DEStringKeyFileData(null, "Potions"));

			Data.PublicGroup.DataFiles = new ObservableCollection<DOS2DEStringKeyFileData>();
			Data.PublicGroup.DataFiles.Add(new DOS2DEStringKeyFileData(null, "Skills"));
			Data.PublicGroup.DataFiles.Add(new DOS2DEStringKeyFileData(null, "Statuses"));
			Data.PublicGroup.DataFiles.Add(new DOS2DEStringKeyFileData(null, "Potions"));

			foreach(var d in Data.ModsGroup.DataFiles)
			{
				d.Debug_TestEntries();
			}

			foreach (var d in Data.PublicGroup.DataFiles)
			{
				d.Debug_TestEntries();
			}

			Data.UpdateCombinedGroup(true);

			Groups = Data.Groups;
		}
	}
}
