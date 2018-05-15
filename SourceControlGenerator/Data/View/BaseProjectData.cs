using SCG.Commands;
using SCG.Data;
using SCG.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SCG.Data.View
{

	public class BaseProjectData : PropertyChangedBase, IProjectData
	{
		private SourceControlData gitData;

		public SourceControlData GitData
		{
			get { return gitData; }
			set
			{
				gitData = value;
				RaisePropertyChanged("GitGenerated");
			}
		}

		public bool GitGenerated
		{
			get
			{
				return GitData != null;
			}
		}

		private string projectName;

		public string ProjectName
		{
			get { return projectName; }
			set
			{
				projectName = value;
				RaisePropertyChanged("ProjectName");
			}
		}

		private string displayName;

		public string DisplayName
		{
			get { return displayName; }
			set
			{
				displayName = value;
				RaisePropertyChanged("DisplayName");
			}
		}

		private string uuid;

		public string UUID
		{
			get { return uuid; }
			set
			{
				uuid = value;
				RaisePropertyChanged("UUID");
			}
		}

		private string tooltip;

		public string Tooltip
		{
			get { return tooltip; }
			set
			{
				tooltip = value;
				RaisePropertyChanged("Tooltip");
			}
		}

		private DateTime? lastBackup = null;

		public DateTime? LastBackup
		{
			get
			{
				return lastBackup;
			}
			set
			{
				lastBackup = value;
				RaisePropertyChanged("LastBackup");
				RaisePropertyChanged("LastBackupText");
			}
		}

		public string LastBackupText
		{
			get
			{
				if (LastBackup != null)
				{
					return LastBackup.ToString();
				}
				return "";
			}
		}

		private bool selected = false;

		public bool Selected
		{
			get { return selected; }
			set
			{
				selected = value;
				RaisePropertyChanged("Selected");
			}

		}

		public ICommand OpenBackupFolder { get; private set; }

		public ICommand OpenGitFolder { get; private set; }

		public virtual void Init()
		{
			OpenBackupFolder = new ActionCommand(() => { FileCommands.OpenBackupFolder(this); });
			OpenGitFolder = new ActionCommand(() => { FileCommands.OpenGitFolder(this); });
		}

		public BaseProjectData()
		{
			Init();
		}
	}
}
