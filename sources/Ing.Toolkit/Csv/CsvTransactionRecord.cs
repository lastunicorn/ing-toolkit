using System.Globalization;
using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

/// <summary>
/// Represents a single transaction record read from the CSV file, which may consist of multiple
/// lines in the CSV file if the transaction details span multiple lines.
/// </summary>
internal class CsvTransactionRecord
{
    private static readonly CultureInfo CultureInfo = new("ro-RO");

    public DateOnly Date { get; private set; }

    public List<string> Details { get; private set; } = [];

    public decimal? DebitAmount { get; private set; }

    public decimal? CreditAmount { get; private set; }

    public static async Task<CsvTransactionRecord> CreateAsync(CsvReader csvReader, IReadOnlyList<CsvTransactionsHeaderCell> headerCells)
    {
        try
        {
            // Read the first line of the transaction record.

            CsvTransactionRecord csvTransactionRecord = new()
            {
                Date = csvReader.GetField<DateOnly>(headerCells[0].Index - 1, new DateTypeConverter(CultureInfo)),
                DebitAmount = csvReader.GetField<decimal?>(headerCells[2].Index - 1, new DecimalTypeConverter(CultureInfo)),
                CreditAmount = csvReader.GetField<decimal?>(headerCells[3].Index - 1, new DecimalTypeConverter(CultureInfo))
            };

            csvTransactionRecord.Details.Add(csvReader.GetField<string>(headerCells[1].Index - 1));

            // Read subsequent lines of the transaction record if the details span multiple lines.

            while (await csvReader.ReadAsync())
            {
                string firstCell = csvReader.Parser.Record[0];
                bool isNextTransaction = !string.IsNullOrEmpty(firstCell);

                if (isNextTransaction)
                {
                    break; // This is the start of the next transaction record, so we stop reading further lines.
                }
                else
                {
                    string secondCell = csvReader.Parser.Record[1];

                    bool isPageFooter = !string.IsNullOrEmpty(secondCell);
                    if (isPageFooter)
                        break; // This is the start of the page footer, so we stop reading further lines.

                    csvTransactionRecord.Details.Add(csvReader.GetField<string>(headerCells[1].Index - 1));
                }
            }

            return csvTransactionRecord;
        }
        catch (Exception ex)
        {
            throw new DocumentLoadException("Failed to read transaction record from CSV file.", ex);
        }
    }
}
