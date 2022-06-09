using System.Text.RegularExpressions;

public class ISO8601Time : ISO8601 {

    public ISO8601Time (double hour, int? zoneOffset = null) {
        _ticks = (long)(hour * 3.6e10);
        _zoneOffset = zoneOffset;

        Init();

        Hour = hour;
    }

    public ISO8601Time (int hour, double minute, int? zoneOffset = null) {
        _ticks = (hour * (long)3.6e10) + (long)(minute * 6e8);
        _zoneOffset = zoneOffset;

        Init();

        Hour = hour;
        Minute = minute;
    }

    public ISO8601Time (int hour, int minute, double second, int? zoneOffset = null) {
        _ticks = (hour * (long)3.6e10) + (minute * (long)6e8) + (long)(second * 1e7);
        _zoneOffset = zoneOffset;

        Init();

        Hour = hour;
        Minute = minute;
        Second = second;
    }

    /// <summary>
    /// Adjusts _ticks
    /// </summary>
    private void Init () {
        var zoneTicks = _zoneOffset is null ? 0 : ((long)_zoneOffset * 60 * 1000 * 10000);

        _ticks -= zoneTicks;
    }

    public override string Type => "time";

    public override string ToString () => Canonical;

    public override string Canonical {
        get {
            var now = DateTime.UtcNow;
            var dtk = _zoneOffset is null ? DateTimeKind.Unspecified : DateTimeKind.Utc;
            return new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, dtk).AddTicks(_ticks).ToString("o").Substring(10);
        }
    }

    public double? Hour { get; } = null;

    public double? Minute { get; } = null;

    public double? Second { get; } = null;

    public int? ZoneHour => _zoneOffset / 60;

    public int? ZoneMinute => _zoneOffset is int zo ? Math.Abs(zo) % 60 : null;

    private long _ticks;

    private int? _zoneOffset;

    public long GetTicks () => _ticks;

    public static new ISO8601Time Parse (string spec) {
        int? zoneOffset = null;

        // ISO 8601-1:2019 § 5.4.3.2
        // Implies time and shift should match (basic vs extended)
        bool? forceExtended = null;

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

            if (match.Groups[2].Success) {
                forceExtended = match.Length == 6;
            }

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

        if (forceExtended is null || !(bool)forceExtended) {

            var hourMinuteBasicRegex = new Regex(@"^T(\d{2})(\d{2}([,.]\d+)?)$");
            if (hourMinuteBasicRegex.IsMatch(spec)) {
                var groups = hourMinuteBasicRegex.Match(spec).Groups;
                return new ISO8601Time(int.Parse(groups[1].Value), double.Parse(groups[2].Value.Replace(",",".")), zoneOffset);
            }

            var timeRegex = new Regex(@"^T(\d{2})(\d{2})(\d{2}([,.]\d+)?)$");
            if (timeRegex.IsMatch(spec)) {
                var groups = timeRegex.Match(spec).Groups;
                return new ISO8601Time(int.Parse(groups[1].Value), int.Parse(groups[2].Value), double.Parse(groups[3].Value.Replace(",",".")), zoneOffset);
            }
        }

        if (forceExtended is null || (bool)forceExtended) {
            var hourMinuteRegex = new Regex(@"^T?(\d{2}):(\d{2}([,.]\d+)?)$");
            if (hourMinuteRegex.IsMatch(spec)) {
                var groups = hourMinuteRegex.Match(spec).Groups;
                return new ISO8601Time(int.Parse(groups[1].Value), double.Parse(groups[2].Value.Replace(",",".")), zoneOffset);
            }

            var timeBasicRegex = new Regex(@"^T?(\d{2}):(\d{2}):(\d{2}([,.]\d+)?)$");
            if (timeBasicRegex.IsMatch(spec)) {
                var groups = timeBasicRegex.Match(spec).Groups;
                return new ISO8601Time(int.Parse(groups[1].Value), int.Parse(groups[2].Value), double.Parse(groups[3].Value.Replace(",",".")), zoneOffset);
            }
        }

        throw new FormatException();
    }
}
