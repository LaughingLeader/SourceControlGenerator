using SCG.Commands;
using SCG.Core;
using SCG.Data;
using SCG.Data.View;
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
using System.Windows.Threading;

namespace SCG.Windows
{
	public class TextGeneratorInputTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var frameworkElement = container as FrameworkElement;
			if (frameworkElement != null && item != null)
			{
				if (item is ITextGeneratorInputData keywordData)
				{
					if (keywordData.InputType == TextGeneratorInputType.Text)
					{
						return frameworkElement.FindResource("InputTextBox") as DataTemplate;
					}
					else if (keywordData.InputType == TextGeneratorInputType.Incremental || keywordData.InputType == TextGeneratorInputType.Decremental)
					{
						return frameworkElement.FindResource("InputNumericUpDown") as DataTemplate;
					}
				}
			}

			return base.SelectTemplate(item, container);
		}
	}

	/// <summary>
	/// Interaction logic for TextGenerator.xaml
	/// </summary>
	public partial class TextGenerator : HideWindowBase
	{
		public TextGeneratorData Data { get; set; }

		public TextGenerator()
		{
			InitializeComponent();
		}

		public void InitData()
		{
			Data = new TextGeneratorData();
			Data.Keywords = new ObservableCollection<ITextGeneratorInputData>();
		}

		public void OnDataLoaded()
		{
			DataContext = Data;

			Data.GenerateCommand = new ActionCommand(Generate);
			Data.AddCommand = new ActionCommand(AddSelectedKeywordType);
			Data.RemoveCommand = new ParameterCommand(RemoveKeyword);

			Data.Init();
			Data.OnFileLoaded = SaveDataIfPathChanged;
			Data.SaveDataEvent += OnSaveData;

			Data.InputData.TargetWindow = this;
			Data.OutputData.TargetWindow = this;
		}

		private void OnSaveData(object sender, EventArgs e)
		{
			SaveData();
		}

		public void Init(AppController controller)
		{
			var menu = controller.Data.MenuBarData.FindByID(MenuID.TextCreator);
			if (menu != null)
			{
				menu.RegisterInputBinding(this);
			}
		}

		public void AddSelectedKeywordType()
		{
			AddKeyword(Data.NextKeywordType);
		}

		public void AddKeyword(TextGeneratorInputType inputType)
		{
			var nextKeywordName = "Keyword" + (Data.Keywords.Count() + 1);

			if(inputType == TextGeneratorInputType.Text)
			{
				Data.Keywords.Add(new TextGeneratorInputTextData(nextKeywordName));
			}
			else if (inputType == TextGeneratorInputType.Incremental || inputType == TextGeneratorInputType.Decremental)
			{
				Data.Keywords.Add(new TextGeneratorInputNumberData(inputType, nextKeywordName));
			}

			//AppController.Main.SaveTextGeneratorData();
		}

		public void RemoveKeyword(object obj)
		{
			if(obj is ITextGeneratorInputData keyword)
			{
				if (Data.Keywords.Contains(keyword))
				{
					Data.Keywords.Remove(keyword);
				}
			}
		}

		public void Generate()
		{
			if (!String.IsNullOrWhiteSpace(Data.InputText) && Data.Keywords.Count > 0 && Data.GenerationAmount > 0)
			{
				Data.OutputText = "";

				for (var i = 0; i < Data.GenerationAmount; i++)
				{
					var resultText = Data.InputText;
					foreach (var k in Data.Keywords)
					{
						if (k.InputType != TextGeneratorInputType.Text)
						{
							if (k is TextGeneratorInputNumberData keyword)
							{
								if (keyword.InputType == TextGeneratorInputType.Incremental)
								{
									keyword.InputCount = i;
								}
								else if (keyword.InputType == TextGeneratorInputType.Decremental)
								{
									keyword.InputCount = -i;
								}
							}
						}

						resultText = k.GetOutput(resultText);
					}

					Data.OutputText += resultText;
					if (i < Data.GenerationAmount) Data.OutputText += Environment.NewLine;
				}

				foreach (var k in Data.Keywords) k.Reset();

				SaveData();
			}
		}

		private string LastInputPath = "";
		private string LastOutputPath = "";

		private void SaveDataIfPathChanged()
		{
			if(LastInputPath != Data.InputData.FilePath || LastOutputPath != Data.OutputData.FilePath)
			{
				LastInputPath = Data.InputData.FilePath;
				LastOutputPath = Data.OutputData.FilePath;

				SaveData();
			}
		}

		private async void SaveData()
		{
			await AppController.Main.MainWindow.Dispatcher.BeginInvoke((Action)(() => {
				AppController.Main.SaveTextGeneratorData();
			}), DispatcherPriority.Background);

			//Activate();
		}
	}
}
