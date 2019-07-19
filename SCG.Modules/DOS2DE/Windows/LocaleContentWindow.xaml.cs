using ReactiveUI;
using SCG.Data;
using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SCG.Modules.DOS2DE.Windows
{
	public class LocaleContentWindowViewModel : HistoryBaseViewModel
	{
		private LocaleKeyEntry entry;

		public LocaleKeyEntry Entry
		{
			get => entry;
			set
			{
				this.RaiseAndSetIfChanged(ref entry, value);
			}
		}

		private bool contentSelected = false;

		public bool ContentSelected
		{
			get => contentSelected;
			set
			{
				this.RaiseAndSetIfChanged(ref contentSelected, value);
			}
		}

		private bool contentLightMode = true;

		public bool ContentLightMode
		{
			get => contentLightMode;
			set
			{
				this.RaiseAndSetIfChanged(ref contentLightMode, value);
			}
		}

		private int contentFontSize = 12;

		public int ContentFontSize
		{
			get => contentFontSize;
			set
			{
				this.RaiseAndSetIfChanged(ref contentFontSize, value);
			}
		}

		private Color? selectedColor;

		public Color? SelectedColor
		{
			get => selectedColor;
			set
			{
				this.RaiseAndSetIfChanged(ref selectedColor, value);
			}
		}

		private string selectedText = "";

		public string SelectedText
		{
			get => selectedText;
			set
			{
				this.RaiseAndSetIfChanged(ref selectedText, value);
			}
		}

		private void AddFontTag()
		{
			if (SelectedText != string.Empty)
			{
				string color = SelectedColor == null ? "#FFFFFF" : SelectedColor.Value.ToHexString();

				int start = Entry.EntryContent.IndexOf(SelectedText);

				string text = Entry.EntryContent;
				string fontStartText = $"<font color='{color}'>";
				text = text.Insert(start, fontStartText);

				int end = start + fontStartText.Length + SelectedText.Length;
				text = text.Insert(end, @"</font>");

				Entry.EntryContent = text;

				//Log.Here().Activity($"Content box text set to: {text} | Start {start} End {end}");
			}
		}

		public ICommand AddFontTagCommand { get; private set; }
		public ICommand ToggleContentLightModeCommand { get; private set; }
		public ICommand ChangeContentFontSizeCommand { get; private set; }

		public void OnActivated(CompositeDisposable disposables)
		{
			AddFontTagCommand = ReactiveCommand.Create(AddFontTag).DisposeWith(disposables);
			ToggleContentLightModeCommand = ReactiveCommand.Create(() => ContentLightMode = !ContentLightMode).DisposeWith(disposables);
			ChangeContentFontSizeCommand = ReactiveCommand.Create<string>((fontSizeStr) => {
				this.RaisePropertyChanging("ContentFontSize");
				if (int.TryParse(fontSizeStr, out contentFontSize))
				{
					this.RaisePropertyChanged("ContentFontSize");
				}
			}).DisposeWith(disposables);
		}
	}
	/// <summary>
	/// Interaction logic for LocaleContentWindow.xaml
	/// </summary>
	public partial class LocaleContentWindow : HideWindowBase, IViewFor<LocaleContentWindowViewModel>, IActivatable
    {
        public LocaleContentWindow()
        {
            InitializeComponent();

			this.WhenActivated((disposables) =>
			{
				this.WhenAnyValue(v => v.EntryContentRichTextBox.Selection.Text).ToProperty(ViewModel, "SelectedText").DisposeWith(disposables);

				ViewModel?.OnActivated(disposables);
				DataContext = ViewModel;
			});
        }

		private LocaleContentWindowViewModel vm;

		public LocaleContentWindowViewModel ViewModel
		{
			get => vm;
			set
			{
				vm = value;
			}
		}

		/// <inheritdoc/>
		object IViewFor.ViewModel
		{
			get => ViewModel;
			set => ViewModel = (LocaleContentWindowViewModel)value;
		}

		private void EntryContent_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (sender is Xceed.Wpf.Toolkit.RichTextBox richTextBox)
			{
				ViewModel.ContentSelected = richTextBox.Selection?.Text != string.Empty;
			}
		}
	}
}
