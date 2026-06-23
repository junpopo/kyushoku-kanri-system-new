using System.Text.Json.Serialization;

namespace KyushokuKanriSystem;

public enum PersonType
{
    Student = 0,
    Staff = 1,
    Alt = 2,
    Trainee = 3,
    Tasting = 4,
    Guest = 5
}

public enum MealStatus
{
    Serve,
    Stop,
    Absent
}

public sealed class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public PersonType Type { get; set; } = PersonType.Student;
    public string Grade { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string StudentNumber { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime ActiveFrom { get; set; } = DateTime.Today;
    public DateTime? ActiveTo { get; set; }
    public string Memo { get; set; } = "";

    [JsonIgnore]
    public string TypeLabel => Type switch
    {
        PersonType.Staff => "職員",
        PersonType.Student => "生徒",
        PersonType.Alt => "ALT",
        PersonType.Trainee => "教育実習生",
        PersonType.Tasting => "試食会",
        PersonType.Guest => "ゲスト",
        _ => "生徒"
    };

    [JsonIgnore]
    public string GroupLabel => Type == PersonType.Student ? $"{Grade}年{ClassName}組".Trim() : TypeLabel;

    [JsonIgnore]
    public string FullName => string.IsNullOrWhiteSpace(LastName + FirstName)
        ? Name
        : $"{LastName} {FirstName}".Trim();
}

public sealed class MealRecord
{
    public Guid PersonId { get; set; }
    public DateTime Date { get; set; }
    public MealStatus Status { get; set; } = MealStatus.Serve;
    public string Reason { get; set; } = "";
}

public sealed class AppData
{
    public List<Person> People { get; set; } = [];
    public List<MealRecord> MealRecords { get; set; } = [];
}
