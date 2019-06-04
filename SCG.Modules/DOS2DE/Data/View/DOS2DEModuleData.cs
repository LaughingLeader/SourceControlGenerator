using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using SCG.Collections;
using SCG.Commands;
using SCG.Data;
using SCG.Data.View;
using SCG.Modules.DOS2DE.Core;
using SCG.Modules.DOS2DE.Data.App;

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
				manageButtonsText = value;
				RaisePropertyChanged("ManageButtonsText");
			}
		}

		private bool canAddProject = false;

		public bool CanAddProject
		{
			get { return canAddProject; }
			set
			{
				canAddProject = value;
				RaisePropertyChanged("CanAddProject");
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
				canCreatePackages = value;
				RaisePropertyChanged("CanCreatePackages");
			}
		}

		private string availableProjectsToggleText;

		public string AvailableProjectsToggleText
		{
			get { return availableProjectsToggleText; }
			set
			{
				availableProjectsToggleText = value;
				RaisePropertyChanged("AvailableProjectsToggleText");
			}
		}

		private bool availableProjectsVisible = false;

		public bool AvailableProjectsVisible
		{
			get { return availableProjectsVisible; }
			set
			{
				availableProjectsVisible = value;
				RaisePropertyChanged("AvailableProjectsVisible");
			}
		}

		private bool newProjectsAvailable = false;

		public bool NewProjectsAvailable
		{
			get { return newProjectsAvailable; }
			set
			{
				newProjectsAvailable = value;
				RaisePropertyChanged("NewProjectsAvailable");
				RaisePropertyChanged("AvailableProjectsTooltip");

				NoProjectsFoundVisibility = value ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		private Visibility noProjectsFoundVisibility = Visibility.Collapsed;

		public Visibility NoProjectsFoundVisibility
		{
			get { return noProjectsFoundVisibility; }
			set
			{
				noProjectsFoundVisibility = value;
				RaisePropertyChanged("NoProjectsFoundVisibility");
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
				canClickRefresh = value;
				RaisePropertyChanged("CanClickRefresh");
			}
		}

		private ManagedProjectsData managedProjectsData;

		public ManagedProjectsData ManagedProjectsData
		{
			get { return managedProjectsData; }
			set
			{
				managedProjectsData = value;
				RaisePropertyChanged("ManagedProjectsData");
			}
		}

		public ObservableImmutableList<ModProjectData> ManagedProjects { get; set; }

		public ObservableImmutableList<ModProjectData> ModProjects { get; set; }

		public ObservableImmutableList<AvailableProjectViewData> NewProjects { get; set; }

		public object ModProjectsLock { get; private set; } = new object();

		public object ManagedProjectsLock { get; private set; } = new object();

		public object NewProjectsLock { get; private set; } = new object();

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

		private static string DOS2DE_ModuleDisplayName => "Divinity: Original Sin 2 - Definitive Edition";
		private static string DOS2DE_ModuleFolderName => "Divinity Original Sin 2 - Definitive Edition";

		public DOS2DEModuleData() : base(DOS2DE_ModuleDisplayName, DOS2DE_ModuleFolderName)
		{
			ManageButtonsText = DOS2DETooltips.Button_ManageProjects_None;
			AvailableProjectsToggleText = DOS2DETooltips.Button_ToggleAvailableProjects;

			ModProjects = new ObservableImmutableList<ModProjectData>();
			ManagedProjects = new ObservableImmutableList<ModProjectData>();
			NewProjects = new ObservableImmutableList<AvailableProjectViewData>();

			BindingOperations.EnableCollectionSynchronization(ModProjects, ModProjectsLock);
			BindingOperations.EnableCollectionSynchronization(ManagedProjects, ManagedProjectsLock);
			BindingOperations.EnableCollectionSynchronization(NewProjects, NewProjectsLock);
		}
	}
}
