namespace DustInTheWind.Ing.Toolkit;

public class DocumentLoadResult
{
	public StatementDocument Document { get; internal init; }

	public IReadOnlyList<string> Warnings { get; internal init; } = [];

	public static implicit operator StatementDocument(DocumentLoadResult documentLoadResult)
	{
		return documentLoadResult.Document;
	}
}