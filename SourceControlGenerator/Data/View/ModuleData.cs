using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using SCG.Collections;
using SCG.Commands;
using SCG.Interfaces;
using Newtonsoft.Json;
using SCG.Windows;
using SCG.Core;
using ReactiveUI;

namespace SCG.Data.View
{
	public class ModuleData<T> : ReactiveObject, IModuleData where T : IModuleSettingsData
	{
		public string AssemblyName { get; set; }

		public string ModuleName { get; set; }

		public string ModuleFolderName { get; set; }

		private T settings;

		public T Settings
		{
			get { return settings; }
			set
			{
				this.RaiseAndSetIfChanged(ref settings, value);
				this.RaisePropertyChanged("ModuleSettings");
			}
		}

		public IModuleSettingsData GetSettings()
		{
			return Settings;
		}

		public IModuleSettingsData ModuleSettings
		{
			get { return settings; }
		}

		private GitGenerationSettings gitGenerationSettings;

		public GitGenerationSettings GitGenerationSettings
		{
			get { return gitGenerationSettings; }
			set
			{
				this.RaiseAndSetIfChanged(ref gitGenerationSettings, value);
			}
		}

		public ObservableImmutableList<TemplateEditorData> Templates { get; set; }
		public ObservableImmutableList<KeywordData> KeyList { get; set; }

		public object TemplatesLock { get; private set; } = new object();
		public object KeyListLock { get; private set; } = new object();

		private UserKeywordData userKeywordData;

		public UserKeywordData UserKeywords
		{
			get { return userKeywordData; }
			set
			{
				this.RaiseAndSetIfChanged(ref userKeywordData, value);
			}
		}

		private TemplateEditorData newTemplateData;

		public TemplateEditorData NewTemplateData
		{
			get { return newTemplateData; }
			set
			{
				this.RaiseAndSetIfChanged(ref newTemplateData, value);
			}
		}

		private bool projectSelected = false;

		public bool ProjectSelected
		{
			get { return projectSelected; }
			set
			{
				this.RaiseAndSetIfChanged(ref projectSelected, value);
				if (projectSelected)
				{
					OnProjectsSelected();
				}
				else
				{
					OnProjectsDeselected();
				}
			}
		}

		public virtual void OnProjectsSelected() { }

		public virtual void OnProjectsDeselected() { }

		private bool canGenerateGit = false;

		public bool CanGenerateGit
		{
			get { return canGenerateGit; }
			set
			{
				this.RaiseAndSetIfChanged(ref canGenerateGit, value);
			}
		}


		private ActionCommand loadKeywords;

		public ActionCommand LoadKeywords
		{
			get { return loadKeywords; }
			set
			{
				this.RaiseAndSetIfChanged(ref loadKeywords, value);
			}
		}

		private ICommand _addTemplateCommand = null;

		public ICommand AddTemplateCommand
		{
			get
			{
				if (_addTemplateCommand == null) _addTemplateCommand = new ActionCommand(AddTemplate);
				return _addTemplateCommand;
			}
		}

		private ICommand _cancelAddTemplateCommand = null;

		public ICommand CancelTemplateCommand
		{
			get
			{
				if (_cancelAddTemplateCommand == null) _cancelAddTemplateCommand = new ActionCommand(HideAddTemplateControl);
				return _cancelAddTemplateCommand;
			}
		}

		private bool addTemplateControlVisible;

		public bool AddTemplateControlVisible
		{
			get { return addTemplateControlVisible; }
			set
			{
				this.RaiseAndSetIfChanged(ref addTemplateControlVisible, value);
			}
		}

		private ICommand saveSettingsCommand;

		public ICommand SaveSettingsCommand
		{
			get { return saveSettingsCommand; }
			set
			{
				this.RaiseAndSetIfChanged(ref saveSettingsCommand, value);
			}
		}

		private ICommand defaultSettingsCommand;

		public ICommand DefaultSettingsCommand
		{
			get { return defaultSettingsCommand; }
			set
			{
				this.RaiseAndSetIfChanged(ref defaultSettingsCommand, value);
			}
		}

		public void AddTemplate()
		{
			if (NewTemplateData != null)
			{
				Templates.Add(NewTemplateData);
			}
		}

		public void HideAddTemplateControl()
		{
			AddTemplateControlVisible = false;
		}

		public void CreateNewTemplateData()
		{
			newTemplateData = new TemplateEditorData();
			this.RaisePropertyChanged("NewTemplateData");
		}

		public virtual void InitializeSettings()
		{
			if (Settings == null) Settings = (T)Activator.CreateInstance(typeof(T));

			Settings.Init(this);
		}

		public virtual void LoadSettings()
		{
			Settings = JsonConvert.DeserializeObject<T>(File.ReadAllText(DefaultPaths.ModuleSettingsFile(this)));
		}

		public virtual string LoadStringResource(string Name)
		{
			return Properties.Resources.ResourceManager.GetString(Name, Properties.Resources.Culture);
		}

		public event EventHandler OnSettingsReverted;

		public virtual void RevertSettingsToDefault(bool confirmed)
		{
			if (confirmed)
			{
				Settings.SetToDefault(this);
				Settings.RaisePropertyChanged(String.Empty);

				OnSettingsReverted?.Invoke(this, EventArgs.Empty);
			}
		}

		public ModuleData(string moduleName, string moduleFolderName)
		{
			ModuleName = moduleName;
			ModuleFolderName = moduleFolderName;
			AddTemplateControlVisible = false;

			Templates = new ObservableImmutableList<TemplateEditorData>();
			KeyList = new ObservableImmutableList<KeywordData>();

			BindingOperations.EnableCollectionSynchronization(Templates, TemplatesLock);
			BindingOperations.EnableCollectionSynchronization(KeyList, KeyListLock);

			LoadKeywords = new ActionCommand(() => { FileCommands.Load.LoadUserKeywords(this); });

			SaveSettingsCommand = new ActionCommand(() => 
			{
				FileCommands.Save.SaveModuleSettings(this);
				AppController.Main?.SaveAppSettings();
			});
			DefaultSettingsCommand = new TaskCommand(RevertSettingsToDefault, null, "Reset Settings?", "Revert settings to default?", "Changes will be lost.");
		}
	}
}
