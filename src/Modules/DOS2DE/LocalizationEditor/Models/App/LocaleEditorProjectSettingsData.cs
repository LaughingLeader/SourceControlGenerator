using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models
{

	[DataContract]
	public class LocaleEditorProjectSettingsData : ReactiveObject
	{
		public Action SaveSettings { get; set; }

		[Reactive] public string Name { get; set; }

		[DataMember]
		[Reactive]
		public string FolderName { get; set; }

		[DataMember]
		[Reactive]
		public string LastFileImportPath { get; set; }

		[DataMember]
		[Reactive]
		public string LastEntryImportPath { get; set; }

		[DataMember]
		[Reactive]
		public bool ExportKeys { get; set; } = false;

		[DataMember]
		[Reactive]
		public bool ExportSource { get; set; } = false;

		[DataMember]
		[Reactive]
		public string TargetLanguages { get; set; } = "English";

		[DataMember] public List<string> CustomFiles { get; set; } = new List<string>();

		public LocaleEditorProjectSettingsData()
		{
			this.WhenAnyValue(x => x.ExportKeys, x => x.ExportSource, x => x.TargetLanguages).Subscribe(_ =>
		   {
			   SaveSettings?.Invoke();
		   });
		}
	}
}
