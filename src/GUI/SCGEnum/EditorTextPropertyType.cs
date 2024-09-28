using SCG.Converters;

using System.ComponentModel;

namespace SCG.SCGEnum;

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