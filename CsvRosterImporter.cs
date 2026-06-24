using System.Text;

namespace KyushokuKanriSystem;

public static class CsvRosterImporter
{
    public static List<Person> Import(string path)
    {
        using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var lines = new List<string[]>();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(ParseLine(line));
            }
        }

        if (lines.Count == 0)
        {
            return [];
        }

        var header = lines[0].Select(NormalizeHeader).ToArray();
        var people = new List<Person>();

        foreach (var row in lines.Skip(1))
        {
            var fullName = Get(row, header, "氏名", "name").Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                continue;
            }

            var (lastName, firstName) = SplitName(fullName);
            var activeFrom = ParseDate(
                Get(row, header, "開始日", "在籍開始日", "activefrom").Trim(),
                DateTime.Today);
            people.Add(new Person
            {
                Type = ParsePersonType(Get(row, header, "区分", "分類", "type").Trim()),
                Grade = Get(row, header, "学年", "grade").Trim(),
                ClassName = Get(row, header, "組", "クラス", "class", "classname").Trim(),
                StudentNumber = Get(row, header, "番号", "出席番号", "number", "studentnumber").Trim(),
                LastName = lastName,
                FirstName = firstName,
                Name = fullName,
                EatMonday = true,
                EatTuesday = true,
                EatWednesday = true,
                EatThursday = true,
                EatFriday = true,
                HasMilk = true,
                HasAllergySupport = false,
                ActiveFrom = activeFrom
            });
        }

        return people;
    }

    private static DateTime ParseDate(string value, DateTime defaultValue)
    {
        return DateTime.TryParse(value, out var date) ? date.Date : defaultValue.Date;
    }

    private static PersonType ParsePersonType(string value)
    {
        return NormalizeHeader(value) switch
        {
            "職員" or "staff" => PersonType.Staff,
            "alt" => PersonType.Alt,
            "教育実習生" or "実習生" or "trainee" => PersonType.Trainee,
            "試食会" or "tasting" => PersonType.Tasting,
            "ゲスト" or "guest" => PersonType.Guest,
            _ => PersonType.Student
        };
    }

    private static (string LastName, string FirstName) SplitName(string fullName)
    {
        var parts = fullName.Split([' ', '　'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return (parts[0], string.Join("", parts.Skip(1)));
        }

        return (fullName, "");
    }

    private static string Get(string[] row, string[] header, params string[] names)
    {
        foreach (var name in names.Select(NormalizeHeader))
        {
            var index = Array.IndexOf(header, name);
            if (index >= 0 && index < row.Length)
            {
                return row[index];
            }
        }

        return "";
    }

    private static string NormalizeHeader(string value)
    {
        return value.Trim().Replace(" ", "", StringComparison.Ordinal).ToLowerInvariant();
    }

    private static string[] ParseLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        values.Add(current.ToString());
        return values.ToArray();
    }
}
