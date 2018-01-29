using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.DOS2.SourceControl.Util.HelperUtil;

namespace LL.DOS2.SourceControl
{
	public static class Helpers
	{
		private static RegisteryHelper _registeryHelper;
		public static RegisteryHelper Registry => _registeryHelper;

		private static DOS2Helper _dos2Helper;
		public static DOS2Helper DOS2 => _dos2Helper;

		public static void Init()
		{
			_registeryHelper = new RegisteryHelper();
			_dos2Helper = new DOS2Helper();
		}
	}
}
