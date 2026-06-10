using CsvHelper;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal sealed class TransactionAsyncEnumerator : IAsyncEnumerator<BankTransaction>
{
    private readonly CsvReader csvReader;
    private readonly IReadOnlyList<CsvTransactionsHeaderCell> headerCells;
    private readonly List<int> optionalHeaderCellIndexes = [];

    public TransactionAsyncEnumerator(CsvReader csvReader, IReadOnlyList<CsvTransactionsHeaderCell> headerCells)
    {
        if (csvReader == null) throw new ArgumentNullException(nameof(csvReader));

        this.csvReader = csvReader;
        this.headerCells = headerCells;

        if (headerCells.Count >= 1)
            optionalHeaderCellIndexes.Add(headerCells[headerCells.Count - 1].Index);

        if (headerCells.Count >= 2)
            optionalHeaderCellIndexes.Add(headerCells[headerCells.Count - 2].Index);
    }

    public BankTransaction Current { get; private set; }

    public async ValueTask<bool> MoveNextAsync()
    {
        bool isValidRecord = IsValidRecord();

        if (!isValidRecord)
            return false;

        CsvTransactionRecord csvTransactionRecord = await CsvTransactionRecord.CreateAsync(csvReader, headerCells);

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
