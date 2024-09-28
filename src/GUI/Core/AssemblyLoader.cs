using System.Reflection;

namespace SCG.Core;

public class AssemblyLoader : MarshalByRefObject
{
	object CallInternal(string dll, string typename, string method, object[] parameters)
	{
		var a = Assembly.LoadFile(dll);
		var o = a.CreateInstance(typename);
		var t = o.GetType();
		var m = t.GetMethod(method);
		return m.Invoke(o, parameters);
	}

	public static object Call(AppDomain domain, string dll, string typename, string method, params object[] parameters)
	{
		var ld = (AssemblyLoader)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AssemblyLoader).FullName);
		var result = ld.CallInternal(dll, typename, method, parameters);
		//AppDomain.Unload(domain);
		return result;
	}
}
