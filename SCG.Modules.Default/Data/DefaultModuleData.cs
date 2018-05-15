using SCG.Collections;
using SCG.Data.View;
using SCG.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.Default.Data
{
	public class DefaultModuleData : ModuleData<DefaultSettingsData>
	{
		public ObservableImmutableList<DefaultProjectData> ManagedProjects { get; set; }

		public DefaultModuleData() : base("Default", "Default")
		{
			ManagedProjects = new ObservableImmutableList<DefaultProjectData>();
		}
	}
}
