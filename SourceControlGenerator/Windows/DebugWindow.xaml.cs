﻿using SCG.Core;
using SCG.Data;
using SCG.Data.View;
using SCG.FileGen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ReactiveUI;
using System.Reactive.Concurrency;
using SCG.Data.App;

namespace SCG.Windows
{
	public class DebugWindowData : ReactiveObject
	{
		private string backupFolderPath;

		public string BackupFolderPath
		{
			get { return backupFolderPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref backupFolderPath, value);
				CanBackupFolder = FileCommands.IsValidDirectoryPath(backupFolderPath);
			}
		}

		private string totalBackupTimeText = "Total Backup Time: 00.00";

		public string TotalBackupTimeText
		{
			get { return totalBackupTimeText; }
			set
			{
				this.RaiseAndSetIfChanged(ref totalBackupTimeText, value);
			}
		}

		private bool canBackupFolder = false;

		public bool CanBackupFolder
		{
			get { return canBackupFolder; }
			set
			{
				this.RaiseAndSetIfChanged(ref canBackupFolder, value);
			}
		}

		private bool isBackingUp = false;

		public bool IsBackingUp
		{
			get { return isBackingUp; }
			set
			{
				this.RaiseAndSetIfChanged(ref isBackingUp, value);

				if (IsBackingUp)
				{
					CanBackupFolder = false;
				}
			}
		}

	}
	/// <summary>
	/// Interaction logic for DebugWindow.xaml
	/// </summary>
	public partial class DebugWindow : Window
	{
		private DebugWindowData debugWindowData;

		public string BackupOutputPath { get; set; }

		public Action OnClose { get; set; }

		public CancellationTokenSource Token { get; set; }

		public DebugWindow()
		{
			InitializeComponent();

			debugWindowData = new DebugWindowData();
			DataContext = debugWindowData;

			Token = new CancellationTokenSource();

			Closing += DebugWindow_Closing;
		}

		private void DebugWindow_Closing(object sender, CancelEventArgs e)
		{
			OnClose?.Invoke();
		}

		public void Init(MenuData menuData, Action onClose)
		{
			//InputBindings.AddRange(menuData.Shortcuts.Where(s => s.InputBinding != null).Select(s => s.InputBinding).ToList());
			menuData.RegisterInputBinding(this.InputBindings);
			OnClose = onClose;
		}

		private void BackupButton_Click(object sender, RoutedEventArgs e)
		{
			if (!debugWindowData.IsBackingUp)
			{
				debugWindowData.IsBackingUp = true;

				if (Token == null || (Token != null && Token.IsCancellationRequested)) Token = new CancellationTokenSource();

				RxApp.MainThreadScheduler.Schedule(() => {
					StartBackup();
				});
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			if(debugWindowData.IsBackingUp)
			{
				Token.Cancel();
			}
		}

		private async void StartBackup()
		{
			Log.Here().Important("Starting backup test...");

			debugWindowData.TotalBackupTimeText = $"Total Backup Time:";

			Stopwatch timer = new Stopwatch();
			timer.Start();

			var result = await BackupTest();

			timer.Stop();

			string elapsedTime = String.Format("{0:hh\\:mm\\:ss}", timer.Elapsed);

			debugWindowData.TotalBackupTimeText = $"Total Backup Time: {elapsedTime} | {result.ToString()}";

			debugWindowData.IsBackingUp = false;
			debugWindowData.CanBackupFolder = FileCommands.IsValidDirectoryPath(debugWindowData.BackupFolderPath);
		}

		private async Task<FileCreationTaskResult> BackupTest()
		{
			if(Directory.Exists(debugWindowData.BackupFolderPath))
			{
				string sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");
				string archiveName = "DebugBackupTest_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".zip";
				Directory.CreateDirectory("Debug");
				string outputFilePath = @"Debug\" + archiveName;
				return await BackupGenerator.CreateArchiveFromDirectory(debugWindowData.BackupFolderPath.Replace("/", "\\\\"), outputFilePath, true, Token.Token);
			}

			return FileCreationTaskResult.Error;
		}
	}
}
