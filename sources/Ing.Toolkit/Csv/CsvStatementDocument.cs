using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace DustInTheWind.Ing.Toolkit.Csv;

/// <summary>
/// Represents a CSV document containing bank transactions.
/// </summary>
/// <remarks>
/// The document is read sequentially, starting with the page header, followed by the transactions' header,
/// then the transactions, and finally the account balance and page signatures if they exist.
/// The document can be in one of several states during reading, and methods will throw exceptions if called in an invalid state.
/// </remarks>
internal class CsvStatementDocument : IDisposable
{
    private static readonly CultureInfo CultureInfo = new("ro-RO");

    private readonly CsvReader csvReader;
    private IReadOnlyList<CsvTransactionsHeaderCell> headerCells;
    private readonly List<string> warnings = [];

    public  CsvDocumentReadState State { get; private set; }

    public IReadOnlyList<string> Warnings => warnings;

    public CsvStatementDocument(TextReader textReader)
    {
        if (textReader == null) throw new ArgumentNullException(nameof(textReader));

        CsvConfiguration csvConfiguration = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            IgnoreBlankLines = true
        };

        csvReader = new CsvReader(textReader, csvConfiguration);
    }

    public async Task OpenAsync()
    {
        if (State != CsvDocumentReadState.New)
            throw new InvalidReadStateException(State, CsvDocumentReadState.New);

        if (!await csvReader.ReadAsync())
            throw new DocumentLoadException("CSV file has no data.");

        State = CsvDocumentReadState.PageHeader;
    }

    public async Task<CsvPageHeader> ReadPageHeaderAsync()
    {
        if (State != CsvDocumentReadState.PageHeader)
            throw new InvalidReadStateException(State, CsvDocumentReadState.PageHeader);

        try
        {
            CsvPageHeader header = await CsvPageHeader.CreateAsync(csvReader);
            State = CsvDocumentReadState.TransactionsHeader;
            return header;
        }
        catch (DocumentLoadException)
        {
            State = CsvDocumentReadState.Ended;
            throw;
        }
        catch (Exception ex)
        {
            State = CsvDocumentReadState.Ended;
            throw new DocumentLoadException("Failed to read transactions CSV document.", ex);
        }
    }

    public async Task<CsvTransactionsHeader> ReadTransactionsHeaderAsync()
    {
        if (State != CsvDocumentReadState.TransactionsHeader)
            throw new InvalidReadStateException(State, CsvDocumentReadState.TransactionsHeader);

        try
        {
            CsvTransactionsHeader csvTransactionsHeader = await CsvTransactionsHeader.Create(csvReader, warnings);
            headerCells = csvTransactionsHeader.Cells;

            State = CsvDocumentReadState.Transaction;
            return csvTransactionsHeader;
        }
        catch (DocumentLoadException)
        {
            State = CsvDocumentReadState.Ended;
            throw;
        }
        catch (Exception ex)
        {
            State = CsvDocumentReadState.Ended;
            throw new DocumentLoadException("Failed to read transactions CSV document.", ex);
        }
    }

    public async IAsyncEnumerable<BankTransaction> ReadTransactionsAsync()
    {
        if (State != CsvDocumentReadState.Transaction)
            throw new InvalidReadStateException(State, CsvDocumentReadState.Transaction);

        await using TransactionAsyncEnumerator enumerator = new(csvReader, headerCells, CultureInfo);

        while (await enumerator.MoveNextAsync())
            yield return enumerator.Current;

        if (csvReader.Parser.Record != null)
        {
            string firstCell = csvReader.Parser.Record[0];
            bool isFirstCellEmpty = string.IsNullOrEmpty(firstCell);

            State = isFirstCellEmpty
                ? CsvDocumentReadState.PageSignatures
                : CsvDocumentReadState.AccountBalance;
        }
        else
        {
            State = CsvDocumentReadState.Ended;
        }
    }

    public async Task<CsvAccountBalance> ReadAccountBalanceAsync()
    {
        if (State != CsvDocumentReadState.AccountBalance)
            throw new InvalidReadStateException(State, CsvDocumentReadState.AccountBalance);

        try
        {
            CsvAccountBalance accountBalance = await CsvAccountBalance.CreateAsync(csvReader, CultureInfo, warnings);

            State = csvReader.Parser.Record == null
                ? CsvDocumentReadState.Ended
                : CsvDocumentReadState.PageSignatures;

            return accountBalance;
        }
        catch (DocumentLoadException)
        {
            State = CsvDocumentReadState.Ended;
            throw;
        }
        catch (Exception ex)
        {
            State = CsvDocumentReadState.Ended;
            throw new DocumentLoadException("Failed to read transactions CSV document.", ex);
        }
    }

    public async Task<CsvPageSignatures> ReadPageSignaturesAsync()
    {
        if (State != CsvDocumentReadState.PageSignatures)
            throw new InvalidReadStateException(State, CsvDocumentReadState.PageSignatures);

        try
        {
            CsvPageSignatures footer = await CsvPageSignatures.CreateAsync(csvReader);

            State = csvReader.Parser.Record == null
                ? CsvDocumentReadState.Ended
                : CsvDocumentReadState.PageHeader;

            return footer;
        }
        catch (DocumentLoadException)
        {
            State = CsvDocumentReadState.Ended;
            throw;
        }
        catch (Exception ex)
        {
            State = CsvDocumentReadState.Ended;
            throw new DocumentLoadException("Failed to read transactions CSV document.", ex);
        }
    }

    public void Dispose()
    {
        csvReader?.Dispose();
    }
}