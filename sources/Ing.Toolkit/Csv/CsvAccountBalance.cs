using System.Globalization;
using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvAccountBalance
{
    public decimal InitialBalance { get; set; }

    public decimal FinalBalance { get; set; }

    public static async Task<CsvAccountBalance> CreateAsync(CsvReader csvReader, CultureInfo cultureInfo)
    {
        if (cultureInfo == null) throw new ArgumentNullException(nameof(cultureInfo));
        
        decimal initialBalance = await ReadInitialBalance(csvReader, cultureInfo);
        decimal finalBalance = await ReadFinalBalance(csvReader, cultureInfo);
        
        return new CsvAccountBalance { InitialBalance = initialBalance, FinalBalance = finalBalance };
    }

    private static async Task<decimal> ReadInitialBalance(CsvReader csvReader, CultureInfo cultureInfo)
    {
        if (csvReader.Parser.Record == null)
            throw new DocumentLoadException("CSV file ended. Initial account balance row is missing.");

        string firstCell = csvReader.Parser.Record[0];

        const string expectedLabel = "Sold iniţial:";
        if (firstCell != expectedLabel)
            throw new DocumentLoadException($"Initial account balance row has unexpected label. Expected: '{expectedLabel}'. Actual: '{firstCell}'");

        decimal value = decimal.Parse(csvReader.Parser.Record[3], cultureInfo);
        await csvReader.ReadAsync();
        return value;
    }

    private static async Task<decimal> ReadFinalBalance(CsvReader csvReader, CultureInfo cultureInfo)
    {
        if (csvReader.Parser.Record == null)
            throw new DocumentLoadException("CSV file ended. Initial account balance row is missing.");

        string firstCell = csvReader.Parser.Record[0];

        const string expectedLabel = "Sold final:";
        if (firstCell != expectedLabel)
            throw new DocumentLoadException($"Initial account balance row has unexpected label. Expected: '{expectedLabel}'. Actual: '{firstCell}'");
        
        decimal value = decimal.Parse(csvReader.Parser.Record[3], cultureInfo);
        await csvReader.ReadAsync();
        return value;
    }
}