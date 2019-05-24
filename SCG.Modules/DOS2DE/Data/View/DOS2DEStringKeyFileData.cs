using System;
using System.Collections.Generic;
using System.Linq;
using SCG.Data;
using System.Collections.ObjectModel;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class DOS2DELocalizationViewData : PropertyChangedBase
	{
		public ObservableCollection<DOS2DEStringKeyFileData> ModsLocalization { get; set; }
		public ObservableCollection<DOS2DEStringKeyFileData> PublicLocalization { get; set; }

		public DOS2DELocalizationViewData()
		{
			ModsLocalization = new ObservableCollection<DOS2DEStringKeyFileData>();
			PublicLocalization = new ObservableCollection<DOS2DEStringKeyFileData>();
		}
	}

	public class DOS2DEStringKeyFileData : PropertyChangedBase
	{
		public LSLib.LS.Resource Source { get; private set; }

		public List<DOS2DEKeyEntry> Entries { get; set; }

		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				RaisePropertyChanged("Name");
			}
		}

		public DOS2DEStringKeyFileData(LSLib.LS.Resource res = null, string name = "")
		{
			Entries = new List<DOS2DEKeyEntry>();

			Name = name;

			try
			{
				if (res != null)
				{
					Source = res;

					var rootNode = res.Regions.First().Value;
					foreach (var entry in rootNode.Children)
					{
						foreach (var node in entry.Value)
						{
							DOS2DEKeyEntry localeEntry = new DOS2DEKeyEntry(node);
							Entries.Add(localeEntry);

							//TraceNode(node);
						}

					}
				}
				
			}
			finally { }
		}

		public void Debug_TestEntries()
		{
			for (int i=0;i<4;i++)
			{
				var node = new LSLib.LS.Node();
				var att = new LSLib.LS.NodeAttribute(LSLib.LS.NodeAttribute.DataType.DT_TranslatedString);
				att.Value = new LSLib.LS.TranslatedString();

				var handle = Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h");
				if (att.Value is LSLib.LS.TranslatedString str)
				{
					str.Handle = handle;
					str.Value = "Test";
				}

				node.Attributes.Add("Content", att);
				var entry = new DOS2DEKeyEntry(node);
				Entries.Add(entry);
			}
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
			get { return uuidNode != null ? uuidNode.Value.ToString() : "Key"; }
			set
			{
				if(node != null) node.Attributes["UUID"].Value = value;
				RaisePropertyChanged("Key");
			}
		}

		public string Content
		{
			get { return contentNode != null ? contentNode.Value.ToString() : "Content"; }
			set
			{
				if (node != null) node.Attributes["Content"].Value = value;
				RaisePropertyChanged("Content");
			}
		}

		public string Handle
		{
			get
			{
				if (contentNode != null && contentNode.Value is LSLib.LS.TranslatedString str)
				{
					return str.Handle;
				}
				return "ls::TranslatedStringRepository::s_HandleUnknown";
			}
			set
			{
				if (contentNode != null && contentNode.Value is LSLib.LS.TranslatedString str)
				{
					str.Handle = value;
				}
				RaisePropertyChanged("Handle");
			}
		}

		private bool selected = false;

		public bool Selected
		{
			get { return selected; }
			set
			{
				selected = value;
				RaisePropertyChanged("Selected");
			}
		}


		public DOS2DEKeyEntry(LSLib.LS.Node resNode)
		{
			if(resNode != null)
			{
				node = resNode;

				node.Attributes.TryGetValue("UUID", out uuidNode);
				node.Attributes.TryGetValue("Content", out contentNode);
			}
		}
	}
}
