using System.Text.RegularExpressions;

public class ISO8601DateTime : ISO8601 {

    public override string Type => "date-time";

    private DateTime _value;

    public override string ToString() => Canonical;

    public override string Canonical => _value.ToString("o");

    public int? Year { get; private set; } = null;

    public int? Month { get; private set; } = null;

    public int? Day { get; private set; } = null;

    public int? WeekYear { get; private set; } = null;

    public int? Week { get; private set; } = null;

    public int? WeekDay { get; private set; } = null;

    public int? YearDay { get; private set; } = null;

    public double? Hour { get; private set; } = null;

    public double? Minute { get; private set; } = null;

    public double? Second { get; private set; } = null;

    public int? ZoneHour { get; private set; } = null;

    public int? ZoneMinute { get; private set; } = null;

    public static new ISO8601DateTime Parse (string input) {
        if (input.IndexOf('T') <= 0) {
            throw new FormatException();
        }

        var parts = input.Split('T');

        if (parts.Length > 2) {
            throw new FormatException();
        }

        var datePart = ISO8601Date.Parse(parts[0]);
        var timePart = ISO8601Time.Parse("T" + parts[1]);

        return FromDateAndTime(datePart, timePart);
    }

    public static ISO8601DateTime FromDateAndTime (ISO8601Date date, ISO8601Time time) {
        if (date.Day is null && date.WeekDay is null && date.YearDay is null) {
            throw new FormatException();
        }

        var dateTimeKind = time.ZoneHour is null ? DateTimeKind.Unspecified : DateTimeKind.Utc;

        var value = new DateTime(date.InclusiveStart.Ticks + time.GetTicks(), dateTimeKind);

        return new ISO8601DateTime() {
            _value      = value,
            Year        = date.Year,
            Month       = date.Month,
            Day         = date.Day,
            WeekYear    = date.WeekYear,
            Week        = date.Week,
            WeekDay     = date.WeekDay,
            YearDay     = date.YearDay,
            Hour        = time.Hour,
            Minute      = time.Minute,
            Second      = time.Second,
            ZoneHour    = time.ZoneHour,
            ZoneMinute  = time.ZoneMinute,
        };
    }

    public static implicit operator ISO8601DateTime (string input) => Parse(input);

    public DateTime GetValue () => _value;

    public bool Equals (ISO8601DateTime other) => _value == other._value;

    public static bool operator < (ISO8601DateTime left, ISO8601DateTime right) => left._value < right._value;

    public static bool operator > (ISO8601DateTime left, ISO8601DateTime right) => left._value > right._value;

    public static bool operator <= (ISO8601DateTime left, ISO8601DateTime right) => left._value <= right._value;

    public static bool operator >= (ISO8601DateTime left, ISO8601DateTime right) => left._value >= right._value;
}
