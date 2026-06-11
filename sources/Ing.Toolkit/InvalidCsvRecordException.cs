namespace DustInTheWind.Ing.Toolkit;

public class InvalidCsvRecordException : DocumentLoadException
{
	public InvalidCsvRecordException(int lineNumber, Exception innerException)
		: base($"Invalid CSV record at line {lineNumber}.", innerException)
	{
	}
}