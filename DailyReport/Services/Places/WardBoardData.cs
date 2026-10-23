using System.Globalization;
using System.Text.RegularExpressions;
using DailyReport.Models.WardsModels;

namespace DailyReport.Services.Places;

public enum WardBoardStatus
{
    Free,
    Partial,
    Full,
    Overflow,
    Closed
}

public static class WardBoardStatusRules
{
    public static WardBoardStatus Get(int capacity, int patients, bool canPut) =>
        !canPut ? WardBoardStatus.Closed
        : patients > capacity ? WardBoardStatus.Overflow
        : patients == 0 ? WardBoardStatus.Free
        : patients >= capacity ? WardBoardStatus.Full
        : WardBoardStatus.Partial;

    public static string Text(WardBoardStatus status) => status switch
    {
        WardBoardStatus.Closed => "закрыта",
        WardBoardStatus.Overflow => "переполнена",
        WardBoardStatus.Full => "заполнена",
        WardBoardStatus.Partial => "частично занята",
        _ => "свободна"
    };

    public static string CssClass(WardBoardStatus status) => status switch
    {
        WardBoardStatus.Closed => "samson-dash-ward--closed",
        WardBoardStatus.Overflow => "samson-dash-ward--overflow",
        WardBoardStatus.Full => "samson-dash-ward--full",
        WardBoardStatus.Partial => "samson-dash-ward--partial",
        _ => "samson-dash-ward--free"
    };
}

public sealed class WardBoardData
{
    public List<WardBoardDepartment> Departments { get; init; } = new();

    public int BedsTotal => Departments.Sum(d => d.Beds);

    public int OccupiedTotal => Departments.Sum(d => d.Occupied);

    public int FreeTotal => Departments.Sum(d => d.Free);

    public int ClosedTotal => Departments.Sum(d => d.Closed);

    public int OverflowTotal => Departments.Sum(d => d.Overflow);
}

public sealed class WardBoardDepartment
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public List<WardBoardWard> Wards { get; init; } = new();

    public int Beds => Wards.Sum(w => w.Capacity);

    public int Occupied => Wards.Sum(w => w.Occupied);

    public int Free => Math.Max(0, Beds - Occupied);

    public int Closed => Wards.Count(w => !w.CanPut);

    public int Overflow => Wards.Count(w => w.OverflowCount > 0);
}

public sealed class WardBoardWard
{
    public string Number { get; init; } = string.Empty;

    public int Capacity { get; init; }

    public bool CanPut { get; init; }

    public List<WardBoardPatient> Patients { get; init; } = new();

    public int Occupied => Patients.Count;

    public int FreeSlots => Math.Max(0, Capacity - Occupied);

    public int OverflowCount => Math.Max(0, Occupied - Capacity);

    public WardBoardStatus Status => WardBoardStatusRules.Get(Capacity, Occupied, CanPut);

    public string StatusText => WardBoardStatusRules.Text(Status);

    public string StatusClass => WardBoardStatusRules.CssClass(Status);
}

public sealed class WardBoardPatient
{
    public string Male { get; init; } = string.Empty;

    public string AgeText { get; init; } = string.Empty;

    public string MkbCode { get; init; } = string.Empty;

    public DateTime HospitalisationDate { get; init; }
}

public static class PatientAge
{
    private static readonly Regex AgeRegex = new(
        @"(\d+)\s*(г|год|л|лет|мес|дн|день|дней)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string Format(Patient patient) =>
        Format(patient.sAge, patient.Age, patient.AgeMonts);

    public static string Format(string? sAge, int age, int? ageMonths)
    {
        var text = (sAge ?? string.Empty).Trim();

        if (text.Length > 0)
        {
            var match = AgeRegex.Match(text);
            if (match.Success)
            {
                return FormatNumber(
                    int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                    match.Groups[2].Value,
                    age,
                    ageMonths);
            }

            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric))
            {
                return numeric > 0 ? $"{numeric} г." : FromFields(age, ageMonths);
            }

            return text;
        }

        return FromFields(age, ageMonths);
    }

    private static string FormatNumber(int value, string unit, int age, int? ageMonths)
    {
        if (unit.StartsWith("мес", StringComparison.OrdinalIgnoreCase))
        {
            return $"{value} мес.";
        }

        if (unit.StartsWith("д", StringComparison.OrdinalIgnoreCase))
        {
            return $"{value} дн.";
        }

        return value > 0 ? $"{value} г." : FromFields(age, ageMonths);
    }

    private static string FromFields(int age, int? ageMonths)
    {
        if (ageMonths is > 0)
        {
            return $"{ageMonths} мес.";
        }

        return age > 0 ? $"{age} г." : "0 дн.";
    }
}
