using Alphaleonis.Win32.Filesystem;

using DynamicData.Binding;

using LSLib.LS;
using LSLib.LS.Enums;

using SCG.Data;
using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Modules.DOS2DE.Utilities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models
{
	public class LocaleNodeFileData : BaseLocaleFileData, ILocaleFileData
	{
		public Resource Source { get; private set; }

		public ResourceFormat Format { get; set; }
		public ResourceFormat ExportFormat { get; set; }

		public ModProjectData ModProject { get; set; }

		public Region RootRegion { get; private set; }

		public bool IsCustom => false;

		public LocaleNodeFileData(LocaleTabGroup parent, ResourceFormat resourceFormat,
			Resource res, string sourcePath, string name = "") : base(parent, name)
		{
			Source = res;
			SourcePath = sourcePath;
			Format = resourceFormat;
			CanCreateFileLink = true;

			ExportFormat = Format;

			if (res != null)
			{
				RootRegion = res.Regions.Values.FirstOrDefault();

				if (Format == ResourceFormat.LSF)
				{
					CanRename = false;

					if (RootRegion.Children.TryGetValue("GameObjects", out var nodes))
					{
						if (nodes.FirstOrDefault().Attributes.TryGetValue("Name", out var nameAtt))
						{
							Name = (string)nameAtt.Value;
						}
					}

				}
				else if (Format == ResourceFormat.LSX)
				{
					if (!RootRegion.Name.Contains("Quest"))
					{
						ExportFormat = ResourceFormat.LSB;
					}
					//TraceRegion();
				}
			}

			if (Path.GetFileNameWithoutExtension(SourcePath).IndexOf("Override", StringComparison.OrdinalIgnoreCase) > -1)
			{
				CanOverride = true;
			}
		}

		private void TraceRegion()
		{
			Log.Here().Activity($"Region {RootRegion.Name} | {RootRegion.RegionName}");
			foreach (var attkvp in RootRegion.Attributes)
			{
				Log.Here().Activity($"  Attributes: {attkvp.Key} => {attkvp.Value.Type} => {attkvp.Value.Value}");
			}

			if (RootRegion != null)
			{
				var nodeList = RootRegion.Children.Values.Where(x => x.Count > 0).OrderBy(x => x.Count).FirstOrDefault();

				Log.Here().Activity($"Parsing region for {SourcePath}.");

				foreach (var kvp in RootRegion.Children)
				{
					Log.Here().Activity($"[{kvp.Key}] NodeList: {kvp.Value.Count}");
					for (var i = 0; i < kvp.Value.Count; i++)
					{
						var x = kvp.Value[i];
						Log.Here().Activity($"  [{i}] Node: {x.Name}");
						foreach (var attkvp in x.Attributes)
						{
							Log.Here().Activity($"   Attributes: {attkvp.Key} => {attkvp.Value.Type}");
						}
						foreach (var nodekvp in x.Children)
						{
							Log.Here().Activity($"   Nodes: {nodekvp.Key} => {string.Join(",", nodekvp.Value.Select(n => n.Name))}");
						}
					}
				}
			}
		}
	}
}
