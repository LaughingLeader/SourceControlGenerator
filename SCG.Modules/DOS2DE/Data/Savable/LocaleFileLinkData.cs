using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.Savable
{
	[DataContract]
	public class LocaleFileLinkData : ReactiveObject
	{
		private string sourceFile;

		[DataMember]
		public string SourceFile
		{
			get => sourceFile;
			set { this.RaiseAndSetIfChanged(ref sourceFile, value); }
		}

		private string targetFile;

		[DataMember]
		public string TargetFile
		{
			get => targetFile;
			set { this.RaiseAndSetIfChanged(ref targetFile, value); }
		}

	}
}
