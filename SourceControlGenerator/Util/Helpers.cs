using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Util.HelperUtil;

namespace LL.SCG
{
	public static class Helpers
	{
		private static RegisteryHelper _registeryHelper;
		public static RegisteryHelper Registry => _registeryHelper;

		private static ImageHelper _imageHelper;
		public static ImageHelper Image => _imageHelper;

		public static void Init()
		{
			_registeryHelper = new RegisteryHelper();
			_imageHelper = new ImageHelper();
		}
	}
}
