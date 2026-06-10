using System.Collections.ObjectModel;
using DustInTheWind.Ing.Toolkit.Csv;

namespace DustInTheWind.Ing.Toolkit;

public class StatementDocument : Collection<BankTransaction>
{
    public string TitularCont { get; set; }

    public string Cnp { get; set; }

    public string Address { get; set; }

    public static async Task<StatementDocument> LoadFromFileAsync(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            using StreamReader streamReader = File.OpenText(filePath);
            return await LoadInternalAsync(streamReader);
        }
        catch (DocumentLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DocumentLoadException(ex);
        }
    }

    public static async Task<StatementDocument> LoadAsync(string csv)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(csv);

        try
        {
            using StringReader stringReader = new(csv);
            return await LoadInternalAsync(stringReader);
        }
        catch (DocumentLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DocumentLoadException(ex);
        }
    }

    public static async Task<StatementDocument> LoadAsync(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        try
        {
            using StreamReader streamReader = new(stream);
            return await LoadInternalAsync(streamReader);
        }
        catch (DocumentLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DocumentLoadException(ex);
        }
    }

    public static async Task<StatementDocument> LoadAsync(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        try
        {
            using StreamReader streamReader = fileInfo.OpenText();
            return await LoadInternalAsync(streamReader);
        }
        catch (DocumentLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DocumentLoadException(ex);
        }
    }

    public static Task<StatementDocument> LoadAsync(StreamReader streamReader)
    {
        ArgumentNullException.ThrowIfNull(streamReader);

        return LoadInternalAsync(streamReader);
    }

    public static Task<StatementDocument> LoadAsync(TextReader textReader)
    {
        ArgumentNullException.ThrowIfNull(textReader);

        return LoadInternalAsync(textReader);
    }

    private static async Task<StatementDocument> LoadInternalAsync(TextReader textReader)
    {
        try
        {
            CsvStatementDocument csvStatementDocument = new(textReader);
            StatementDocument statementDocument = [];

            while (csvStatementDocument.State != CsvDocumentReadState.Ended)
            {
                switch (csvStatementDocument.State)
                {
                    case CsvDocumentReadState.New:
                        await csvStatementDocument.OpenAsync();
                        break;

                    case CsvDocumentReadState.PageHeader:
                        CsvPageHeader csvDocumentHeader = await csvStatementDocument.ReadPageHeaderAsync();
                        statementDocument.TitularCont = csvDocumentHeader.AccountOwner;
                        statementDocument.Cnp = csvDocumentHeader.Cnp;
                        statementDocument.Address = csvDocumentHeader.Address;
                        break;

                    case CsvDocumentReadState.TransactionsHeader:
                        _ = csvStatementDocument.ReadTransactionsHeaderAsync();
                        break;

                    case CsvDocumentReadState.Transaction:
                        await foreach (BankTransaction bankTransaction in csvStatementDocument.ReadTransactionsAsync())
                            statementDocument.Add(bankTransaction);
                        break;

                    case CsvDocumentReadState.DocumentTotals:
                        _ = await csvStatementDocument.ReadDocumentTotalsAsync();
                        break;

                    case CsvDocumentReadState.PageSignatures:
                        _ = await csvStatementDocument.ReadPageSignaturesAsync();
                        break;

                    case CsvDocumentReadState.Ended:
                        break;

                    default:
                        throw new DocumentLoadException($"Invalid read state value: {csvStatementDocument.State}");
                }
            }

            return statementDocument;
        }
        catch (DocumentLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DocumentLoadException(ex);
        }
    }
}