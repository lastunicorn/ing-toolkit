using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvTransactionsHeader
{
    private List<CsvTransactionsHeaderCell> cells = [];

    public IReadOnlyList<CsvTransactionsHeaderCell> Cells => cells;

    public static async Task<CsvTransactionsHeader> Create(CsvReader csvReader)
    {
        string[] values = csvReader.Parser.Record;

        if (values.Length != 10)
            throw new DocumentLoadException($"CSV header line has {values.Length} columns, but 10 were expected.");

        CsvTransactionsHeader csvTransactionsHeader = new();

        IEnumerable<CsvTransactionsHeaderCell> cells = values
             .Select((x, i) => new CsvTransactionsHeaderCell
             {
                 Index = i,
                 Title = x
             })
             .Where(x => !string.IsNullOrEmpty(x.Title));

        csvTransactionsHeader.cells.AddRange(cells);

        _ = await csvReader.ReadAsync();

        return csvTransactionsHeader;
    }
}
