using SCG.Collections;
using SCG.Data;
using SCG.Data.View;
using SCG.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCG.Modules.BG3.Core;
public class BG3ModuleData : ModuleData<BG3SettingsData>
{
	private static string DisplayName => "Baldur's Gate 3";
	private static string FolderName => "Baldurs Gate 3";

	public BG3ModuleData() : base(DisplayName, FolderName)
	{

	}
}
