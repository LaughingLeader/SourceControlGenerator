using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Core;
using SCG.Interfaces;
using SCG.Modules.Default.Core;

namespace SCG
{
	public class Module : IModuleMain
	{
		public DefaultProjectController Controller { get; private set; }

		public void Init()
		{
#if DEBUG
			//Not ready for release yet (still testing)
			AppController.RegisterController("Default", Controller, "Resources/Logos/Default.png", "");
#endif
		}

		public Module()
		{
			Controller = new DefaultProjectController();
		}
	}
}
