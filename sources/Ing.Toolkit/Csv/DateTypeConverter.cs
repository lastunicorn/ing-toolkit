using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace DustInTheWind.Ing.Toolkit.Csv;

internal sealed class DateTypeConverter : DefaultTypeConverter
{
	private readonly CultureInfo cultureInfo;

	public DateTypeConverter(CultureInfo cultureInfo)
	{
		this.cultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
	}

	public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
	{
		if (string.IsNullOrWhiteSpace(text))
			throw new TypeConverterException(this, memberMapData, text, row.Context, "Date cannot be empty.");

		try
		{
			return DateOnly.ParseExact(text, "dd MMMM yyyy", cultureInfo);
		}
		catch (ArgumentException ex)
		{
			throw new TypeConverterException(this, memberMapData, text, row.Context, ex.Message, ex);
		}
	}

	public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
	{
		return value is DateOnly dateOnly
			? dateOnly.ToString("dd MMMM yyyy", cultureInfo)
			: base.ConvertToString(value, row, memberMapData);
	}
}