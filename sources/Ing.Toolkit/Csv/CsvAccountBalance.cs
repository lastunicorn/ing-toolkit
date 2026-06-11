using System.Globalization;
using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvAccountBalance
{
    public decimal InitialBalance { get; set; }

    public decimal FinalBalance { get; set; }

    public static async Task<CsvAccountBalance> CreateAsync(CsvReader csvReader, CultureInfo cultureInfo, List<string> warnings)
    {
        if (cultureInfo == null) throw new ArgumentNullException(nameof(cultureInfo));
        if (warnings == null) throw new ArgumentNullException(nameof(warnings));

        decimal initialBalance = await ReadInitialBalance(csvReader, cultureInfo, warnings);
        decimal finalBalance = await ReadFinalBalance(csvReader, cultureInfo, warnings);

        return new CsvAccountBalance { InitialBalance = initialBalance, FinalBalance = finalBalance };
    }

    private static async Task<decimal> ReadInitialBalance(CsvReader csvReader, CultureInfo cultureInfo, List<string> warnings)
    {
        if (csvReader.Parser.Record == null)
        {
            warnings.Add("CSV file ended. Initial account balance row is missing.");
            return 0m;
        }

        int recordLength = csvReader.Parser.Record.Length;
        const int expectedLength = 4;

        if (recordLength < expectedLength)
        {
            warnings.Add($"The initial account balance row is shorter than expected. Expected: {expectedLength} columns. Actual: {recordLength}.");
            return 0m;
        }

        string firstCell = csvReader.Parser.Record[0];
        const string expectedLabel = "Sold iniţial:";
        
        if (firstCell != expectedLabel)
            warnings.Add($"Initial account balance row has unexpected label. Expected: '{expectedLabel}'. Actual: '{firstCell}'");

        decimal value = decimal.Parse(csvReader.Parser.Record[3], cultureInfo);
        await csvReader.ReadAsync();
        return value;
    }

    private static async Task<decimal> ReadFinalBalance(CsvReader csvReader, CultureInfo cultureInfo, List<string> warnings)
    {
        if (csvReader.Parser.Record == null)
        {
            warnings.Add("CSV file ended. Final account balance row is missing.");
            return 0m;
        }

        int recordLength = csvReader.Parser.Record.Length;
        const int expectedLength = 4;

        if (recordLength < expectedLength)
        {
            warnings.Add($"The final account balance row is shorter than expected. Expected: {expectedLength} columns. Actual: {recordLength}.");
            return 0m;
        }

        string firstCell = csvReader.Parser.Record[0];
        const string expectedLabel = "Sold final:";

        if (firstCell != expectedLabel)
            warnings.Add($"Final account balance row has unexpected label. Expected: '{expectedLabel}'. Actual: '{firstCell}'");

        decimal value = decimal.Parse(csvReader.Parser.Record[3], cultureInfo);
        await csvReader.ReadAsync();
        return value;
    }
}