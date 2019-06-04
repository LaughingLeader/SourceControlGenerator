using SCG.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;

namespace SCG.Modules.DOS2DE.Data.App
{
	public class LocaleEditorSettingsData : PropertyChangedBase
	{
		private string lastImportPath = "";

		public string LastImportPath
		{
			get { return lastImportPath; }
			set
			{
				lastImportPath = value;
				RaisePropertyChanged("LastImportPath");
			}
		}

		public LocaleEditorSettingsData()
		{
			
		}
	}
}
