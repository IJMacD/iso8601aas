using System.Text.RegularExpressions;

public class ISO8601Date : ISO8601 {
    private ISO8601Date () {}

    public ISO8601Date (DateTime start, DateTime end) {
        InclusiveStart = start;
        ExclusiveEnd = end;
    }

    public static new ISO8601Date Parse (string spec) {
        if (Regex.IsMatch(spec, @"^\d{2}$")) {
            return FromCentury(int.Parse(spec));
        }

        if (Regex.IsMatch(spec, @"^\d{3}$")) {
            return FromDecade(int.Parse(spec));
        }

        if (Regex.IsMatch(spec, @"^\d{4}$")) {
            return new ISO8601Date(int.Parse(spec));
        }

        if (Regex.IsMatch(spec, @"^(\d{4})-(\d{2})$")) {
            return new ISO8601Date(int.Parse(spec.Substring(0,4)), int.Parse(spec.Substring(5,2)));
        }

        // Forbidden format
        // if (Regex.IsMatch(spec, @"^(\d{4})(\d{2})$")) {
        //     return new ISO8601Date(int.Parse(spec.Substring(0,4)), int.Parse(spec.Substring(4,2)));
        // }

        if (Regex.IsMatch(spec, @"^(\d{4})-(\d{2})-(\d{2})$")) {
            return new ISO8601Date(int.Parse(spec.Substring(0,4)), int.Parse(spec.Substring(5,2)), int.Parse(spec.Substring(8,2)));
        }

        if (Regex.IsMatch(spec, @"^(\d{4})(\d{2})(\d{2})$")) {
            return new ISO8601Date(int.Parse(spec.Substring(0,4)), int.Parse(spec.Substring(4,2)), int.Parse(spec.Substring(6,2)));
        }

        var ordinalRegex = new Regex(@"^(\d{4})-?(\d{3})$");
        if (ordinalRegex.IsMatch(spec)) {
            var groups = ordinalRegex.Match(spec).Groups;
            return new ISO8601Date(int.Parse(groups[1].Value)){ YearDay = int.Parse(groups[2].Value) };
        }

        var weekRegex = new Regex(@"^(\d{4})-?W(\d{2})$");
        if (weekRegex.IsMatch(spec)) {
            var groups = weekRegex.Match(spec).Groups;
            return new ISO8601Date(){ WeekYear = int.Parse(groups[1].Value), Week = int.Parse(groups[2].Value) };
        }

        var weekDayRegex = new Regex(@"^(\d{4})-W(\d{2})-(\d)$");
        if (weekDayRegex.IsMatch(spec)) {
            var groups = weekDayRegex.Match(spec).Groups;
            return new ISO8601Date(){ WeekYear = int.Parse(groups[1].Value), Week = int.Parse(groups[2].Value), WeekDay = int.Parse(groups[3].Value) };
        }

        var weekDayBasicRegex = new Regex(@"^(\d{4})W(\d{2})(\d)$");
        if (weekDayBasicRegex.IsMatch(spec)) {
            var groups = weekDayBasicRegex.Match(spec).Groups;
            return new ISO8601Date(){ WeekYear = int.Parse(groups[1].Value), Week = int.Parse(groups[2].Value), WeekDay = int.Parse(groups[3].Value) };
        }

        throw new FormatException($"Spec({spec.Length}) not matched '{spec}'");
    }

    public ISO8601Date (int year, int? month = null, int? day = null) {
        Year = year;
        Month = month;
        Day = day;
        InclusiveStart = new DateTime(year, month ?? 1, day ?? 1);

        if (day is not null) {
            ExclusiveEnd = InclusiveStart.AddDays(1);
        }
        else if (month is not null) {
            ExclusiveEnd = InclusiveStart.AddMonths(1);
        }
        else {
            ExclusiveEnd = InclusiveStart.AddYears(1);
        }
    }

    public override string Type => "date";

    public override string Canonical {
        get {
            if (Century.HasValue)   return Century.ToString()!;
            if (Decade.HasValue)    return Decade.ToString()!;
            if (Day.HasValue)       return InclusiveStart.ToString("yyyy-MM-dd");
            if (Month.HasValue)     return InclusiveStart.ToString("yyyy-MM");
            if (YearDay.HasValue)   return $"{Year.ToString()!.PadLeft(4, '0')}-{YearDay!.ToString()!.PadLeft(3, '0')}";
            if (WeekDay.HasValue)   return $"{WeekYear.ToString()!.PadLeft(4, '0')}-W{Week!.ToString()!.PadLeft(2, '0')}-{WeekDay}";
            if (Week.HasValue)      return $"{WeekYear.ToString()!.PadLeft(4, '0')}-W{Week!.ToString()!.PadLeft(2, '0')}";
            if (Year.HasValue)      return Year.ToString()!.PadLeft(4, '0');
            return String.Empty;
        }
    }

    public DateTime InclusiveStart { get; private set; }
    public DateTime ExclusiveEnd { get; private set; }

    public int? Century { get; private set; } = null;

    public int? Decade { get; private set; } = null;

    public int? Year { get; private set; } = null;

    public int? Month { get; private set; } = null;

    public int? Day { get; private set; } = null;

    public int? WeekYear { get; private set; } = null;

    private int? _week = null;

    public int? Week {
        get => _week;
        private set {
            if (value < 1) throw new ArgumentOutOfRangeException();
            if (value > 53) throw new ArgumentOutOfRangeException();

            if (WeekYear is not null && value is not null) {
                var dt1 = new DateTime((int)WeekYear, 1, 1);
                // 0: Sun, 6: Sat
                var wd = (int)dt1.DayOfWeek;
                var dt2 =
                    wd == 0 ? dt1.AddDays(1) :
                    (
                        (wd <= 4) ?
                            dt1.AddDays(1 - wd) :
                            dt1.AddDays(8 - wd)
                    );

                InclusiveStart = dt2.AddDays((int)(value - 1) * 7);

                if (value > 52 && InclusiveStart.Year != WeekYear) {
                    throw new ArgumentOutOfRangeException();
                }

                ExclusiveEnd = InclusiveStart.AddDays(7);
            }

            _week = value;
        }
    }

    private int? _weekDay = null;

    public int? WeekDay {
        get => _weekDay;
        private set {
            if (value < 1) throw new ArgumentOutOfRangeException();
            if (value > 7) throw new ArgumentOutOfRangeException();

            if (WeekYear is not null && Week is not null && value is not null) {
                Week = Week;

                InclusiveStart = InclusiveStart.AddDays((int)value - 1);
                ExclusiveEnd = InclusiveStart.AddDays(1);
            }

            _weekDay = value;
        }
    }

    private int? _yearDay = null;

    public int? YearDay {
        get => _yearDay;
        private set {
            if (_yearDay < 1) throw new ArgumentOutOfRangeException();

            if (Year is not null && value is not null) {
                InclusiveStart = new DateTime((int)Year, 1, 1).AddDays((int)value - 1);

                if (InclusiveStart.Year != Year) {
                    // Handle tidy up?
                    throw new ArgumentOutOfRangeException();
                }

                ExclusiveEnd = InclusiveStart.AddDays(1);
            }

            _yearDay = value;
        }
    }

    private int? _subYearGrouping = null;

    private int? SubYearGrouping {
        get => _subYearGrouping;
         set {
            if (_subYearGrouping < 20) throw new ArgumentOutOfRangeException();
            if (_subYearGrouping > 40) throw new ArgumentOutOfRangeException();

            if (Year is not null && value is not null) {
                InclusiveStart = new DateTime((int)Year, 1, 1).AddDays((int)value - 1);

                if (InclusiveStart.Year != Year) {
                    throw new ArgumentOutOfRangeException();
                }

                ExclusiveEnd = InclusiveStart.AddDays(1);
            }

            _subYearGrouping = value;
        }
    }

    public static ISO8601Date FromCentury (int century) {
        var start = new DateTime(century * 100, 1, 1);
        var end = start.AddYears(100);
        return new ISO8601Date(start, end) { Century = century };
    }

    public static ISO8601Date FromDecade (int decade) {
        var start = new DateTime(decade * 10, 1, 1);
        var end = start.AddYears(10);
        return new ISO8601Date(start, end) { Decade = decade };
    }
}
