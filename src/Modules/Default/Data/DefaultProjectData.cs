using SCG.Data.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.Default.Data
{
	public class DefaultProjectData : BaseProjectData
	{
		private string directory;

		public string Directory
		{
			get { return directory; }
			set
			{
				Update(ref directory, value);
			}
		}

	}
}
