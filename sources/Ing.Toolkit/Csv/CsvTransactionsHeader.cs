using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvTransactionsHeader
{
	private readonly List<CsvTransactionsHeaderCell> cells = [];

	public IReadOnlyList<CsvTransactionsHeaderCell> Cells => cells;

	public static async Task<CsvTransactionsHeader> Create(CsvReader csvReader, List<string> warnings)
	{
		if (csvReader == null) throw new ArgumentNullException(nameof(csvReader));
		if (warnings == null) throw new ArgumentNullException(nameof(warnings));

		string[] values = csvReader.Parser.Record;

		if (values == null)
			throw new DocumentLoadException("CSV header line is missing.");

		CsvTransactionsHeader csvTransactionsHeader = new();

		IEnumerable<CsvTransactionsHeaderCell> cells = values
			.Select((x, i) => new CsvTransactionsHeaderCell
			{
				Index = i,
				Title = x
			})
			.Where(x => !string.IsNullOrEmpty(x.Title));

		csvTransactionsHeader.cells.AddRange(cells);

		if (csvTransactionsHeader.Cells.Count == 0)
			warnings.Add("[Data Header] CSV header line does not contain any valid cell.");

		_ = await csvReader.ReadAsync();

		return csvTransactionsHeader;
	}
}