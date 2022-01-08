using System;
using System.Collections.Generic;
using System.Linq;
using SCG.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SCG.Commands;
using System.Windows;
using SCG.Data.View;
using LSLib.LS;
using SCG.Modules.DOS2DE.Windows;
using SCG.Modules.DOS2DE.Core;
using Alphaleonis.Win32;
using Alphaleonis.Win32.Filesystem;
using SCG.Core;
using System.ComponentModel;
using SCG.Extensions;
using DynamicData.Binding;
using ReactiveUI;
using System.Reactive;
using System.Windows.Media;
using SCG.Windows;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using SCG.FileGen;
using DynamicData;
using System.Reactive.Linq;
using SCG.Controls;
using System.Windows.Controls;
using LSLib.LS.Enums;
using System.Reactive.Concurrency;
using System.Diagnostics;
using SCG.Modules.DOS2DE.LocalizationEditor.Models;
using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Modules.DOS2DE.LocalizationEditor.Views;
using SCG.Modules.DOS2DE.LocalizationEditor.Utilities;

namespace SCG.Modules.DOS2DE.LocalizationEditor.ViewModels
{
	public class LocaleCommands
	{
		private LocaleViewModel _vm { get; set; }

		public ICommand AddCustomFileCommand { get; private set; }
		public ICommand RemoveCustomFileCommand { get; private set; }

		public void AddCustomFile()
		{

		}

		public void RemoveCustomFile(LocaleCustomFileData customFileData)
		{
			_vm.CustomGroup.DataFiles.Remove(customFileData);
		}

		public LocaleCommands(LocaleViewModel vm)
		{
			_vm = vm;

			AddCustomFileCommand = ReactiveCommand.Create(AddCustomFile);
			RemoveCustomFileCommand = ReactiveCommand.Create<LocaleCustomFileData>(RemoveCustomFile);
		}
	}
}
