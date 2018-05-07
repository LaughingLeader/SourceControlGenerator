using System.ComponentModel;
using LL.SCG.Converters;

namespace LL.SCG.Enum
{
	[TypeConverter(typeof(EnumDescriptionConverter))]
	public enum EditorTextPropertyType
	{
		[Description("String")]
		String,
		[Description("File")]
		File,
		[Description("Resource")]
		Resource
	}
}