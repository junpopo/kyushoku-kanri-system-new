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
    public string DeliveryPlace1 { get; set; } = "";
    public string DeliveryPlace2 { get; set; } = "";
    public bool EatMonday { get; set; } = true;
    public bool EatTuesday { get; set; } = true;
    public bool EatWednesday { get; set; } = true;
    public bool EatThursday { get; set; } = true;
    public bool EatFriday { get; set; } = true;
    public bool HasMilk { get; set; } = true;
    public bool HasAllergySupport { get; set; }
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

    public bool EatsOn(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => EatMonday,
            DayOfWeek.Tuesday => EatTuesday,
            DayOfWeek.Wednesday => EatWednesday,
            DayOfWeek.Thursday => EatThursday,
            DayOfWeek.Friday => EatFriday,
            _ => false
        };
    }
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
