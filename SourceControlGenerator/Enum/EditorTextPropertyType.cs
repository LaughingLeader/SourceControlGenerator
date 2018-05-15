using System.ComponentModel;
using SCG.Converters;

namespace SCG.Enum
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