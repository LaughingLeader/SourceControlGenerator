using System;
using System.Collections.Generic;
using System.Linq;
using SCG.Data;
using System.Collections.ObjectModel;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class DOS2DELocalizationViewData:PropertyChangedBase
	{
		public ObservableCollection<DOS2DEStringKeyFileData> ModsLocalization { get; set; }

		public ObservableCollection<DOS2DEStringKeyFileData> PublicLocalization { get; set; }
	}

	public class DOS2DEStringKeyFileData
	{
		public LSLib.LS.Resource Source { get; private set; }

		public List<DOS2DEKeyEntry> Entries { get; set; }

		public DOS2DEStringKeyFileData(LSLib.LS.Resource res)
		{
			Source = res;

			Entries = new List<DOS2DEKeyEntry>();

			try
			{
				var rootNode = res.Regions.First().Value;
				foreach(var entry in rootNode.Children)
				{
					foreach(var node in entry.Value)
					{
						DOS2DEKeyEntry localeEntry = new DOS2DEKeyEntry(node);
						Entries.Add(localeEntry);
					}
					
				}
			}
			finally { }
		}

		private void TraceNodes(KeyValuePair<string, List<LSLib.LS.Node>> keyValuePair, int indent = 0)
		{
			Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Key[{keyValuePair.Key}]|Value[{keyValuePair.Value}]");
			foreach (var next in keyValuePair.Value)
			{
				TraceNode(next, indent + 1);
			}
		}

		private void TraceNode(LSLib.LS.Node v, int indent = 0)
		{
			Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Name[{v.Name}]|{v.GetType()}|Parent: [{v.Parent.Name}]");
			foreach (var att in v.Attributes)
			{
				TraceAtt(att, indent + 1);
			}
			foreach (var child in v.Children)
			{
				TraceNodes(child, indent + 1);
			}
		}

		private void TraceAtt(KeyValuePair<string, LSLib.LS.NodeAttribute> attdict, int indent = 0)
		{
			Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Attribute: {attdict.Key} | {attdict.Value.Type} = {attdict.Value.Value}");
		}
	}

	public class DOS2DEKeyEntry : SCG.Data.PropertyChangedBase
	{
		private LSLib.LS.Node node;

		private LSLib.LS.NodeAttribute uuidNode;
		private LSLib.LS.NodeAttribute contentNode;

		public string Key
		{
			get { return uuidNode != null ? uuidNode.Value.ToString() : ""; }
			set
			{
				node.Attributes["UUID"].Value = value;
				RaisePropertyChanged("Key");
			}
		}

		public string Content
		{
			get { return contentNode != null ? contentNode.Value.ToString() : ""; }
			set
			{
				node.Attributes["Content"].Value = value;
				RaisePropertyChanged("Content");
			}
		}

		public DOS2DEKeyEntry(LSLib.LS.Node resNode)
		{
			node = resNode;

			uuidNode = node.Attributes["UUID"];
			contentNode = node.Attributes["Content"];
		}
	}
}
