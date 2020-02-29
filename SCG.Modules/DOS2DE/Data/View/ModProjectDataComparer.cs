using Alphaleonis.Win32.Filesystem;
using SCG.Controls.Behavior;
using SCG.Data.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View
{

	public class ModProjectDataComparer : ICustomSorter
	{
		public System.ComponentModel.ListSortDirection SortDirection { get; set; }

		public string SortMemberPath { get; set; } = "";

		private static List<string> StringTypes = new List<string>()
		{
			"ModuleInfo.Name",
			"ModuleInfo.Description",
			"ModuleInfo.Type",
			"ModuleInfo.Author",
			"ModuleInfo.Type",
		};

		public int Compare(object x, object y)
		{
			if (x is ModProjectData a && y is ModProjectData b)
			{
				//Log.Here().Activity($"Sorting projects by: {SortMemberPath}");
				if (SortMemberPath == "ModuleInfo.Version")
				{
					return -a.ModuleInfo.Version.CompareTo(b.ModuleInfo.Version);
				}
				else if (SortMemberPath == "ProjectInfo.CreationDate")
				{
					return -a.ProjectInfo.CreationDate.CompareTo(b.ProjectInfo.CreationDate);
				}
				else if (SortMemberPath == "ModuleInfo.ModifiedDate")
				{
					return -a.ModuleInfo.ModifiedDate.CompareTo(b.ModuleInfo.ModifiedDate);
				}
				else if (SortMemberPath == "LastBackup")
				{
					if (a.LastBackup != null && b.LastBackup != null)
					{
						return -a.LastBackup.Value.CompareTo(b.LastBackup.Value);
					}
					else if (a.LastBackup != null)
					{
						return -1;
					}
					else if (b.LastBackup != null)
					{
						return 1;
					}
				}
				else if (StringTypes.Contains(SortMemberPath))
				{
					var dataType = typeof(ModuleInfo);
					string property = SortMemberPath.Replace("ModuleInfo.", "");
					string valueA = (string)dataType.GetProperty(property).GetValue(a.ModuleInfo);
					string valueB = (string)dataType.GetProperty(property).GetValue(b.ModuleInfo);

					//Log.Here().Activity($"Property: {property} = {valueA} | {valueB}");
					if (String.IsNullOrWhiteSpace(valueA))
					{
						return 1;
					}
					else if (String.IsNullOrWhiteSpace(valueB))
					{
						return -1;
					}
					else
					{
						return valueA.CompareTo(valueB);
					}
				}
				else if (SortMemberPath == "Icon")
				{
					if (!a.ThumbnailExists)
					{
						return 1;
					}
					else if (!b.ThumbnailExists)
					{
						return -1;
					}
					else
					{
						return Path.GetFileNameWithoutExtension(a.ThumbnailPath).CompareTo(Path.GetFileNameWithoutExtension(b.ThumbnailPath));
					}
				}
			}
			return 0;
		}
	}
}
