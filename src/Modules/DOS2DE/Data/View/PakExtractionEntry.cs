using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class PakExtractionEntry : ReactiveObject
	{
		[Reactive] public string Name { get; set; }
		[Reactive] public string FullPath { get; set; }
		[Reactive] public bool IsChecked { get; set; }
	}
}
