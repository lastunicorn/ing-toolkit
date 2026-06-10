namespace DustInTheWind.Ing.Toolkit;

public class DocumentLoadException : Exception
{
    public DocumentLoadException()
        : base("Failed to load transactions CSV file.")
    {
    }

    public DocumentLoadException(Exception innerException)
        : base("Failed to load transactions CSV file.", innerException)
    {
    }


    public DocumentLoadException(string message)
        : base(message)
    {
    }

    public DocumentLoadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
