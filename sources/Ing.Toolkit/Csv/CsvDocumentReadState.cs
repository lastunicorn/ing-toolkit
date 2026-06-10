namespace DustInTheWind.Ing.Toolkit.Csv;

internal enum CsvDocumentReadState
{
    New = 0,
    PageHeader,
    TransactionsHeader,
    Transaction,
    DocumentTotals,
    PageSignatures,
    Ended
}
