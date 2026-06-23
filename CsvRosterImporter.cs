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
            var lastName = Get(row, header, "lastname", "last_name", "姓", "名字", "苗字").Trim();
            var firstName = Get(row, header, "firstname", "first_name", "名", "名前").Trim();
            var fullName = Get(row, header, "name", "氏名").Trim();

            if (string.IsNullOrWhiteSpace(lastName + firstName) && !string.IsNullOrWhiteSpace(fullName))
            {
                var parts = fullName.Split([' ', '　'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    lastName = parts[0];
                    firstName = string.Join("", parts.Skip(1));
                }
                else
                {
                    lastName = fullName;
                }
            }

            if (string.IsNullOrWhiteSpace(lastName + firstName))
            {
                continue;
            }

            var type = ParsePersonType(Get(row, header, "type", "区分", "種別"));

            people.Add(new Person
            {
                Type = type,
                Grade = Get(row, header, "grade", "学年").Trim(),
                ClassName = Get(row, header, "class", "classname", "組", "クラス").Trim(),
                StudentNumber = Get(row, header, "number", "studentnumber", "出席番号", "番号").Trim(),
                LastName = lastName,
                FirstName = firstName,
                Name = $"{lastName} {firstName}".Trim(),
                DeliveryPlace1 = Get(row, header, "deliveryplace1", "配膳場所1", "配膳場所１").Trim(),
                DeliveryPlace2 = Get(row, header, "deliveryplace2", "配膳場所2", "配膳場所２").Trim(),
                EatMonday = GetBool(row, header, true, "eatmonday", "月", "月曜", "月曜日"),
                EatTuesday = GetBool(row, header, true, "eattuesday", "火", "火曜", "火曜日"),
                EatWednesday = GetBool(row, header, true, "eatwednesday", "水", "水曜", "水曜日"),
                EatThursday = GetBool(row, header, true, "eatthursday", "木", "木曜", "木曜日"),
                EatFriday = GetBool(row, header, true, "eatfriday", "金", "金曜", "金曜日"),
                HasMilk = GetBool(row, header, true, "milk", "hasmilk", "牛乳", "牛乳有無"),
                HasAllergySupport = GetBool(row, header, false, "allergy", "hasallergysupport", "アレルギー", "アレルギー対応", "アレルギー対応有無"),
                ActiveFrom = DateTime.Today,
                Memo = Get(row, header, "memo", "備考").Trim()
            });
        }

        return people;
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

    private static bool GetBool(string[] row, string[] header, bool defaultValue, params string[] names)
    {
        var value = Get(row, header, names).Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("y", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("あり", StringComparison.Ordinal) ||
               value.Equals("有", StringComparison.Ordinal) ||
               value.Equals("○", StringComparison.Ordinal) ||
               value.Equals("〇", StringComparison.Ordinal) ||
               value.Equals("済", StringComparison.Ordinal);
    }

    private static PersonType ParsePersonType(string value)
    {
        var text = value.Trim();
        if (text.Equals("ALT", StringComparison.OrdinalIgnoreCase))
        {
            return PersonType.Alt;
        }

        if (text.Contains("教育", StringComparison.Ordinal) || text.Contains("実習", StringComparison.Ordinal))
        {
            return PersonType.Trainee;
        }

        if (text.Contains("試食", StringComparison.Ordinal))
        {
            return PersonType.Tasting;
        }

        if (text.Contains("ゲスト", StringComparison.Ordinal) || text.Equals("guest", StringComparison.OrdinalIgnoreCase))
        {
            return PersonType.Guest;
        }

        if (text.Contains("職", StringComparison.Ordinal) || text.Equals("staff", StringComparison.OrdinalIgnoreCase))
        {
            return PersonType.Staff;
        }

        return PersonType.Student;
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
