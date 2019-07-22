using ReactiveUI;
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

	public class BaseProjectData : ReactiveObject, IProjectData
	{
		private SourceControlData gitData;

		public SourceControlData GitData
		{
			get { return gitData; }
			set
			{
				this.RaiseAndSetIfChanged(ref gitData, value);
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
				this.RaiseAndSetIfChanged(ref projectName, value);
			}
		}

		private string displayName;

		public string DisplayName
		{
			get { return displayName; }
			set
			{
				this.RaiseAndSetIfChanged(ref displayName, value);
			}
		}

		private string uuid;

		public string UUID
		{
			get { return uuid; }
			set
			{
				this.RaiseAndSetIfChanged(ref uuid, value);
			}
		}

		private string tooltip;

		public string Tooltip
		{
			get { return tooltip; }
			set
			{
				this.RaiseAndSetIfChanged(ref tooltip, value);
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
				this.RaiseAndSetIfChanged(ref lastBackup, value);
				this.RaisePropertyChanged("LastBackupText");
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
				this.RaiseAndSetIfChanged(ref selected, value);
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
