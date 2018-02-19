using System.ComponentModel;
using LL.DOS2.SourceControl.Converters;

namespace LL.DOS2.SourceControl.Enum
{
	[TypeConverter(typeof(EnumDescriptionTypeConverter))]
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