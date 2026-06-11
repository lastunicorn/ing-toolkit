using DustInTheWind.ConsoleTools.Controls;
using DustInTheWind.ConsoleTools.Controls.Tables;

namespace DustInTheWind.Ing.Toolkit.Demo;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        const string fileName = "statement.csv";

        try
        {
            StatementDocument document = await StatementDocument.LoadFromFileAsync(fileName);

            DataGrid dataGrid = Display(document);
            dataGrid.Display();
        }
        catch (DocumentLoadException ex)
        {
            await Console.Error.WriteLineAsync($"Failed to read '{fileName}': {ex}");
            Environment.ExitCode = 1;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex}");
            Environment.ExitCode = 1;
        }
    }

    private static DataGrid Display(StatementDocument document)
    {
        DataGrid dataGrid = new()
        {
            Title = "Transactions",
            DisplayBorderBetweenRows = true,
            Footer = new[]
            {
                $"Count: {document.Count}",
                $"Initial Balance: {document.InitialBalance}",
                $"Final Balance: {document.FinalBalance}"
            }
        };

        dataGrid.Columns.Add("Date");
        dataGrid.Columns.Add("Details");
        dataGrid.Columns.Add("Debit\nAmount", HorizontalAlignment.Right);
        dataGrid.Columns.Add("Credit\nAmount", HorizontalAlignment.Right);

        foreach (BankTransaction transaction in document)
            dataGrid.Rows.Add(
                transaction.Date.ToString("yyyy-MM-dd"),
                string.Join(Environment.NewLine, transaction.Details),
                transaction.DebitAmount.ToString(),
                transaction.CreditAmount.ToString());

        return dataGrid;
    }
}
