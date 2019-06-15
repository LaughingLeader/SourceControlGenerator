using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SCG.Collections;
using SCG.Commands;
using SCG.Data;
using SCG.Data.View;
using SCG.Modules.DOS2.Core;
using SCG.Modules.DOS2.Data.App;

namespace SCG.Modules.DOS2.Data.View
{
	public class DOS2ModuleData : ModuleData<DOS2SettingsData>
	{
		private string manageButtonsText;

		public string ManageButtonsText
		{
			get { return manageButtonsText; }
			set
			{
				Update(ref manageButtonsText, value);
			}
		}

		private bool canAddProject = false;

		public bool CanAddProject
		{
			get { return canAddProject; }
			set
			{
				Update(ref canAddProject, value);
			}
		}


		private string availableProjectsToggleText;

		public string AvailableProjectsToggleText
		{
			get { return availableProjectsToggleText; }
			set
			{
				Update(ref availableProjectsToggleText, value);
			}
		}

		private bool availableProjectsVisible = false;

		public bool AvailableProjectsVisible
		{
			get { return availableProjectsVisible; }
			set
			{
				Update(ref availableProjectsVisible, value);
			}
		}

		private bool newProjectsAvailable = false;

		public bool NewProjectsAvailable
		{
			get { return newProjectsAvailable; }
			set
			{
				Update(ref newProjectsAvailable, value);
				Notify("AvailableProjectsTooltip");
			}
		}

		public string AvailableProjectsTooltip
		{
			get
			{
				return NewProjectsAvailable ? "New Projects Available" : "No New Projects Found";
			}
		}


		private bool canClickRefresh = true;

		public bool CanClickRefresh
		{
			get { return canClickRefresh; }
			set
			{
				Update(ref canClickRefresh, value);
			}
		}


		private ManagedProjectsData managedProjectsData;

		public ManagedProjectsData ManagedProjectsData
		{
			get { return managedProjectsData; }
			set
			{
				Update(ref managedProjectsData, value);
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
				ManageButtonsText = "Manage Selected Projects";
			}
			else if (selectedCount == 1)
			{
				ManageButtonsText = "Manage Selected Project";
			}
			else
			{
				if (NewProjects.Count > 0)
				{
					ManageButtonsText = "Select a Project";
				}
				else
				{
					//ManageButtonsText = "No New Projects Found";
					ManageButtonsText = "Select a Project";
				}
			}
		}

		public DOS2ModuleData() : base("Divinity: Original Sin 2", "Divinity Original Sin 2")
		{
			ManageButtonsText = "Select a Project";
			AvailableProjectsToggleText = "Available Projects";

			ModProjects = new ObservableImmutableList<ModProjectData>();
			ManagedProjects = new ObservableImmutableList<ModProjectData>();
			NewProjects = new ObservableImmutableList<AvailableProjectViewData>();

			BindingOperations.EnableCollectionSynchronization(ModProjects, ModProjectsLock);
			BindingOperations.EnableCollectionSynchronization(ManagedProjects, ManagedProjectsLock);
			BindingOperations.EnableCollectionSynchronization(NewProjects, NewProjectsLock);
		}
	}
}
