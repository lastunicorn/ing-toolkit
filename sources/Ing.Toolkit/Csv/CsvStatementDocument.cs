using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal class CsvStatementDocument : IDisposable
{
    private readonly CsvReader csvReader;
    private IReadOnlyList<CsvTransactionsHeaderCell> headerCells;

    internal CsvDocumentReadState State { get; private set; }

    public CsvStatementDocument(TextReader textReader)
    {
        if (textReader == null) throw new ArgumentNullException(nameof(textReader));

        CsvConfiguration csvConfiguration = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            IgnoreBlankLines = true
        };

        csvReader = new(textReader, csvConfiguration);
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
            CsvTransactionsHeader csvTransactionsHeader = await CsvTransactionsHeader.Create(csvReader);
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

        await using IAsyncEnumerator<BankTransaction> enumerator = new TransactionAsyncEnumerator(csvReader, headerCells);

        while (await enumerator.MoveNextAsync())
            yield return enumerator.Current;

        if (csvReader.Parser.Record != null)
        {
            string firstCell = csvReader.Parser.Record[0];
            bool isFirstCellEmpty = string.IsNullOrEmpty(firstCell);

            if (isFirstCellEmpty)
                State = CsvDocumentReadState.PageSignatures;
            else
                State = CsvDocumentReadState.DocumentTotals;
        }
        else
        {
            State = CsvDocumentReadState.Ended;
        }
    }

    public async Task<CsvDocumentTotals> ReadDocumentTotalsAsync()
    {
        if (State != CsvDocumentReadState.DocumentTotals)
            throw new InvalidReadStateException(State, CsvDocumentReadState.DocumentTotals);

        try
        {
            CsvDocumentTotals totals = await CsvDocumentTotals.CreateAsync(csvReader);

            State = csvReader.Parser.Record == null
                ? CsvDocumentReadState.Ended
                : CsvDocumentReadState.PageSignatures;

            return totals;
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
