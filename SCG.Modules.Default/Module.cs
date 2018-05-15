using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Core;
using SCG.Interfaces;

namespace SCG.Modules.Default
{
	public class Module : IModuleMain
	{
		private DefaultProjectController Controller { get; set; }

		public void Init()
		{
			AppController.RegisterController("Default", Controller, "Resources/Logos/Git.png", "");
		}

		public Module()
		{
			Controller = new DefaultProjectController();
		}
	}
}
