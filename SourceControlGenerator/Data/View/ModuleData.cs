﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LL.SCG.Commands;
using LL.SCG.Interfaces;
using Newtonsoft.Json;

namespace LL.SCG.Data.View
{
	public class ModuleData<T> : PropertyChangedBase, IModuleData where T : IModuleSettingsData
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
				settings = value;
				RaisePropertyChanged("Settings");
				RaisePropertyChanged("ModuleSettings");
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
				gitGenerationSettings = value;
				RaisePropertyChanged("GitGenerationSettings");
			}
		}

		public ObservableCollection<TemplateEditorData> Templates { get; set; }
		public ObservableCollection<KeywordData> KeyList { get; set; }

		private UserKeywordData userKeywordData;

		public UserKeywordData UserKeywords
		{
			get { return userKeywordData; }
			set
			{
				userKeywordData = value;
				RaisePropertyChanged("UserKeywords");
			}
		}

		private TemplateEditorData newTemplateData;

		public TemplateEditorData NewTemplateData
		{
			get { return newTemplateData; }
			set
			{
				newTemplateData = value;
				RaisePropertyChanged("NewTemplateData");
			}
		}

		private bool projectSelected = false;

		public bool ProjectSelected
		{
			get { return projectSelected; }
			set
			{
				projectSelected = value;
				RaisePropertyChanged("ProjectSelected");
			}
		}

		private bool canGenerateGit;

		public bool CanGenerateGit
		{
			get { return canGenerateGit; }
			set
			{
				canGenerateGit = value;
				RaisePropertyChanged("CanGenerateGit");
			}
		}


		private ActionCommand loadKeywords;

		public ActionCommand LoadKeywords
		{
			get { return loadKeywords; }
			set
			{
				loadKeywords = value;
				RaisePropertyChanged("LoadKeywords");
			}
		}

		private ICommand _addTemplateCommand = null;

		public ICommand AddTemplateCommand
		{
			get
			{
				if (_addTemplateCommand == null) _addTemplateCommand = new RelayCommand(param => AddTemplate(), null);
				return _addTemplateCommand;
			}
		}

		private ICommand _cancelAddTemplateCommand = null;

		public ICommand CancelTemplateCommand
		{
			get
			{
				if (_cancelAddTemplateCommand == null) _cancelAddTemplateCommand = new RelayCommand(param => HideAddTemplateControl(), null);
				return _cancelAddTemplateCommand;
			}
		}

		private bool addTemplateControlVisible;

		public bool AddTemplateControlVisible
		{
			get { return addTemplateControlVisible; }
			set
			{
				addTemplateControlVisible = value;
				RaisePropertyChanged("AddTemplateControlVisible");
			}
		}

		private ICommand saveSettingsCommand;

		public ICommand SaveSettingsCommand
		{
			get { return saveSettingsCommand; }
			set
			{
				saveSettingsCommand = value;
				RaisePropertyChanged("SaveSettingsCommand");
			}
		}

		private ICommand defaultSettingsCommand;

		public ICommand DefaultSettingsCommand
		{
			get { return defaultSettingsCommand; }
			set
			{
				defaultSettingsCommand = value;
				RaisePropertyChanged("DefaultSettingsCommand");
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
			RaisePropertyChanged("NewTemplateData");
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

			Templates = new ObservableCollection<TemplateEditorData>();
			KeyList = new ObservableCollection<KeywordData>();

			LoadKeywords = new ActionCommand(() => { FileCommands.Load.LoadUserKeywords(this); });

			SaveSettingsCommand = new ActionCommand(() => FileCommands.Save.SaveModuleSettings(this));
			DefaultSettingsCommand = new TaskCommand(RevertSettingsToDefault, null, "Reset Settings?", "Revert settings to default?", "Changes will be lost.");
		}
	}
}
