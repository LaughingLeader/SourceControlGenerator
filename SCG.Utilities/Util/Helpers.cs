using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Util.HelperUtil;

namespace SCG
{
	public static class Helpers
	{
		private static RegisteryHelper _registeryHelper;
		public static RegisteryHelper Registry => _registeryHelper;

		private static ImageHelper _imageHelper;
		public static ImageHelper Image => _imageHelper;

		private static WebHelper _webHelper;
		public static WebHelper Web => _webHelper;

		public static void Init()
		{
			_registeryHelper = new RegisteryHelper();
			_imageHelper = new ImageHelper();
			_webHelper = new WebHelper();
		}
	}
}
