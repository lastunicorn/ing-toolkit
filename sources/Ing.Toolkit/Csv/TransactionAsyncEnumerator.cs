using System.Globalization;
using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal sealed class TransactionAsyncEnumerator : IAsyncEnumerator<BankTransaction>
{
	private readonly CsvReader csvReader;
	private readonly IReadOnlyList<CsvTransactionsHeaderCell> headerCells;
	private readonly CultureInfo cultureInfo;
	private readonly List<int> optionalHeaderCellIndexes = [];

	public TransactionAsyncEnumerator(CsvReader csvReader, IReadOnlyList<CsvTransactionsHeaderCell> headerCells, CultureInfo cultureInfo)
	{
		this.csvReader = csvReader ?? throw new ArgumentNullException(nameof(csvReader));
		this.headerCells = headerCells ?? throw new ArgumentNullException(nameof(headerCells));
		this.cultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));

		if (headerCells.Count >= 1)
			optionalHeaderCellIndexes.Add(headerCells[^1].Index);

		if (headerCells.Count >= 2)
			optionalHeaderCellIndexes.Add(headerCells[^2].Index);
	}

	public BankTransaction Current { get; private set; }

	public async ValueTask<bool> MoveNextAsync()
	{
		bool isValidRecord = IsValidRecord();

		if (!isValidRecord)
			return false;

		CsvTransactionRecord csvTransactionRecord = await CsvTransactionRecord.CreateAsync(csvReader, headerCells, cultureInfo);

		Current = new BankTransaction
		{
			Date = csvTransactionRecord.Date,
			Details = csvTransactionRecord.Details,
			DebitAmount = csvTransactionRecord.DebitAmount,
			CreditAmount = csvTransactionRecord.CreditAmount
		};

		return true;
	}

	private bool IsValidRecord()
	{
		string[] cellValues = csvReader.Parser.Record;

		if (cellValues == null)
			return false;

		string firstCell = cellValues[0];

		if (firstCell.StartsWith("Sold initial") || firstCell.StartsWith("Sold iniţial"))
			return false; // This is the account balance line, not a transaction record.

		for (int cellIndex = 0; cellIndex < cellValues.Length; cellIndex++)
		{
			string cellValue = cellValues[cellIndex];
			int? headerIndex = headerCells.FirstOrDefault(x => x.Index == cellIndex + 1)?.Index;


			if (headerIndex != null)
			{
				bool isOptional = optionalHeaderCellIndexes.Contains(headerIndex.Value);

				if (!isOptional && string.IsNullOrEmpty(cellValue))
					return false;
			}
			else
			{
				if (!string.IsNullOrEmpty(cellValue))
					return false;
			}
		}

		return true;
	}

	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}
}