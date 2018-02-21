using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Core;

namespace LL.SCG
{
	public class AddonModule
	{
		private DOS2ProjectController controller;

		public DOS2ProjectController Controller
		{
			get { return controller; }
			set { controller = value; }
		}

		public void Init()
		{
			AppController.RegisterController("Divinity: Original Sin 2", Controller);
		}

		public AddonModule()
		{
			Controller = new DOS2ProjectController();
		}
	}
}
