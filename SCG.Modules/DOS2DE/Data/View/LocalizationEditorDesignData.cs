﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Alphaleonis.Win32.Filesystem;
using LSLib.LS.Enums;
using SCG.Data.View;
using SCG.Modules.DOS2DE.Utilities;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class LocalizationEditorDesignData : DOS2DELocalizationViewData
	{

		public LocalizationEditorDesignData() : base()
		{
			//var metaFile = new FileInfo(@"G:\Divinity Original Sin 2\DefEd\Data\Mods\Nemesis_627c8d3a-7e6b-4fd2-8ce5-610d553fdbe9\meta.lsx");
			//ModProjectData testData = new ModProjectData(metaFile, @"G:\Divinity Original Sin 2\DefEd\Data\Projects");

			//var result = LoadStringKeyData(@"G:\Divinity Original Sin 2\DefEd\Data\", testData);
			//Data = result.Data;
			//Name = result.Error;

			ModsGroup.DataFiles.Add(new DOS2DEStringKeyFileDataBase("Skills"));
			ModsGroup.DataFiles.Add(new DOS2DEStringKeyFileDataBase("Statuses"));
			ModsGroup.DataFiles.Add(new DOS2DEStringKeyFileDataBase("Potions"));

			PublicGroup.DataFiles.Add(new DOS2DEStringKeyFileDataBase("Skills"));
			PublicGroup.DataFiles.Add(new DOS2DEStringKeyFileDataBase("Statuses"));
			PublicGroup.DataFiles.Add(new DOS2DEStringKeyFileDataBase("Potions"));

			foreach(var d in ModsGroup.DataFiles)
			{
				DOS2DELocalizationEditor.Debug_CreateEntries(d.Entries);
			}

			foreach (var d in PublicGroup.DataFiles)
			{
				DOS2DELocalizationEditor.Debug_CreateEntries(d.Entries);
			}

			UpdateCombinedGroup(true);
		}
	}
}
