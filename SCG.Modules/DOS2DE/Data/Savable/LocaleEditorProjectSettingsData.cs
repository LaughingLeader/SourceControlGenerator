using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data
{

	[DataContract]
	public class LocaleEditorProjectSettingsData : ReactiveObject
	{
		public Action SaveSettings { get; set; }

		private string name;

		public string Name
		{
			get => name;
			set { this.RaiseAndSetIfChanged(ref name, value); }
		}

		private string foldername = "";

		[DataMember]
		public string FolderName
		{
			get { return foldername; }
			set
			{
				this.RaiseAndSetIfChanged(ref foldername, value);
			}
		}

		private string lastFileImportPath = "";

		[DataMember]
		public string LastFileImportPath
		{
			get { return lastFileImportPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref lastFileImportPath, value);
			}
		}

		private string lastEntryimportPath = "";

		[DataMember]
		public string LastEntryImportPath
		{
			get { return lastEntryimportPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref lastEntryimportPath, value);
			}
		}

		private bool exportKeys = false;

		[DataMember]
		public bool ExportKeys
		{
			get { return exportKeys; }
			set
			{
				this.RaiseAndSetIfChanged(ref exportKeys, value);
				this.SaveSettings?.Invoke();
			}
		}

		private bool exportSource = false;

		[DataMember]
		public bool ExportSource
		{
			get { return exportSource; }
			set
			{
				this.RaiseAndSetIfChanged(ref exportSource, value);
				this.SaveSettings?.Invoke();
			}
		}

		private string targetLanguages = "English";

		[DataMember]
		public string TargetLanguages
		{
			get { return targetLanguages; }
			set
			{
				this.RaiseAndSetIfChanged(ref targetLanguages, value);
				this.SaveSettings?.Invoke();
			}
		}
	}
}
