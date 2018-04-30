using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LL.SCG.Commands;
using LL.SCG.Interfaces;
using LL.SCG.Markdown;

namespace LL.SCG.Data.View
{
	public class MarkdownConverterViewData : PropertyChangedBase
	{
		private string input;

		public string Input
		{
			get { return input; }
			set
			{
				input = value;
				RaisePropertyChanged("Input");
			}
		}

		private string output;

		public string Output
		{
			get { return output; }
			set
			{
				output = value;
				RaisePropertyChanged("Output");
			}
		}

		public ObservableCollection<IMarkdownFormatter> Formatters { get; set; }

		private IMarkdownFormatter selectedFormatter;

		public IMarkdownFormatter SelectedFormatter
		{
			get { return selectedFormatter; }
			set
			{
				selectedFormatter = value;
				RaisePropertyChanged("SelectedFormatter");
			}
		}

		public void ConvertInput()
		{
			if(SelectedFormatter != null && !String.IsNullOrWhiteSpace(Input))
			{
				Output = SelectedFormatter.Convert(Input);
			}
		}

		public ICommand ConvertCommand { get; set; }

		private StringWriter writer;


		public MarkdownConverterViewData()
		{
			writer = new StringWriter();

			Formatters = new ObservableCollection<IMarkdownFormatter>(MarkdownConverter.InitFormatters());
			ConvertCommand = new ActionCommand(ConvertInput);

			SelectedFormatter = Formatters.First();
		}
	}
}
