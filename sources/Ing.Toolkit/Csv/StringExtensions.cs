namespace DustInTheWind.Ing.Toolkit.Csv;

internal static class StringExtensions
{
	public static bool IsAllEmpty(this IEnumerable<string> collection)
	{
		if (collection == null)
			return true;

		return collection.All(x => string.IsNullOrEmpty(x));
	}
}