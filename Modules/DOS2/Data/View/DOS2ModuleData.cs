using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LL.SCG.Collections;
using LL.SCG.Data;
using LL.SCG.Data.View;
using LL.SCG.DOS2.Data.App;

namespace LL.SCG.DOS2.Data.View
{
	public class DOS2ModuleData : ModuleData<DOS2SettingsData>
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

		public DOS2ModuleData() : base("Divinity: Original Sin 2", "DivinityOriginalSin2")
		{
			ManageButtonsText = "Select a Project";
			AvailableProjectsToggleText = "Hide Available Projects";

			ModProjects = new ObservableImmutableList<ModProjectData>();
			ManagedProjects = new ObservableImmutableList<ModProjectData>();
			NewProjects = new ObservableImmutableList<AvailableProjectViewData>();

			BindingOperations.EnableCollectionSynchronization(ModProjects, ModProjectsLock);
			BindingOperations.EnableCollectionSynchronization(ManagedProjects, ManagedProjectsLock);
			BindingOperations.EnableCollectionSynchronization(NewProjects, NewProjectsLock);
		}
	}
}
