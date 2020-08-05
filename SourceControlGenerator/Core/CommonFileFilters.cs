using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Core
{
	public class FileBrowserFilter
	{
		public string Name { get; set; }
		public string Values { get; set; }
	}

	public static class CommonFileFilters
	{
		public static string CombineFilters(params FileBrowserFilter[] filters)
		{
			return String.Join(";", filters.Select(f => f.Values));
		}

		public static FileBrowserFilter All { get; private set; } = new FileBrowserFilter()
		{
			Name = "All types",
			Values = "*.*"
		};

		public static FileBrowserFilter NormalTextFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Normal text file",
			Values = "*.txt"
		};

		public static FileBrowserFilter Json { get; private set; } = new FileBrowserFilter()
		{
			Name = "JSON file",
			Values = "*.json"
		};

		public static List<FileBrowserFilter> JsonList { get; private set; } = new List<FileBrowserFilter>()
		{
			Json,
			All
		};

		public static FileBrowserFilter GitFiles { get; private set; } = new FileBrowserFilter()
		{
			Name = "Git text files",
			Values = "*.md;*.gitignore;*.gitattributes"
		};

		public static FileBrowserFilter SourceControlGeneratorFiles { get; private set; } = new FileBrowserFilter()
		{
			Name = "Source Control Generator files",
			Values = CombineFilters(NormalTextFile, Json, GitFiles)
		};

		public static FileBrowserFilter MarkdownFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Markdown file",
			Values = "*.md"
		};

		public static FileBrowserFilter HTMLFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "HTML file",
			Values = "*.html"
		};

		public static FileBrowserFilter MarkdownConverterFiles { get; private set; } = new FileBrowserFilter()
		{
			Name = "Markdown Converter files",
			Values = CombineFilters(NormalTextFile, MarkdownFile, HTMLFile)
		};

		public static List<FileBrowserFilter> MarkdownConverterFilesList { get; private set; } = new List<FileBrowserFilter>()
		{
			MarkdownConverterFiles,
			All
		};

		public static FileBrowserFilter TabSeparatedFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Tab-Separated file",
			Values = "*.tsv"
		};

		public static FileBrowserFilter CommaSeparatedFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Comma-Separated file",
			Values = "*.csv"
		};

		public static FileBrowserFilter DelimitedLocaleFiles { get; private set; } = new FileBrowserFilter()
		{
			Name = "Delimited Localization file",
			Values = CombineFilters(TabSeparatedFile, CommaSeparatedFile, NormalTextFile)
		};

		public static FileBrowserFilter XMLLocaleFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "XML Localization file",
			Values = "*.xml"
		};

		public static List<FileBrowserFilter> AllLocaleFilesList { get; set; } = new List<FileBrowserFilter>()
		{
			DelimitedLocaleFiles,
			TabSeparatedFile,
			CommaSeparatedFile,
			All
		};

		public static List<FileBrowserFilter> DefaultFilters { get; set; } = new List<FileBrowserFilter>()
		{
			SourceControlGeneratorFiles,
			NormalTextFile,
			Json,
			All
		};
	}
}
