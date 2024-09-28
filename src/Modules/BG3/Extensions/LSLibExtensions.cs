using LSLib.LS;

using System.Diagnostics.CodeAnalysis;

namespace SCG.BG3;
public static class LSLibExtensions
{
	private static readonly NodeSerializationSettings _bg3NodeSettings = new()
	{
		DefaultByteSwapGuids = true
	};

	public static bool TryGetNodeById(this Resource resource, string id, [NotNullWhen(true)] out Node? result)
	{
		result = null;
		foreach (var region in resource.Regions.Values)
		{
			foreach (var nodeList in region.Children.Values)
			{
				foreach (var node in nodeList)
				{
					if (node != null && node.Attributes.TryGetValue("id", out var nodeId) && nodeId.AsString(_bg3NodeSettings) == id)
					{
						result = node;
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool TryGetNodeById(this Node parentNode, string id, [NotNullWhen(true)] out Node? result)
	{
		result = null;
		foreach (var nodeList in parentNode.Children.Values)
		{
			foreach (var node in nodeList)
			{
				if (node != null && node.Attributes.TryGetValue("id", out var nodeId) && nodeId.AsString(_bg3NodeSettings) == id)
				{
					result = node;
					return true;
				}
			}
		}
		return false;
	}

	public static string? GetNodeValueById(this Node parentNode, string id, string? nodeType = "attribute")
	{
		foreach (var nodeList in parentNode.Children.Values)
		{
			foreach (var node in nodeList)
			{
				if (node != null)
				{
					if (nodeType == null || node.Name == nodeType)
					{
						if(node.Attributes.TryGetValue("id", out var nodeIdAttribute) && nodeIdAttribute.AsString(_bg3NodeSettings) == id)
						{
							if(node.Attributes.TryGetValue("value", out var nodeValue))
							{
								return nodeValue.AsString(_bg3NodeSettings);
							}
						}
					}
				}
			}
		}
		return null;
	}
}
