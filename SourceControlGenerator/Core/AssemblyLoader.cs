using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Core
{
	public class AssemblyLoader : MarshalByRefObject
	{
		object CallInternal(string dll, string typename, string method, object[] parameters)
		{
			Assembly a = Assembly.LoadFile(dll);
			object o = a.CreateInstance(typename);
			Type t = o.GetType();
			MethodInfo m = t.GetMethod(method);
			return m.Invoke(o, parameters);
		}

		public static object Call(AppDomain domain, string dll, string typename, string method, params object[] parameters)
		{
			AssemblyLoader ld = (AssemblyLoader)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AssemblyLoader).FullName);
			object result = ld.CallInternal(dll, typename, method, parameters);
			//AppDomain.Unload(domain);
			return result;
		}
	}
}
