using System.Globalization;
using System.Text;

namespace KyushokuKanriSystem;

public static class CabinetOfficeHolidayImporter
{
    public const string SourceName = "内閣府";
    public const string CsvUrl =
        "https://www8.cao.go.jp/chosei/shukujitsu/syukujitsu.csv";

    public static async Task<List<NoMealDate>> DownloadAsync(
        int fiscalYear,
        CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        var bytes = await client.GetByteArrayAsync(CsvUrl, cancellationToken);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var text = Encoding.GetEncoding(932).GetString(bytes);
        var fiscalStart = new DateTime(fiscalYear, 4, 1);
        var fiscalEnd = new DateTime(fiscalYear + 1, 3, 31);
        var holidays = new List<NoMealDate>();
        DateTime? latestPublishedDate = null;

        foreach (var line in text
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Skip(1))
        {
            var separatorIndex = line.IndexOf(',');
            if (separatorIndex < 1)
            {
                continue;
            }

            var dateText = Unquote(line[..separatorIndex]);
            var name = Unquote(line[(separatorIndex + 1)..]);
            if (!DateTime.TryParseExact(
                    dateText,
                    ["yyyy/M/d", "yyyy/MM/dd"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                continue;
            }

            if (latestPublishedDate is null || date.Date > latestPublishedDate.Value.Date)
            {
                latestPublishedDate = date.Date;
            }

            if (date.Date < fiscalStart || date.Date > fiscalEnd)
            {
                continue;
            }

            holidays.Add(new NoMealDate
            {
                Date = date.Date,
                Name = name,
                Source = SourceName
            });
        }

        if (latestPublishedDate is null || latestPublishedDate.Value.Date < fiscalEnd)
        {
            throw new InvalidDataException(
                $"{fiscalYear}年度末までの祝日が、内閣府からまだ公表されていません。");
        }

        return holidays
            .GroupBy(holiday => holiday.Date.Date)
            .Select(group => group.First())
            .OrderBy(holiday => holiday.Date)
            .ToList();
    }

    private static string Unquote(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length >= 2 &&
               trimmed[0] == '"' &&
               trimmed[^1] == '"'
            ? trimmed[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal)
            : trimmed;
    }
}
