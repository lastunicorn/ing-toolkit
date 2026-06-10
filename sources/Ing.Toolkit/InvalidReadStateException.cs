using DustInTheWind.Ing.Toolkit.Csv;

namespace DustInTheWind.Ing.Toolkit;

public class InvalidReadStateException : DocumentLoadException
{
    internal InvalidReadStateException(CsvDocumentReadState actual, CsvDocumentReadState expected)
        : base($"CSV document is not in the expected state: '{expected}'. Actual state: {actual}")
    {
    }
}