using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

using Xceed.Wpf.Toolkit;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Utilities;

public struct LocaleLocalizationFontTextRange
{
	public TextRange Range;
	public Color Color;
	public double Size;
}

public class LocalizationTextFormatter : ITextFormatter
{
	public string GetText(FlowDocument document)
	{
		return new TextRange(document.ContentStart, document.ContentEnd).Text;
	}

	private TextRange GetRange(TextPointer pointer, int startIndex, int length)
	{
		var start = pointer.GetPositionAtOffset(startIndex);
		var end = start.GetPositionAtOffset(length);
		return new TextRange(start, end);
	}

	private const string pattern = @"<font\s*color='#([0-9A-Fa-f]{6})'.*?>(.*?)<\/font>";

	private IEnumerable<LocaleLocalizationFontTextRange> GetAllFontRanges(FlowDocument document)
	{
		var ranges = new List<LocaleLocalizationFontTextRange>();

		var pointer = document.ContentStart;
		while (pointer != null)
		{
			if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
			{
				var textRun = pointer.GetTextInRun(LogicalDirection.Forward);
				var matches = Regex.Matches(textRun, pattern, RegexOptions.IgnoreCase);
				foreach (Match match in matches)
				{
					var hexColor = match.Groups[1];
					var content = match.Groups[2];

					//var startHex = hexColor.Index;
					//var endHex = hexColor.Length;
					var startContent = content.Index;
					var endContent = content.Length;

					var color = (Color)ColorConverter.ConvertFromString("#FF" + hexColor.Value);

					//ranges.Add(new LocaleLocalizationFontTextRange()
					//{
					//	Range = GetRange(pointer, startHex, endHex),
					//	Color = color
					//});

					ranges.Add(new LocaleLocalizationFontTextRange()
					{
						Range = GetRange(pointer, startContent, endContent),
						Color = color
					});
				}
			}

			pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
		}

		return ranges;
	}

	public void SetText(FlowDocument document, string text)
	{
		try
		{
			//if the text is null/empty clear the contents of the RTB. If you were to pass a null/empty string
			//to the TextRange.Load method an exception would occur.

			if (string.IsNullOrEmpty(text))
			{
				document.Blocks.Clear();
			}
			else
			{
				var tr = new TextRange(document.ContentStart, document.ContentEnd)
				{
					Text = text
				};

				var ranges = GetAllFontRanges(document);
				foreach (var range in ranges)
				{
					range.Range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(range.Color));
				}
			}
		}
		catch (NullReferenceException ex)
		{
			Log.Here().Error($"Error formatting text: {ex.ToString()}");
		}
	}
}
