using SCG.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Interfaces
{
	public interface IProjectData : INotifyPropertyChanged
	{
		string ProjectName { get; set; }
		string DisplayName { get; set; }
		string UUID { get; set; }

		SourceControlData GitData { get; set; }
		bool GitGenerated { get; }
		string Tooltip { get; set; }
		DateTime? LastBackup { get; set; }
		bool Selected { get; set; }
	}
}
