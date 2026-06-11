namespace DustInTheWind.Ing.Toolkit;

public class DataHeaderMissingException : DocumentLoadException
{
	public DataHeaderMissingException()
		: base("CSV transactions header line is missing.")
	{
	}
}