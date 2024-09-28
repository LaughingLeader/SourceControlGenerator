namespace SCG.Extensions;

public static class StringExtensions
{
	public static string ReplaceLastOccurrence(this string Source, string Find, string Replace)
	{
		var place = Source.LastIndexOf(Find);

		if (place == -1)
			return Source;

		var result = Source.Remove(place, Find.Length).Insert(place, Replace);
		return result;
	}
}
