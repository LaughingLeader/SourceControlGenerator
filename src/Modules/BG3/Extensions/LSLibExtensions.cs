using LSLib.LS;

using System.Diagnostics.CodeAnalysis;

namespace SCG.BG3;
public static class LSLibExtensions
{
	private static readonly NodeSerializationSettings _bg3NodeSettings = new()
	{
		DefaultByteSwapGuids = true
	};

	public static string? GetAttributeValue(this Node node, string id)
	{
		if(node.Attributes.TryGetValue(id, out var attribute))
		{
			return attribute.AsString(_bg3NodeSettings);
		}
		return null;
	}
}
