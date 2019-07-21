using DynamicData.Binding;
using SCG.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class LocaleNodeFileData : BaseLocaleFileData
	{
		public LSLib.LS.Resource Source { get; private set; }

		public LSLib.LS.Enums.ResourceFormat Format { get; set; }

		public LocaleNodeFileData(LSLib.LS.Enums.ResourceFormat resourceFormat, LSLib.LS.Resource res, string sourcePath, string name = "") : base(name)
		{
			Source = res;
			SourcePath = sourcePath;
			Format = resourceFormat;
		}
	}
}
