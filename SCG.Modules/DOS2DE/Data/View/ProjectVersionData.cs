using SCG.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class ProjectVersionData : PropertyChangedBase
	{
		private int major = 0;

		public int Major
		{
			get { return major; }
			set
			{
				major = value;
				RaisePropertyChanged("Major");
				UpdateVersion();
			}
		}

		private int minor = 0;

		public int Minor
		{
			get { return minor; }
			set
			{
				minor = value;
				RaisePropertyChanged("Minor");
				UpdateVersion();
			}
		}

		private int revision = 0;

		public int Revision
		{
			get { return revision; }
			set
			{
				revision = value;
				RaisePropertyChanged("Revision");
				UpdateVersion();
			}
		}

		private int build = 0;

		public int Build
		{
			get { return build; }
			set
			{
				build = value;
				RaisePropertyChanged("Build");
				UpdateVersion();
			}
		}

		private string version;

		public string Version
		{
			get { return version; }
			set
			{
				version = value;
				RaisePropertyChanged("Version");
			}
		}

		private int versionInt;

		public int VersionInt
		{
			get { return versionInt; }
			set
			{
				versionInt = value;
				RaisePropertyChanged("VersionInt");
			}
		}


		private void UpdateVersion()
		{
			Version = String.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Build);
		}

		public int ToInt()
		{
			return (Major << 28) + (Minor << 24) + (Revision << 16) + (Build);
		}

		public override string ToString()
		{
			return String.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Build);
		}

		public void ParseInt(int vInt)
		{
			VersionInt = vInt;
			major = (VersionInt >> 28);
			minor = (VersionInt >> 24) & 0x0F;
			revision = (VersionInt >> 16) & 0xFF;
			build = (VersionInt & 0xFFFF);
			UpdateVersion();
		}

		public static ProjectVersionData FromInt(int vInt)
		{
			return new ProjectVersionData(vInt);
		}

		public ProjectVersionData() { }

		public ProjectVersionData(int vInt)
		{
			ParseInt(vInt);
		}
	}
}
