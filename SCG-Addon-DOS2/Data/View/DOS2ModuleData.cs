using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public ObservableCollection<ModProjectData> ManagedProjects { get; set; }

		public ObservableCollection<ModProjectData> ModProjects { get; set; }

		public ObservableCollection<AvailableProjectViewData> NewProjects { get; set; }

		public DOS2ModuleData() : base("DOS2", "DivinityOriginalSin2")
		{
			ManageButtonsText = "Select a Project";
			AvailableProjectsToggleText = "Hide Available Projects";

			ManagedProjects = new ObservableCollection<ModProjectData>();
			ModProjects = new ObservableCollection<ModProjectData>();
			NewProjects = new ObservableCollection<AvailableProjectViewData>();
		}
	}
}
