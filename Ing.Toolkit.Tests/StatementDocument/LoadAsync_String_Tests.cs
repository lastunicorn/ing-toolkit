using System.Globalization;
using FluentAssertions;

namespace DustInTheWind.Ing.Toolkit.Tests.StatementDocument;

public class LoadAsync_String_Tests
{
    // Header: ,Data,,,Detalii tranzactie,,,Debit,,Credit
    //         0  1       4                  7     9
    // Transaction row columns (0-indexed):
    //   0=Date, 3=Details, 6=Debit, 8=Credit
    //   All other columns must be empty.

    private const string Header =
        "Titular cont: John Doe,,,,,,,,,\n" +
        "CNP: 1234567890123,,,,,,,,,\n" +
        "Str. Example 1,,,,,,,,,\n" +
        ",Data,,,Detalii tranzactie,,,Debit,,Credit\n";

    private const string Balances =
        "Sold iniţial:,,,\"500,00\",,,,,,\n" +
        "Sold final:,,,\"400,00\",,,,,,\n";

    // Debit at col 6: date,,,details,,,"amount",,,
    private const string DebitRow = "01 ianuarie 2026,,,Payment,,,\"100,00\",,,\n";

    private const string MinimalValidCsv = Header + DebitRow + Balances;

    // -------------------------------------------------------------------------
    // Argument validation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingNullCsv_WhenLoading_ThenThrowsArgumentException()
    {
        Func<Task> act = () => Toolkit.StatementDocument.LoadAsync((string)null);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HavingEmptyCsv_WhenLoading_ThenThrowsArgumentException()
    {
        Func<Task> act = () => Toolkit.StatementDocument.LoadAsync(string.Empty);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HavingWhitespaceCsv_WhenLoading_ThenThrowsArgumentException()
    {
        Func<Task> act = () => Toolkit.StatementDocument.LoadAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -------------------------------------------------------------------------
    // Page header fields
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenReturnsCorrectAccountOwner()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document.TitularCont.Should().Be("John Doe");
    }

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenReturnsCorrectCnp()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document.Cnp.Should().Be("1234567890123");
    }

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenReturnsCorrectAddress()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document.Address.Should().Be("Str. Example 1");
    }

    // -------------------------------------------------------------------------
    // Account balances
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenReturnsCorrectInitialBalance()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document.InitialBalance.Should().Be(500.00m);
    }

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenReturnsCorrectFinalBalance()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document.FinalBalance.Should().Be(400.00m);
    }

    // -------------------------------------------------------------------------
    // Transaction count and basic fields
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenReturnsCorrectTransactionCount()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document.Should().HaveCount(1);
    }

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenReturnsCorrectTransactionDate()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document[0].Date.Should().Be(new DateOnly(2026, 1, 1));
    }

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenReturnsCorrectTransactionDetails()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document[0].Details.Should().ContainSingle().Which.Should().Be("Payment");
    }

    // -------------------------------------------------------------------------
    // Debit transaction
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingDebitTransaction_WhenLoading_ThenReturnsCorrectDebitAmount()
    {
        string csv =
            Header +
            "01 ianuarie 2026,,,Transfer,,,\"250,75\",,,\n" +
            "Sold iniţial:,,,\"500,00\",,,,,,\n" +
            "Sold final:,,,\"249,25\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document[0].DebitAmount.Should().Be(250.75m);
    }

    [Fact]
    public async Task HavingDebitTransaction_WhenLoading_ThenCreditAmountIsNull()
    {
        string csv =
            Header +
            "01 ianuarie 2026,,,Transfer,,,\"250,75\",,,\n" +
            "Sold iniţial:,,,\"500,00\",,,,,,\n" +
            "Sold final:,,,\"249,25\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document[0].CreditAmount.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Credit transaction
    // Credit at col 8: date,,,details,,,,,"amount",
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingCreditTransaction_WhenLoading_ThenReturnsCorrectCreditAmount()
    {
        string csv =
            Header +
            "01 ianuarie 2026,,,Incasare,,,,,\"1.200,50\",\n" +
            "Sold iniţial:,,,\"0,00\",,,,,,\n" +
            "Sold final:,,,\"1.200,50\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document[0].CreditAmount.Should().Be(1200.50m);
    }

    [Fact]
    public async Task HavingCreditTransaction_WhenLoading_ThenDebitAmountIsNull()
    {
        string csv =
            Header +
            "01 ianuarie 2026,,,Incasare,,,,,\"1.200,50\",\n" +
            "Sold iniţial:,,,\"0,00\",,,,,,\n" +
            "Sold final:,,,\"1.200,50\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document[0].DebitAmount.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Multi-line transaction details
    // Continuation rows: first cell empty, second cell empty, details at col 3
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingTransactionWithMultiLineDetails_WhenLoading_ThenAllDetailLinesAreReturned()
    {
        string csv =
            Header +
            "01 ianuarie 2026,,,Line one,,,\"50,00\",,,\n" +
            ",,,Line two,,,,,,\n" +
            ",,,Line three,,,,,,\n" +
            "Sold iniţial:,,,\"500,00\",,,,,,\n" +
            "Sold final:,,,\"450,00\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document[0].Details.Should().Equal("Line one", "Line two", "Line three");
    }

    [Fact]
    public async Task HavingTransactionWithSingleDetailLine_WhenLoading_ThenDetailsHasOneEntry()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document[0].Details.Should().ContainSingle();
    }

    // -------------------------------------------------------------------------
    // Multiple transactions
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingMultipleTransactions_WhenLoading_ThenAllTransactionsAreReturned()
    {
        string csv =
            Header +
            "01 ianuarie 2026,,,First payment,,,\"100,00\",,,\n" +
            "02 ianuarie 2026,,,Second payment,,,\"200,00\",,,\n" +
            "03 ianuarie 2026,,,Income,,,,,\"300,00\",\n" +
            "Sold iniţial:,,,\"1.000,00\",,,,,,\n" +
            "Sold final:,,,\"1.000,00\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document.Should().HaveCount(3);
    }

    [Fact]
    public async Task HavingMultipleTransactions_WhenLoading_ThenDatesAreCorrect()
    {
        string csv =
            Header +
            "01 ianuarie 2026,,,First payment,,,\"100,00\",,,\n" +
            "02 ianuarie 2026,,,Second payment,,,\"200,00\",,,\n" +
            "Sold iniţial:,,,\"1.000,00\",,,,,,\n" +
            "Sold final:,,,\"800,00\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document[0].Date.Should().Be(new DateOnly(2026, 1, 1));
        result.Document[1].Date.Should().Be(new DateOnly(2026, 1, 2));
    }

    // -------------------------------------------------------------------------
    // Multi-page document
    // Page break: after last transaction comes a signature line (first cell
    // empty, second cell non-empty), then a new page header.
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingMultiPageCsv_WhenLoading_ThenTransactionsFromAllPagesAreReturned()
    {
        string csv =
            // Page 1
            "Titular cont: John Doe,,,,,,,,,\n" +
            "CNP: 1234567890123,,,,,,,,,\n" +
            "Str. Example 1,,,,,,,,,\n" +
            ",Data,,,Detalii tranzactie,,,Debit,,Credit\n" +
            "01 ianuarie 2026,,,Page1 Tx1,,,\"100,00\",,,\n" +
            ",Semnatar1,,,,Semnatar2,,,,\n" +
            // Page 2
            "Titular cont: John Doe,,,,,,,,,\n" +
            "CNP: 1234567890123,,,,,,,,,\n" +
            "Str. Example 1,,,,,,,,,\n" +
            ",Data,,,Detalii tranzactie,,,Debit,,Credit\n" +
            "02 ianuarie 2026,,,Page2 Tx1,,,\"200,00\",,,\n" +
            "Sold iniţial:,,,\"500,00\",,,,,,\n" +
            "Sold final:,,,\"200,00\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document.Should().HaveCount(2);
    }

    [Fact]
    public async Task HavingMultiPageCsv_WhenLoading_ThenLastPageBalancesAreUsed()
    {
        string csv =
            // Page 1
            "Titular cont: John Doe,,,,,,,,,\n" +
            "CNP: 1234567890123,,,,,,,,,\n" +
            "Str. Example 1,,,,,,,,,\n" +
            ",Data,,,Detalii tranzactie,,,Debit,,Credit\n" +
            "01 ianuarie 2026,,,Page1 Tx1,,,\"100,00\",,,\n" +
            ",Semnatar1,,,,Semnatar2,,,,\n" +
            // Page 2
            "Titular cont: John Doe,,,,,,,,,\n" +
            "CNP: 1234567890123,,,,,,,,,\n" +
            "Str. Example 1,,,,,,,,,\n" +
            ",Data,,,Detalii tranzactie,,,Debit,,Credit\n" +
            "02 ianuarie 2026,,,Page2 Tx1,,,\"200,00\",,,\n" +
            "Sold iniţial:,,,\"500,00\",,,,,,\n" +
            "Sold final:,,,\"200,00\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document.InitialBalance.Should().Be(500.00m);
        result.Document.FinalBalance.Should().Be(200.00m);
    }

    // -------------------------------------------------------------------------
    // DocumentLoadResult
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenResultIsNotNull()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenDocumentIsNotNull()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Document.Should().NotBeNull();
    }

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenImplicitCastYieldsDocument()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        Toolkit.StatementDocument document = result;

        document.Should().BeSameAs(result.Document);
    }

    [Fact]
    public async Task HavingValidCsv_WhenLoading_ThenWarningsAreEmpty()
    {
        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv);

        result.Warnings.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Error cases
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingCsvWithOnlyEmptyRows_WhenLoading_ThenThrowsDocumentLoadException()
    {
        // Commas produce all-empty cells which are skipped by CsvHelper; no
        // readable record remains, so OpenAsync throws "CSV file has no data."
        Func<Task> act = () => Toolkit.StatementDocument.LoadAsync(",");

        await act.Should().ThrowAsync<DocumentLoadException>()
            .WithMessage("*no data*");
    }

    [Fact]
    public async Task HavingCsvTruncatedAfterOwnerRow_WhenLoading_ThenThrowsDocumentLoadException()
    {
        // File ends before the CNP row; ReadPageHeaderAsync throws.
        string csv = "Titular cont: John Doe,,,,,,,,,\n";

        Func<Task> act = () => Toolkit.StatementDocument.LoadAsync(csv);

        await act.Should().ThrowAsync<DocumentLoadException>()
            .WithMessage("*CNP*");
    }

    [Fact]
    public async Task HavingCsvTruncatedAfterCnpRow_WhenLoading_ThenThrowsDocumentLoadException()
    {
        // File ends before the address row; ReadPageHeaderAsync throws.
        string csv =
            "Titular cont: John Doe,,,,,,,,,\n" +
            "CNP: 1234567890123,,,,,,,,,\n";

        Func<Task> act = () => Toolkit.StatementDocument.LoadAsync(csv);

        await act.Should().ThrowAsync<DocumentLoadException>()
            .WithMessage("*Address*");
    }

    // -------------------------------------------------------------------------
    // Custom CultureInfo
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingValidCsvAndExplicitRomanianCulture_WhenLoading_ThenParsesSuccessfully()
    {
        CultureInfo roCulture = new("ro-RO");

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(MinimalValidCsv, roCulture);

        result.Document.Should().HaveCount(1);
    }

    // -------------------------------------------------------------------------
    // Romanian month names
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("01 ianuarie 2026", 2026, 1, 1)]
    [InlineData("01 februarie 2026", 2026, 2, 1)]
    [InlineData("01 martie 2026", 2026, 3, 1)]
    [InlineData("01 aprilie 2026", 2026, 4, 1)]
    [InlineData("01 mai 2026", 2026, 5, 1)]
    [InlineData("01 iunie 2026", 2026, 6, 1)]
    [InlineData("01 iulie 2026", 2026, 7, 1)]
    [InlineData("01 august 2026", 2026, 8, 1)]
    [InlineData("01 septembrie 2026", 2026, 9, 1)]
    [InlineData("01 octombrie 2026", 2026, 10, 1)]
    [InlineData("01 noiembrie 2026", 2026, 11, 1)]
    [InlineData("01 decembrie 2026", 2026, 12, 1)]
    public async Task HavingTransactionWithRomanianMonthName_WhenLoading_ThenDateIsParsedCorrectly(
        string dateString, int year, int month, int day)
    {
        string csv =
            Header +
            $"{dateString},,,Payment,,,\"10,00\",,,\n" +
            "Sold iniţial:,,,\"100,00\",,,,,,\n" +
            "Sold final:,,,\"90,00\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document[0].Date.Should().Be(new DateOnly(year, month, day));
    }

    // -------------------------------------------------------------------------
    // Decimal formatting (Romanian: period=thousands, comma=decimal)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HavingAmountsWithThousandsSeparator_WhenLoading_ThenParsesCorrectly()
    {
        string csv =
            Header +
            "01 ianuarie 2026,,,Payment,,,\"1.500,75\",,,\n" +
            "Sold iniţial:,,,\"10.000,00\",,,,,,\n" +
            "Sold final:,,,\"8.499,25\",,,,,,\n";

        DocumentLoadResult result = await Toolkit.StatementDocument.LoadAsync(csv);

        result.Document.InitialBalance.Should().Be(10000.00m);
        result.Document.FinalBalance.Should().Be(8499.25m);
        result.Document[0].DebitAmount.Should().Be(1500.75m);
    }
}
