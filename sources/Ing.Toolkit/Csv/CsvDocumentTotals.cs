using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvDocumentTotals
{
    public static async Task<CsvDocumentTotals> CreateAsync(CsvReader csvReader)
    {
        while (await csvReader.ReadAsync())
        {
            string firstCell = csvReader.Parser.Record[0];
            bool isFirstCellEmpty = string.IsNullOrEmpty(firstCell);

            if (isFirstCellEmpty)
                break;
        }

        return new CsvDocumentTotals();
    }
}