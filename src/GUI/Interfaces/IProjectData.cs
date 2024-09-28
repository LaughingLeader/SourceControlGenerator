using SCG.Data;

using System.ComponentModel;

namespace SCG.Interfaces;

public interface IProjectData : INotifyPropertyChanged
{
	string ProjectName { get; set; }
	string ProjectFolder { get; set; }
	string DisplayName { get; set; }
	string UUID { get; set; }

	SourceControlData GitData { get; set; }
	bool GitGenerated { get; }
	string Tooltip { get; set; }
	DateTime? LastBackup { get; set; }
	bool Selected { get; set; }
}
