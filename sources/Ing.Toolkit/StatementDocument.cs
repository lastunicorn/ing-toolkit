using System.Collections.ObjectModel;
using System.Globalization;
using DustInTheWind.Ing.Toolkit.Csv;

namespace DustInTheWind.Ing.Toolkit;

public class StatementDocument : Collection<BankTransaction>
{
	public string TitularCont { get; set; }

	public string Cnp { get; set; }

	public string Address { get; set; }

	public decimal InitialBalance { get; set; }

	public decimal FinalBalance { get; set; }

	public static async Task<DocumentLoadResult> LoadFromFileAsync(string filePath, CultureInfo cultureInfo = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		try
		{
			using StreamReader streamReader = File.OpenText(filePath);
			return await LoadInternalAsync(streamReader, cultureInfo);
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

	public static async Task<DocumentLoadResult> LoadAsync(string csv, CultureInfo cultureInfo = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(csv);

		try
		{
			using StringReader stringReader = new(csv);
			return await LoadInternalAsync(stringReader, cultureInfo);
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

	public static async Task<DocumentLoadResult> LoadAsync(Stream stream, CultureInfo cultureInfo = null)
	{
		ArgumentNullException.ThrowIfNull(stream);

		try
		{
			using StreamReader streamReader = new(stream);
			return await LoadInternalAsync(streamReader, cultureInfo);
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

	public static async Task<DocumentLoadResult> LoadAsync(FileInfo fileInfo, CultureInfo cultureInfo = null)
	{
		ArgumentNullException.ThrowIfNull(fileInfo);

		try
		{
			using StreamReader streamReader = fileInfo.OpenText();
			return await LoadInternalAsync(streamReader, cultureInfo);
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

	public static Task<DocumentLoadResult> LoadAsync(StreamReader streamReader, CultureInfo cultureInfo = null)
	{
		ArgumentNullException.ThrowIfNull(streamReader);

		return LoadInternalAsync(streamReader, cultureInfo);
	}

	public static Task<DocumentLoadResult> LoadAsync(TextReader textReader, CultureInfo cultureInfo = null)
	{
		ArgumentNullException.ThrowIfNull(textReader);

		return LoadInternalAsync(textReader, cultureInfo);
	}

	private static async Task<DocumentLoadResult> LoadInternalAsync(TextReader textReader, CultureInfo cultureInfo = null)
	{
		try
		{
			CsvStatementDocument csvStatementDocument = new(textReader, cultureInfo);
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

					case CsvDocumentReadState.AccountBalance:
						CsvAccountBalance csvAccountBalance = await csvStatementDocument.ReadAccountBalanceAsync();
						statementDocument.InitialBalance = csvAccountBalance.InitialBalance;
						statementDocument.FinalBalance = csvAccountBalance.FinalBalance;
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
 
			return new DocumentLoadResult
			{
				Document = statementDocument,
				Warnings = csvStatementDocument.Warnings
			};
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