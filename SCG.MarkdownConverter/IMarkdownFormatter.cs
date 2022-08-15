namespace SCG.Markdown
{
	public interface IMarkdownFormatter
	{
		string Name { get; set; }
		string DefaultFileExtension { get; set; }

		string ConvertHTML(string input);
	}
}
