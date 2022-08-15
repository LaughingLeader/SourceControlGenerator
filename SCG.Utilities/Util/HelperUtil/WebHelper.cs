using System;

namespace SCG.Util.HelperUtil
{
	public class WebHelper
	{
		public bool IsValidUri(string uri)
		{
			if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
				return false;
			Uri tmp;
			if (!Uri.TryCreate(uri, UriKind.Absolute, out tmp))
				return false;
			return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
		}

		public bool OpenUri(string uri)
		{
			if (!IsValidUri(uri))
				return false;
			System.Diagnostics.Process.Start(uri);
			return true;
		}
	}
}
