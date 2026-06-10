namespace DustInTheWind.Ing.Toolkit;

public class BankTransaction
{
    public DateOnly Date { get; init; }

    public List<string> Details { get; init; }

    public decimal? DebitAmount { get; init; }

    public decimal? CreditAmount { get; init; }
}
