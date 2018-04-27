﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Core;

namespace LL.SCG
{
	public class Module
	{
		private DOS2ProjectController controller;

		public DOS2ProjectController Controller
		{
			get { return controller; }
			set { controller = value; }
		}

		public void Init()
		{
			AppController.RegisterController("Divinity: Original Sin 2", controller, "Resources/Logos/DivinityOriginalSin2.png", "");
		}

		public Module()
		{
			Controller = new DOS2ProjectController();
		}
	}
}