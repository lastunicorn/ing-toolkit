using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvPageSignatures
{
    public static async Task<CsvPageSignatures> CreateAsync(CsvReader csvReader)
    {
        while (await csvReader.ReadAsync())
        {
            string firstCell = csvReader.GetField(0);

            if (!string.IsNullOrEmpty(firstCell))
                break;
        }

        return new CsvPageSignatures();
    }
}
