using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		override public string LoadStringResource(string Name)
		{
			return Properties.Resources.ResourceManager.GetString(Name, Properties.Resources.Culture);
		}

		public DOS2ModuleData() : base("Divinity: Original Sin 2", "DivinityOriginalSin2")
		{
			ManageButtonsText = "Select a Project";
			AvailableProjectsToggleText = "Hide Available Projects";

			ManagedProjects = new ObservableImmutableList<ModProjectData>();
			ModProjects = new ObservableImmutableList<ModProjectData>();
			NewProjects = new ObservableImmutableList<AvailableProjectViewData>();
		}
	}
}
