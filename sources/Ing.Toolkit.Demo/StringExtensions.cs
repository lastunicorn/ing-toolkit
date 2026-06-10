namespace DustInTheWind.Ing.Toolkit.Demo;

internal static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
    {
        if (maxLength <= 0)
            return string.Empty;

        if (maxLength <= 3)
            return new string('.', maxLength);

        return value.Length > maxLength
            ? string.Concat(value.AsSpan(0, maxLength - 3), "...")
            : value;
    }
}