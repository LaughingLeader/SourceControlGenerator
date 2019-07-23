using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Core;
using SCG.Interfaces;

namespace SCG
{
	public class Module : IModuleMain
	{
		public DOS2DEProjectController Controller { get; private set; }

		public void Init()
		{
			Controller = new DOS2DEProjectController();
			AppController.RegisterController("Divinity: Original Sin 2 - Definitive Edition", Controller, "Resources/Logos/DivinityOriginalSin2DE.png", "");
		}

		public Module() { }
	}
}
