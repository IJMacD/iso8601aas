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
            var year = int.Parse(spec.Substring(0,4));
            var month = int.Parse(spec.Substring(5,2));

            if (month > 20) {
                return new ISO8601Date(year) {
                    SubYearGrouping = month
                };
            }

            return new ISO8601Date(year, month);
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
            if (Century is int c)           return c.ToString().PadLeft(2, '0');
            if (Decade is int d)            return d.ToString().PadLeft(3, '0');
            if (Day is int)                 return InclusiveStart.ToString("yyyy-MM-dd");
            if (Month is int)               return InclusiveStart.ToString("yyyy-MM");

            if (WeekYear is int wy && Week is int w) {
                var weekYearString = wy.ToString().PadLeft(4, '0');

                if (WeekDay is int wd) {
                    return $"{weekYearString}-W{w.ToString().PadLeft(2, '0')}-{wd}";
                }

                return $"{weekYearString}-W{w.ToString().PadLeft(2, '0')}";
            }

            if (Year is int y) {
                var yearString = y.ToString().PadLeft(4, '0');

                if (YearDay is int yd)
                    return $"{yearString}-{yd.ToString().PadLeft(3, '0')}";

                if (SubYearGrouping is int syg)
                    return $"{yearString}-{syg}";

                return yearString;
            }

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

    public int? SubYearGrouping {
        get => _subYearGrouping;
        private set {
            // Outside spec
            if (value < 21 || value > 41)
                throw new ArgumentOutOfRangeException();

            // Can't implement hemisphere-independent seasons
            if (value < 25) throw new NotImplementedException();

            if (Year is int y && value is int v) {

                if (v < 29) {
                    // Northern hemisphere meteorological seasons
                    // Spring starts --03-01
                    // Summer starts --06-01
                    // Autumn starts --09-01
                    // Winter starts --12-01
                    InclusiveStart = new DateTime(y, 3, 1).AddMonths((v - 25) * 3);
                    ExclusiveEnd = InclusiveStart.AddMonths(3);
                }
                else if (v < 33) {
                    // Southern hemisphere meteorological seasons
                    // Spring starts --09-01
                    // Summer starts --12-01
                    // Autumn starts --03-01
                    // Winter starts --06-01
                    InclusiveStart = new DateTime(v < 31 ? y : y - 1, 9, 1).AddMonths((v - 29) * 3);
                    ExclusiveEnd = InclusiveStart.AddMonths(3);
                }
                else {
                    int i, step;

                    switch (v) {
                        // Quarters
                        case 33:
                        case 34:
                        case 35:
                        case 36:
                            i = (v - 33);
                            step = 3;
                            break;
                        // Quadrimester
                        case 37:
                        case 38:
                        case 39:
                            i = (v - 37);
                            step = 4;
                            break;
                        // Semestral
                        case 40:
                        case 41:
                            i = (v - 40);
                            step = 6;
                            break;
                        default:
                            throw new NotImplementedException();
                    };

                    InclusiveStart = new DateTime(y, 1, 1).AddMonths(i * step);
                    ExclusiveEnd = InclusiveStart.AddMonths(step);
                }
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
