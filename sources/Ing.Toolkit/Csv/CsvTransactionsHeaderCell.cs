namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvTransactionsHeaderCell
{
	public int Index { get; set; }

	public string Title { get; set; }

	public override string ToString()
	{
		return $"{Index}: {Title}";
	}
}