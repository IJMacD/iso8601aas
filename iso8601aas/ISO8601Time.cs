using System.Text.RegularExpressions;

public class ISO8601Time : ISO8601 {
    public static new ISO8601Time Parse (string spec) {
        int? zoneOffset = null;

        // ISO 8601-1:2019 § 3.2.1
        // Can use Minus or Hyphen-Minus
        var zoneRegex = new Regex(@"[-+−](\d{2})(?::?(\d{2}))?$");

        if (spec.EndsWith("Z")) {
            zoneOffset = 0;
            spec = spec.Substring(0, spec.Length - 1);
        }
        else if (zoneRegex.IsMatch(spec)) {
            var match = zoneRegex.Match(spec);
            spec = spec.Substring(0, spec.Length - match.Length);

            var zh = int.Parse(match.Groups[1].Value);
            var zm = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;

            if (zh > 24) throw new FormatException();
            if (zm > 59) throw new FormatException();

            zoneOffset = (match.Value[0] == '+' ? 1 : -1) * (zh * 60 + zm);

            // ISO 8601-1:2019 § 4.3.13
            // Zero offset must use '+'
            if (zoneOffset == 0 && match.Value[0] != '+') {
                throw new FormatException();
            }
        }

        var hourRegex = new Regex(@"^T?(\d{2}([,.]\d+)?)$");
        if (hourRegex.IsMatch(spec)) {
            var groups = hourRegex.Match(spec).Groups;
            return new ISO8601Time(double.Parse(groups[1].Value.Replace(",",".")), zoneOffset);
        }

        var hourMinuteBasicRegex = new Regex(@"^T(\d{2})(\d{2}([,.]\d+)?)$");
        if (hourMinuteBasicRegex.IsMatch(spec)) {
            var groups = hourMinuteBasicRegex.Match(spec).Groups;
            return new ISO8601Time(int.Parse(groups[1].Value), double.Parse(groups[2].Value.Replace(",",".")), zoneOffset);
        }

        var hourMinuteRegex = new Regex(@"^T?(\d{2}):(\d{2}([,.]\d+)?)$");
        if (hourMinuteRegex.IsMatch(spec)) {
            var groups = hourMinuteRegex.Match(spec).Groups;
            return new ISO8601Time(int.Parse(groups[1].Value), double.Parse(groups[2].Value.Replace(",",".")), zoneOffset);
        }

        var timeRegex = new Regex(@"^T(\d{2})(\d{2})(\d{2}([,.]\d+)?)$");
        if (timeRegex.IsMatch(spec)) {
            var groups = timeRegex.Match(spec).Groups;
            return new ISO8601Time(int.Parse(groups[1].Value), int.Parse(groups[2].Value), double.Parse(groups[3].Value.Replace(",",".")), zoneOffset);
        }

        var timeBasicRegex = new Regex(@"^T?(\d{2}):(\d{2}):(\d{2}([,.]\d+)?)$");
        if (timeBasicRegex.IsMatch(spec)) {
            var groups = timeBasicRegex.Match(spec).Groups;
            return new ISO8601Time(int.Parse(groups[1].Value), int.Parse(groups[2].Value), double.Parse(groups[3].Value.Replace(",",".")), zoneOffset);
        }

        throw new FormatException($"Spec({spec.Length}) not matched '{spec}'");
    }

    public ISO8601Time (double hour, int? zoneOffset = null) {
        var now = DateTime.UtcNow;

        var h = (int)hour;
        var m = (int)((hour * 60) % 60);
        var s = (int)((hour * 3600) % 60);

        var ticks = (long)((hour * 36000000000) % 10000000);

        var dateTimeKind = zoneOffset is null ? DateTimeKind.Unspecified : DateTimeKind.Utc;

        var dt1 = new DateTime(now.Year, now.Month, now.Day, h, m, s, 0, dateTimeKind);

        var zoneTicks = zoneOffset is null ? 0 : ((long)zoneOffset * 60 * 1000 * 10000);

        _value = dt1.AddTicks(ticks - zoneTicks);

        var dt2 = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, dateTimeKind);
        _dayTicks = _value.Ticks - dt2.Ticks;

        Hour = hour;

        if (zoneOffset is not null) {
            ZoneHour = zoneOffset / 60;
            ZoneMinute = Math.Abs((int)zoneOffset) % 60;
        }
    }

    public ISO8601Time (int hour, double minute, int? zoneOffset = null) {
        var now = DateTime.UtcNow;

        var m = (int)minute;
        var s = (int)((minute * 60) % 60);
        var ticks = (long)((minute * 600000000) % 10000000);

        var dateTimeKind = zoneOffset is null ? DateTimeKind.Unspecified : DateTimeKind.Utc;

        var dt1 = new DateTime(now.Year, now.Month, now.Day, hour, m, s, 0, dateTimeKind);

        var zoneTicks = zoneOffset is null ? 0 : ((long)zoneOffset * 60 * 1000 * 10000);

        _value = dt1.AddTicks(ticks - zoneTicks);

        var dt2 = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, dateTimeKind);
        _dayTicks = _value.Ticks - dt2.Ticks;

        Hour = hour;
        Minute = minute;

        if (zoneOffset is not null) {
            ZoneHour = zoneOffset / 60;
            ZoneMinute = Math.Abs((int)zoneOffset) % 60;
        }
    }

    public ISO8601Time (int hour, int minute, double second, int? zoneOffset = null) {
        var now = DateTime.UtcNow;

        var s = (int)second;
        var ticks = (int)((second * 10000000) % 10000000);

        var dateTimeKind = zoneOffset is null ? DateTimeKind.Unspecified : DateTimeKind.Utc;

        var dt1 = new DateTime(now.Year, now.Month, now.Day, hour, minute, s, 0, dateTimeKind);

        var zoneTicks = zoneOffset is null ? 0 : ((long)zoneOffset * 60 * 1000 * 10000);

        _value = dt1.AddTicks(ticks - zoneTicks);

        var dt2 = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, dateTimeKind);
        _dayTicks = _value.Ticks - dt2.Ticks;

        Hour = hour;
        Minute = minute;
        Second = second;

        if (zoneOffset is not null) {
            ZoneHour = zoneOffset / 60;
            ZoneMinute = Math.Abs((int)zoneOffset) % 60;
        }
    }

    public override string Type => "time";

    private DateTime _value { get; }

    public override string ToString () => Canonical;

    public override string Canonical => _value.ToString("o").Substring(10);

    public double? Hour { get; } = null;

    public double? Minute { get; } = null;

    public double? Second { get; } = null;

    public int? ZoneHour { get; } = null;

    public int? ZoneMinute { get; } = null;

    private long _dayTicks;

    public long GetTicks () => _dayTicks;
}
