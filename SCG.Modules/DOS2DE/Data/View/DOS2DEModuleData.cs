using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using SCG.Collections;
using SCG.Commands;
using SCG.Core;
using SCG.Data;
using SCG.Data.View;
using SCG.Modules.DOS2DE.Core;
using SCG.Modules.DOS2DE.Data;
using SCG.Modules.DOS2DE.Views;
using System.Windows.Threading;
using System.ComponentModel;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class DOS2DEModuleData : ModuleData<DOS2DESettingsData>
	{
		private string manageButtonsText;

		public string ManageButtonsText
		{
			get { return manageButtonsText; }
			set
			{
				this.RaiseAndSetIfChanged(ref manageButtonsText, value);
			}
		}

		private bool canAddProject = false;

		public bool CanAddProject
		{
			get { return canAddProject; }
			set
			{
				this.RaiseAndSetIfChanged(ref canAddProject, value);
			}
		}

		/*
		public override void OnProjectsSelected()
		{
			base.OnProjectsSelected();

			CanCreatePackages = Settings.DivinePathSet;
		}

		public override void OnProjectsDeselected()
		{
			base.OnProjectsDeselected();

			CanCreatePackages = false;
		}
		*/

		private bool canCreatePackages = false;

		public bool CanCreatePackages
		{
			get { return canCreatePackages; }
			set
			{
				this.RaiseAndSetIfChanged(ref canCreatePackages, value);
			}
		}

		private string availableProjectsToggleText;

		public string AvailableProjectsToggleText
		{
			get { return availableProjectsToggleText; }
			set
			{
				this.RaiseAndSetIfChanged(ref availableProjectsToggleText, value);
			}
		}

		private bool availableProjectsVisible = false;

		public bool AvailableProjectsVisible
		{
			get { return availableProjectsVisible; }
			set
			{
				this.RaiseAndSetIfChanged(ref availableProjectsVisible, value);
			}
		}

		private readonly ObservableAsPropertyHelper<bool> _newProjectsAvailable;

		public bool NewProjectsAvailable => _newProjectsAvailable.Value;

		private Visibility noProjectsFoundVisibility = Visibility.Collapsed;

		public Visibility NoProjectsFoundVisibility
		{
			get { return noProjectsFoundVisibility; }
			set
			{
				this.RaiseAndSetIfChanged(ref noProjectsFoundVisibility, value);
			}
		}

		public string AvailableProjectsTooltip
		{
			get
			{
				return NewProjectsAvailable ? DOS2DETooltips.AvailableProjects_Availability_New : DOS2DETooltips.AvailableProjects_Availability_None;
			}
		}

		private bool canClickRefresh = true;

		public bool CanClickRefresh
		{
			get { return canClickRefresh; }
			set
			{
				this.RaiseAndSetIfChanged(ref canClickRefresh, value);
			}
		}

		private readonly ObservableAsPropertyHelper<bool> openingLocaleEditor;

		public bool OpeningLocaleEditor => openingLocaleEditor.Value;

		private ManagedProjectsData managedProjectsData;

		public ManagedProjectsData ManagedProjectsData
		{
			get => managedProjectsData;
			set
			{
				this.RaiseAndSetIfChanged(ref managedProjectsData, value);
			}
		}

		public readonly SourceList<ModProjectData> ModProjects = new SourceList<ModProjectData>();

		//private readonly ReadOnlyObservableCollection<ModProjectData> _modProjects;
		//public ReadOnlyObservableCollection<ModProjectData> ModProjects => _modProjects;

		
		private readonly ReadOnlyObservableCollection<ModProjectData> _managedProjects;
		public ReadOnlyObservableCollection<ModProjectData> ManagedProjects => _managedProjects;

		private readonly ReadOnlyObservableCollection<ModProjectData> _unmanagedProjects;
		public ReadOnlyObservableCollection<ModProjectData> UnmanagedProjects => _unmanagedProjects;
		

		//public ObservableCollectionExtended<ModProjectData> ManagedProjects { get; set; } = new ObservableCollectionExtended<ModProjectData>();
		//public ObservableCollectionExtended<ModProjectData> UnmanagedProjects { get; set; } = new ObservableCollectionExtended<ModProjectData>();

		override public string LoadStringResource(string Name)
		{
			return Properties.Resources.ResourceManager.GetString(Name, Properties.Resources.Culture);
		}

		public void UpdateManageButtonsText(int selectedCount = 0)
		{
			CanAddProject = selectedCount != 0;

			if (selectedCount > 1)
			{
				ManageButtonsText = DOS2DETooltips.Button_ManageProjects_Multi;
			}
			else if (selectedCount == 1)
			{
				ManageButtonsText = DOS2DETooltips.Button_ManageProjects_Single;
			}
			else
			{
				ManageButtonsText = DOS2DETooltips.Button_ManageProjects_None;
			}
		}

		private Visibility loadPanelVisibility = Visibility.Collapsed;

		public Visibility LoadPanelVisibility
		{
			get => loadPanelVisibility;
			set { this.RaiseAndSetIfChanged(ref loadPanelVisibility, value); }
		}

		private static string DOS2DE_ModuleDisplayName => "Divinity: Original Sin 2 - Definitive Edition";
		private static string DOS2DE_ModuleFolderName => "Divinity Original Sin 2 - Definitive Edition";

		public ICommand OpenBackupFolderCommand { get; private set; }
		public ICommand OpenGitFolderCommand { get; private set; }
		public ICommand OpenModsFolderCommand { get; private set; }
		public ICommand OpenPublicFolderCommand { get; private set; }
		public ICommand OpenEditorFolderCommand { get; private set; }
		public ICommand OpenProjectFolderCommand { get; private set; }
		public ICommand EditProjectVersionCommand { get; private set; }
		public ICommand RefreshAllCommand { get; internal set; }
		public ReactiveCommand<ModProjectData, Unit> OpenInLocalizationEditorCommand { get; private set; }

		public DOS2DEModuleData() : base(DOS2DE_ModuleDisplayName, DOS2DE_ModuleFolderName)
		{
			ManageButtonsText = DOS2DETooltips.Button_ManageProjects_None;
			AvailableProjectsToggleText = DOS2DETooltips.Button_ToggleAvailableProjects;

			var sortOrder = SortExpressionComparer<ModProjectData>.Ascending(m => m.DisplayName);
			
			var connection = ModProjects.Connect().AutoRefreshOnObservable(x => x.WhenPropertyChanged(p => p.IsManaged)).
				Buffer(TimeSpan.FromMilliseconds(100)).FlattenBufferResult().ObserveOn(RxApp.MainThreadScheduler).
				Sort(sortOrder);

			connection.Filter(m => m.IsManaged).Bind(out _managedProjects).Subscribe();
			connection.Filter(m => !m.IsManaged).Bind(out _unmanagedProjects).Subscribe();
	
			var conn = this.WhenAnyValue(vm => vm.UnmanagedProjects.Count, (count) => count > 0).
				ObserveOnDispatcher(DispatcherPriority.Background);
			conn.Subscribe((b) =>
			{
				this.RaisePropertyChanged("AvailableProjectsTooltip");
				NoProjectsFoundVisibility = b ? Visibility.Collapsed : Visibility.Visible;
			});
			conn.ToProperty(this, x => x.NewProjectsAvailable, out _newProjectsAvailable);

			OpenBackupFolderCommand = ReactiveCommand.Create<ModProjectData>(DOS2DECommands.OpenBackupFolder);
			OpenGitFolderCommand = ReactiveCommand.Create<ModProjectData>(DOS2DECommands.OpenGitFolder);
			OpenModsFolderCommand = ReactiveCommand.Create<ModProjectData>(DOS2DECommands.OpenModsFolder);
			OpenPublicFolderCommand = ReactiveCommand.Create<ModProjectData>(DOS2DECommands.OpenPublicFolder);
			OpenEditorFolderCommand = ReactiveCommand.Create<ModProjectData>(DOS2DECommands.OpenEditorFolder);
			OpenProjectFolderCommand = ReactiveCommand.Create<ModProjectData>(DOS2DECommands.OpenProjectFolder);

			EditProjectVersionCommand = ReactiveCommand.Create<ModProjectData>(DOS2DEProjectsView.EditProjectVersion);

			OpenInLocalizationEditorCommand = ReactiveCommand.CreateFromTask<ModProjectData, Unit>(DOS2DEProjectsView.OpenLocalizationEditorForProject);

			OpenInLocalizationEditorCommand.IsExecuting.ToProperty(this, x => x.OpeningLocaleEditor, out openingLocaleEditor);
			OpenInLocalizationEditorCommand.ThrownExceptions.Subscribe(ex => Log.Here().Error("Error opening Localization Editor: ", ex.ToString()));

			//this.WhenAnyValue(x => x.CanClickRefresh).ToProperty(this, x => x.CanExecuteRefresh, out canExecuteRefresh);

			/*
			var modlistConnection = ModProjectsSource.Connect().Sort(sortOrder).
					ObserveOnDispatcher().Bind(out _modProjects).DisposeMany().
				Publish().
					Filter(m => m.IsManaged == true).Bind(out _managedProjects).DisposeMany().
				Publish().
					Filter(m => !m.IsManaged).Sort(sortOrder).
					ObserveOnDispatcher().Transform(m => new AvailableProjectViewData() { Name = m.ProjectName, Tooltip = m.Tooltip }).
					Bind(out _newProjects).DisposeMany().Subscribe();
			*/
		}
	}
}
