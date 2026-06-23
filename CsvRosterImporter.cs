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

            var typeText = Get(row, header, "type", "区分", "種別");
            var type = typeText.Contains("職", StringComparison.Ordinal) ||
                       typeText.Equals("staff", StringComparison.OrdinalIgnoreCase)
                ? PersonType.Staff
                : PersonType.Student;

            people.Add(new Person
            {
                Type = type,
                Grade = Get(row, header, "grade", "学年").Trim(),
                ClassName = Get(row, header, "class", "classname", "組", "クラス").Trim(),
                StudentNumber = Get(row, header, "number", "studentnumber", "出席番号", "番号").Trim(),
                LastName = lastName,
                FirstName = firstName,
                Name = $"{lastName} {firstName}".Trim(),
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
