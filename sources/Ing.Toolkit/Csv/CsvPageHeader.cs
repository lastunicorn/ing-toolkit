using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvPageHeader
{
	public string AccountOwner { get; set; }

	public string Cnp { get; set; }

	public string Address { get; set; }

	public static async Task<CsvPageHeader> CreateAsync(CsvReader csvReader)
	{
		string accountOwner = GetAccountOwner(csvReader.Parser.Record);

		if (!await csvReader.ReadAsync())
			throw new DocumentLoadException("CSV file ended before line 2 (CNP).");

		string ownerCnp = GetOwnerCnp(csvReader.Parser.Record);

		if (!await csvReader.ReadAsync())
			throw new DocumentLoadException("CSV file ended before line 3 (Address).");

		string ownerAddress = GetOwnerAddress(csvReader.Parser.Record);

		CsvPageHeader csvPageHeader = new()
		{
			AccountOwner = accountOwner,
			Cnp = ownerCnp,
			Address = ownerAddress
		};

		_ = await csvReader.ReadAsync();

		return csvPageHeader;
	}

	private static string GetAccountOwner(string[] row)
	{
		const string Key = "Titular cont: ";

		if (row.Length == 0 || !row[0].StartsWith(Key, StringComparison.OrdinalIgnoreCase))
			throw new DocumentLoadException($"CSV owner name row must respect pattern: '{Key}<name>'");

		return row[0].Substring(Key.Length);
	}

	private static string GetOwnerCnp(string[] row)
	{
		const string Key = "CNP: ";

		if (row.Length == 0 || !row[0].StartsWith(Key, StringComparison.OrdinalIgnoreCase))
			throw new DocumentLoadException($"CSV CNP row must respect pattern: '{Key}<name>'");

		return row[0].Substring(Key.Length);
	}

	private static string GetOwnerAddress(string[] row)
	{
		if (row.Length == 0)
			throw new DocumentLoadException("CSV address row must contain at least one cell.");

		return row[0]
			.Replace("\r", " ")
			.Replace("\n", " ")
			.Replace("\r\n", " ");
	}
}