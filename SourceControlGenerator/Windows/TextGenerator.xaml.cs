using ReactiveUI;
using SCG.Commands;
using SCG.Core;
using SCG.Data;
using SCG.Data.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
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
					if (keywordData.InputType == TextGeneratorInputType.Incremental || keywordData.InputType == TextGeneratorInputType.Decremental)
					{
						return frameworkElement.FindResource("InputNumericUpDown") as DataTemplate;
					}
					else if (keywordData.InputType == TextGeneratorInputType.Text)
					{
						return frameworkElement.FindResource("InputTextBox") as DataTemplate;
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
		public TextGeneratorViewModel Data { get; set; }

		public TextGenerator()
		{
			InitializeComponent();
		}

		public void InitData()
		{
			Data = new TextGeneratorViewModel();
		}

		public void OnDataLoaded()
		{
			DataContext = Data;

			Data.Init(this);

			Data.GenerateCommand = new ActionCommand(Generate);
			Data.AddCommand = new ActionCommand(AddSelectedKeywordType);
			Data.RemoveCommand = new ParameterCommand(RemoveKeyword);

			Data.OnFileLoaded = SaveDataIfPathChanged;
			Data.SaveDataEvent += OnSaveData;

			if(Data.GeneratorPresets.Count == 0)
			{
				Data.AddPreset();
			}
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
				menu.RegisterInputBinding(this.InputBindings);
			}
		}

		public void AddSelectedKeywordType()
		{
			if(Data.ActiveData != null)
			{
				AddKeyword(Data.ActiveData.NextKeywordType);
			}
		}

		public void AddKeyword(TextGeneratorInputType inputType)
		{
			if (Data.ActiveData != null)
			{
				var nextKeywordName = "Keyword" + (Data.ActiveData.Keywords.Count() + 1);

				if (inputType == TextGeneratorInputType.Text)
				{
					Data.ActiveData.Keywords.Add(new TextGeneratorInputTextData(nextKeywordName));
				}
				else if (inputType == TextGeneratorInputType.Incremental || inputType == TextGeneratorInputType.Decremental)
				{
					Data.ActiveData.Keywords.Add(new TextGeneratorInputNumberData(inputType, nextKeywordName));
				}
			}
			//AppController.Main.SaveTextGeneratorData();
		}

		public void RemoveKeyword(object obj)
		{
			if(obj is ITextGeneratorInputData keyword && Data.ActiveData != null)
			{
				if (Data.ActiveData.Keywords.Contains(keyword))
				{
					Data.ActiveData.Keywords.Remove(keyword);
				}
			}
		}

		public void Generate()
		{
			if (Data.ActiveData != null && !String.IsNullOrWhiteSpace(Data.ActiveData.InputText) && Data.ActiveData.Keywords.Count > 0 && Data.ActiveData.GenerationAmount > 0)
			{
				Data.ActiveData.OutputText = "";

				for (var i = 0; i < Data.ActiveData.GenerationAmount; i++)
				{
					var resultText = Data.ActiveData.InputText;
					foreach (var k in Data.ActiveData.Keywords)
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

					Data.ActiveData.OutputText += resultText;
					if (i < Data.ActiveData.GenerationAmount) Data.ActiveData.OutputText += Environment.NewLine;
				}

				foreach (var k in Data.ActiveData.Keywords) k.Reset();

				SaveData();
			}
		}

		private string LastInputPath = "";
		private string LastOutputPath = "";

		private void SaveDataIfPathChanged()
		{
			if(Data.ActiveData != null && (LastInputPath != Data.ActiveData.InputData.FilePath || LastOutputPath != Data.ActiveData.OutputData.FilePath))
			{
				LastInputPath = Data.ActiveData.InputData.FilePath;
				LastOutputPath = Data.ActiveData.OutputData.FilePath;

				SaveData();
			}
		}

		private void SaveData()
		{
			RxApp.MainThreadScheduler.Schedule(() => {
				Data.FooterText = AppController.Main.SaveTextGeneratorData();
			});

			//Activate();
		}

		private void PresetComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(sender is ComboBox cb)
			{
				if(e.Key == Key.Enter)
				{
					this.Data.ActiveData.Name = cb.Text;
					Keyboard.ClearFocus();
				}
				else if (e.Key == Key.Escape)
				{
					cb.Text = this.Data.ActiveData.Name;
					Keyboard.ClearFocus();
				}
			}
		}
	}
}
