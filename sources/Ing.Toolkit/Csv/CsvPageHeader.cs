using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvPageHeader
{
	public string AccountOwner { get; set; }

	public string Cnp { get; set; }

	public string Address { get; set; }

	public static async Task<CsvPageHeader> CreateAsync(CsvReader csvReader, List<string> warnings)
	{
		if (csvReader == null) throw new ArgumentNullException(nameof(csvReader));
		if (warnings == null) throw new ArgumentNullException(nameof(warnings));

		string accountOwner = GetAccountOwner(csvReader.Parser.Record, warnings);

		if (!await csvReader.ReadAsync())
			throw new DocumentLoadException("CSV file ended. CNP row not found.");

		string ownerCnp = GetOwnerCnp(csvReader.Parser.Record, warnings);

		if (!await csvReader.ReadAsync())
			throw new DocumentLoadException("CSV file ended. Address row not found.");

		string ownerAddress = GetOwnerAddress(csvReader.Parser.Record, warnings);

		CsvPageHeader csvPageHeader = new()
		{
			AccountOwner = accountOwner,
			Cnp = ownerCnp,
			Address = ownerAddress
		};

		_ = await csvReader.ReadAsync();

		return csvPageHeader;
	}

	private static string GetAccountOwner(string[] row, List<string> warnings)
	{
		const string key = "Titular cont: ";

		if (row.Length == 0)
		{
			warnings.Add("[Page Header] Owner name row missing.");
			return string.Empty;
		}
		
		string firstCell = row[0];

		if (!row[0].StartsWith(key, StringComparison.OrdinalIgnoreCase))
		{
			warnings.Add($"[Page Header] Owner name row does not respect pattern: '{key}<name>'");
			return firstCell;
		}

		return firstCell.Substring(key.Length);
	}

	private static string GetOwnerCnp(string[] row, List<string> warnings)
	{
		const string key = "CNP: ";

		if (row.Length == 0)
		{
			warnings.Add("[Page Header] CNP row missing.");
			return string.Empty;
		}
		
		string firstCell = row[0];

		if (!row[0].StartsWith(key, StringComparison.OrdinalIgnoreCase))
		{
			warnings.Add($"[Page Header] CNP row does not respect pattern: '{key}<name>'");
			return firstCell;
		}

		return firstCell.Substring(key.Length);
	}

	private static string GetOwnerAddress(string[] row, List<string> warnings)
	{
		if (row.Length == 0)
			warnings.Add("[Page Header] Address row is missing.");

		return row[0]
			.Replace("\r", " ")
			.Replace("\n", " ")
			.Replace("\r\n", " ");
	}
}