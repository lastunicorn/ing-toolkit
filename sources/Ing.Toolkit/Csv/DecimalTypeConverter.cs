using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal sealed class DecimalTypeConverter : DefaultTypeConverter
{
	private readonly CultureInfo cultureInfo;

	public DecimalTypeConverter(CultureInfo cultureInfo)
	{
		this.cultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
	}

	public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		try
		{
			return decimal.Parse(text, NumberStyles.Any, cultureInfo);
		}
		catch (ArgumentException ex)
		{
			throw new TypeConverterException(this, memberMapData, text, row.Context, ex.Message, ex);
		}
	}
}