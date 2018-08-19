using SCG.Commands;
using SCG.Core;
using SCG.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

namespace SCG.Windows
{
	public enum BatchTextCreatorInputType
	{
		[Description("Replace keywords with text.")]
		Text,
		[Description("Incremental a keyword per text generation.")]
		Incremental,
		[Description("Decrement a keyword per text generation.")]
		Decremental
	}

	public interface IBatchTextCreatorInputData
	{
		BatchTextCreatorInputType InputType { get; }

		string Keyword { get; set; }

		//object InputValue { get; set; }

		void Reset();

		string GetOutput(string input);
	}

	public class BatchTextCreatorInputTextData : PropertyChangedBase, IBatchTextCreatorInputData
	{
		private BatchTextCreatorInputType inputType = BatchTextCreatorInputType.Text;

		public BatchTextCreatorInputType InputType
		{
			get { return inputType; }
			private set
			{
				inputType = value;
				RaisePropertyChanged("InputType");
			}
		}

		private string keyword;

		public string Keyword
		{
			get { return keyword; }
			set
			{
				keyword = value;
				RaisePropertyChanged("Keyword");
			}
		}

		public void Reset() { }

		private string inputValue;

		public string InputValue
		{
			get { return inputValue; }
			set
			{
				inputValue = value;
				RaisePropertyChanged("InputValue");
			}
		}

		public string GetOutput(string input)
		{
			return input.Replace(Keyword, InputValue);
		}
	}

	public class BatchTextCreatorInputNumberData : PropertyChangedBase, IBatchTextCreatorInputData
	{
		private BatchTextCreatorInputType inputType;

		public BatchTextCreatorInputType InputType
		{
			get { return inputType; }
			private set
			{
				inputType = value;
				RaisePropertyChanged("InputType");
			}
		}

		private string keyword;

		public string Keyword
		{
			get { return keyword; }
			set
			{
				keyword = value;
				RaisePropertyChanged("Keyword");
			}
		}

		private int inputValue = 1;

		public int InputValue
		{
			get { return inputValue; }
			set
			{
				inputValue = value;
				RaisePropertyChanged("InputValue");
			}
		}

		public int InputModifier = 0;

		public string GetOutput(string input)
		{
			int result = InputValue + InputModifier;
			return input.Replace(Keyword, result.ToString());
		}

		public void Reset()
		{
			InputModifier = 0;
		}

		public BatchTextCreatorInputNumberData() { }

		public BatchTextCreatorInputNumberData(BatchTextCreatorInputType batchTextCreatorInputType)
		{
			InputType = batchTextCreatorInputType;
		}
	}

	public class BatchTextCreatorData : PropertyChangedBase
	{
		private string inputText;

		public string InputText
		{
			get { return inputText; }
			set
			{
				inputText = value;
				RaisePropertyChanged("InputText");
			}
		}

		private string outputText;

		public string OutputText
		{
			get { return outputText; }
			set
			{
				outputText = value;
				RaisePropertyChanged("OutputText");
			}
		}

		private int generationAmount = 1;

		public int GenerationAmount
		{
			get { return generationAmount; }
			set
			{
				generationAmount = value;
				RaisePropertyChanged("GenerationAmount");
			}
		}

		private BatchTextCreatorInputType nextKeywordType = BatchTextCreatorInputType.Incremental;

		public BatchTextCreatorInputType NextKeywordType
		{
			get { return nextKeywordType; }
			set
			{
				nextKeywordType = value;
				RaisePropertyChanged("NextKeywordType");
			}
		}

		public ICommand AddCommand { get; set; }

		public ICommand RemoveCommand { get; set; }

		public ICommand GenerateCommand { get; set; }

		public ObservableCollection<IBatchTextCreatorInputData> Keywords { get; set; }
	}

	public class BatchTextCreatorInputTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var frameworkElement = container as FrameworkElement;
			if (frameworkElement != null && item != null)
			{
				if (item is IBatchTextCreatorInputData keywordData)
				{
					if(keywordData.InputType == BatchTextCreatorInputType.Text)
					{
						return frameworkElement.FindResource("InputTextBox") as DataTemplate;
					}
					else if (keywordData.InputType == BatchTextCreatorInputType.Incremental || keywordData.InputType == BatchTextCreatorInputType.Decremental)
					{
						return frameworkElement.FindResource("InputNumericUpDown") as DataTemplate;
					}
				}
			}

			return base.SelectTemplate(item, container);
		}
	}

	/// <summary>
	/// Interaction logic for BatchTextCreator.xaml
	/// </summary>
	public partial class BatchTextCreator : HideWindowBase
	{
		public BatchTextCreatorData Data { get; set; }

		public BatchTextCreator()
		{
			InitializeComponent();

			Data = new BatchTextCreatorData();
			Data.Keywords = new ObservableCollection<IBatchTextCreatorInputData>();

			DataContext = Data;

			Data.GenerateCommand = new ActionCommand(Generate);
			Data.AddCommand = new ActionCommand(AddSelectedKeywordType);
			Data.RemoveCommand = new ParameterCommand(RemoveKeyword);
		}

		public void Init(AppController controller)
		{
			var menu = controller.Data.MenuBarData.FindByID(MenuID.TextCreator);
			if (menu != null && menu.InputBinding != null)
			{
				InputBindings.Add(menu.InputBinding);
			}
		}

		public void AddSelectedKeywordType()
		{
			AddKeyword(Data.NextKeywordType);
		}

		public void AddKeyword(BatchTextCreatorInputType inputType)
		{
			if(inputType == BatchTextCreatorInputType.Text)
			{
				Data.Keywords.Add(new BatchTextCreatorInputTextData());
			}
			else if (inputType == BatchTextCreatorInputType.Incremental || inputType == BatchTextCreatorInputType.Decremental)
			{
				Data.Keywords.Add(new BatchTextCreatorInputNumberData(inputType));
			}
		}

		public void RemoveKeyword(object obj)
		{
			if(obj is IBatchTextCreatorInputData keyword)
			{
				if (Data.Keywords.Contains(keyword))
				{
					Data.Keywords.Remove(keyword);
				}
			}
		}

		public void Generate()
		{
			if(!String.IsNullOrWhiteSpace(Data.InputText) && Data.Keywords.Count > 0 && Data.GenerationAmount > 0)
			{
				Data.OutputText = "";

				for (var i = 0; i <= Data.GenerationAmount; i++)
				{
					var resultText = Data.InputText;
					foreach (var k in Data.Keywords)
					{
						if (k.InputType != BatchTextCreatorInputType.Text)
						{
							if (k is BatchTextCreatorInputNumberData keyword)
							{
								if (keyword.InputType == BatchTextCreatorInputType.Incremental)
								{
									keyword.InputModifier = i;
								}
								else if (keyword.InputType == BatchTextCreatorInputType.Decremental)
								{
									keyword.InputModifier = -i;
								}
							}
						}

						resultText = k.GetOutput(resultText);
					}

					Data.OutputText += resultText;
					if (i < Data.GenerationAmount) Data.OutputText += Environment.NewLine;
				}

				foreach (var k in Data.Keywords) k.Reset();
			}
		}
	}
}
