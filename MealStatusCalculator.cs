namespace KyushokuKanriSystem;

public static class MealStatusCalculator
{
    public static MealStatus GetStatus(
        Person person,
        DateTime date,
        IReadOnlyCollection<MealRecord> records,
        IReadOnlyCollection<MealScheduleChange> changes,
        IReadOnlyCollection<NoMealDate> noMealDates)
    {
        if (FindNoMealDate(date, noMealDates) is not null)
        {
            return MealStatus.Stop;
        }

        var record = records.FirstOrDefault(item =>
            item.PersonId == person.Id && item.Date.Date == date.Date);
        if (record is not null)
        {
            return record.Status;
        }

        var change = ApplicableChange(person, date, changes);
        if (HasPendingHigherPriorityStart(person, date, change, changes))
        {
            return MealStatus.Stop;
        }

        if (change?.Action == MealScheduleAction.Stop)
        {
            return MealStatus.Stop;
        }

        return person.EatsOn(date.DayOfWeek) ? MealStatus.Serve : MealStatus.Stop;
    }

    public static string GetReason(
        Person person,
        DateTime date,
        IReadOnlyCollection<MealRecord> records,
        IReadOnlyCollection<MealScheduleChange> changes,
        IReadOnlyCollection<NoMealDate> noMealDates)
    {
        var noMealDate = FindNoMealDate(date, noMealDates);
        if (noMealDate is not null)
        {
            return noMealDate.Name;
        }

        var record = records.FirstOrDefault(item =>
            item.PersonId == person.Id && item.Date.Date == date.Date);
        if (record is not null && !string.IsNullOrWhiteSpace(record.Reason))
        {
            return record.Reason;
        }

        if (record?.Status == MealStatus.Serve)
        {
            return "";
        }

        if (record?.Status == MealStatus.Absent)
        {
            return "欠席";
        }

        var change = ApplicableChange(person, date, changes);
        var pendingStart = PendingHigherPriorityStart(person, date, change, changes);
        if (pendingStart is not null)
        {
            return string.IsNullOrWhiteSpace(pendingStart.Reason)
                ? "給食開始日前"
                : $"給食開始日前（{pendingStart.Reason}）";
        }

        if (change?.Action == MealScheduleAction.Stop)
        {
            return string.IsNullOrWhiteSpace(change.Reason)
                ? $"{ScopeLabel(change.Scope)}で給食停止"
                : change.Reason;
        }

        if (record is not null)
        {
            return "日別設定で給食停止";
        }

        return person.EatsOn(date.DayOfWeek) ? "" : "喫食日ではありません";
    }

    private static MealScheduleChange? ApplicableChange(
        Person person,
        DateTime date,
        IReadOnlyCollection<MealScheduleChange> changes)
    {
        return changes
            .Where(change =>
                change.EffectiveDate.Date <= date.Date &&
                (change.EndDate is null || change.EndDate.Value.Date >= date.Date) &&
                Applies(change, person))
            .OrderByDescending(change => change.EffectiveDate)
            .ThenByDescending(change => ScopePriority(change.Scope))
            .FirstOrDefault();
    }

    private static bool HasPendingHigherPriorityStart(
        Person person,
        DateTime date,
        MealScheduleChange? currentChange,
        IReadOnlyCollection<MealScheduleChange> changes)
    {
        return PendingHigherPriorityStart(person, date, currentChange, changes) is not null;
    }

    private static MealScheduleChange? PendingHigherPriorityStart(
        Person person,
        DateTime date,
        MealScheduleChange? currentChange,
        IReadOnlyCollection<MealScheduleChange> changes)
    {
        var currentPriority = currentChange is null
            ? -1
            : ScopePriority(currentChange.Scope);
        return changes
            .Where(change =>
                change.Action == MealScheduleAction.Start &&
                change.EndDate is null &&
                change.EffectiveDate.Date > date.Date &&
                Applies(change, person) &&
                ScopePriority(change.Scope) > currentPriority)
            .OrderBy(change => change.EffectiveDate)
            .ThenByDescending(change => ScopePriority(change.Scope))
            .FirstOrDefault();
    }

    private static bool Applies(MealScheduleChange change, Person person)
    {
        return change.Scope switch
        {
            MealScheduleScope.All => true,
            MealScheduleScope.Grade =>
                person.Type == PersonType.Student &&
                person.Grade.Equals(change.Grade, StringComparison.CurrentCultureIgnoreCase),
            MealScheduleScope.Person => change.PersonId == person.Id,
            _ => false
        };
    }

    private static int ScopePriority(MealScheduleScope scope)
    {
        return scope switch
        {
            MealScheduleScope.All => 0,
            MealScheduleScope.Grade => 1,
            _ => 2
        };
    }

    private static string ScopeLabel(MealScheduleScope scope)
    {
        return scope switch
        {
            MealScheduleScope.All => "全体",
            MealScheduleScope.Grade => "学年",
            _ => "個人"
        };
    }

    private static NoMealDate? FindNoMealDate(
        DateTime date,
        IReadOnlyCollection<NoMealDate> noMealDates)
    {
        return noMealDates.FirstOrDefault(item => item.Date.Date == date.Date);
    }
}
