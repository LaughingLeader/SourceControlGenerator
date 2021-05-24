using SCG.Modules.DOS2DE.Data.View;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.List;
using DynamicData;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using System.Windows.Input;
using DynamicData.Binding;
using SCG.Core;
using SCG.Modules.DOS2DE.Utilities;
using System.Threading;
using Alphaleonis.Win32.Filesystem;

namespace SCG.Modules.DOS2DE.ViewModels
{
	public class PakExtractionViewModel
	{
		private readonly SourceList<PakExtractionEntry> _items;

		private readonly ReadOnlyObservableCollection<PakExtractionEntry> _paks;
		public ReadOnlyObservableCollection<PakExtractionEntry> Paks => _paks;

		[Reactive] public bool ExtractInOrder { get; set; } = true;
		[Reactive] public bool ExtractToIndividualFolders { get; set; } = false;
		[Reactive] public string ExportDirectory { get; set; }
		
		public ICommand ExtractPaksCommand { get; private set; }

		private List<string> _extractOrder = new List<string>()
		{
			"Game.pak",
			"GamePlatform.pak",
			"Engine.pak",
			"EngineShaders.pak",
			"Textures.pak",
			"Materials.pak",
			"Icons.pak",
			"LowTex.pak",
			"Minimaps.pak",
			"Effects.pak",
			"Arena.pak",
			"GameMaster.pak",
			"Shared.pak",
			"SharedDOS.pak",
			"SharedSoundBanks.pak",
			"SharedSounds.pak",
			"Origins.pak",
			"Squirrel.pak",
			"Patch1.pak",
			"Patch1_Hotfix1.pak",
			"Patch1_Hotfix2.pak",
			"Patch1_Hotfix4.pak",
			"Patch1_TDE.pak",
			"Patch2.pak",
			"Patch3.pak",
			"Patch4.pak",
			"Patch4-1.pak",
			"Patch5.pak",
			"Patch6.pak",
			"Patch1_Gold.pak",
			"Patch7.pak",
			"Patch7_Hotfix.pak",
			"Patch8.pak",
			"Patch9.pak",
			"Amlatspanish.pak",
			"Chinese.pak",
			"ChineseData.pak",
			"Chinesetraditional.pak",
			"ChinesetraditionalData.pak",
			"Czech.pak",
			"English.pak",
			"French.pak",
			"German.pak",
			"Italian.pak",
			"Japanese.pak",
			"JapaneseData.pak",
			"Korean.pak",
			"KoreanData.pak",
			"Polish.pak",
			"Russian.pak",
		};

		public PakExtractionViewModel()
		{
			var connection = _items.Connect();
			connection.ObserveOn(RxApp.MainThreadScheduler).Bind(out _paks).Subscribe();

			var canExtract = connection.WhenValueChanged(x => x.IsChecked).Any(b => b);
			ExtractPaksCommand = ReactiveCommand.Create(StartExtract, canExtract);
		}

		public void StartExtract()
		{
			var selectedPaks = Paks.Where(x => x.IsChecked).ToList();
			if(ExtractInOrder)
			{
				selectedPaks = selectedPaks.OrderBy(x => _extractOrder.IndexOf(x.Name)).ToList();
			}
			AppController.Main.Data.ProgressValueMax = 100;
			AppController.Main.StartProgressAsync($"Extracing paks... ", async (t) => await ExtractAsync(selectedPaks, t), "", 0, true);
		}

		private async Task<bool> ExtractAsync(List<PakExtractionEntry> selectedPaks,  CancellationToken token)
		{
			foreach(var entry in selectedPaks)
			{
				var exportDir = ExportDirectory;
				if(ExtractToIndividualFolders)
				{
					exportDir = Path.Combine(ExportDirectory, entry.Name);
				}
				AppController.Main.UpdateProgressMessage($"Extracting...");
				AppController.Main.UpdateProgressLog($"Extracting pak {entry.Name}...");
				if (await DOS2DEPackageCreator.ExtractPackageAsync(entry.FullPath, exportDir, token))
				{
					Log.Here().Important($"Successfully extracted pak {entry.Name}.");
					AppController.Main.UpdateProgressLog($"Pak extracted!");
				}
			}
			return true;
		}

		public void Add(PakExtractionEntry entry)
		{
			_items.Add(entry);
		}

		public void Add(IEnumerable<PakExtractionEntry> entries)
		{
			_items.AddRange(entries);
		}

		public void Clear()
		{
			_items.Clear();
		}
	}
}
