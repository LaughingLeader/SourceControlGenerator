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
				Update(ref gitData, value);
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
				Update(ref projectName, value);
			}
		}

		private string displayName;

		public string DisplayName
		{
			get { return displayName; }
			set
			{
				Update(ref displayName, value);
			}
		}

		private string uuid;

		public string UUID
		{
			get { return uuid; }
			set
			{
				Update(ref uuid, value);
			}
		}

		private string tooltip;

		public string Tooltip
		{
			get { return tooltip; }
			set
			{
				Update(ref tooltip, value);
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
				Update(ref lastBackup, value);
				Notify("LastBackupText");
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
				Update(ref selected, value);
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
