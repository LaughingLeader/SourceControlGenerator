using System.ComponentModel;
using SCG.Converters;

namespace SCG.SCGEnum
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