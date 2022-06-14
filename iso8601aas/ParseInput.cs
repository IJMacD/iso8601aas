class ParseInput {

    public static object TryParse (string input) {

        // List
        if (input.Contains(';')) {
            return from part in input.Trim(';').Split(';')
                select TryParse(part);
        }

        // Conjunction
        if (input.Contains('^') || input.Contains('∧')) {
            var parts = (
                from part in input.Split(new [] {'^','∧'})
                select TryParseDate(part)
            ).ToArray();

            if (parts.Length != 2) {
                return new { Error = "Cannot compute input" };
            }

            if (parts[0] is ISO8601Date d1 && parts[1] is ISO8601Date d2) {
                return new {
                    Left = d1,
                    Right = d2,
                    Contains = d1.Contains(d2),
                    IsContainedBy = d2.Contains(d1),
                    Overlaps = d1.Overlaps(d2),
                    Equals = d1.Equals(d2),
                };
            }

            if (parts[0] is ISO8601Date d3 && parts[1] is ISO8601DateTime d4) {
                return new {
                    Left = d3,
                    Right = d4,
                    Contains = d3.Contains(d4),
                };
            }

            if (parts[0] is ISO8601DateTime d5 && parts[1] is ISO8601Date d6) {
                return new {
                    Left = d6,
                    Right = d5,
                    Contains = d6.Contains(d5),
                };
            }

            if (parts[0] is ISO8601DateTime d7 && parts[1] is ISO8601DateTime d8) {
                return new {
                    Left = d7,
                    Right = d8,
                    Equals = d7 == d8,
                    IsBefore = d7 < d8,
                    IsAfter = d7 > d8,
                };
            }

            if (parts[0] is ISO8601Time d9 && parts[1] is ISO8601Time d10) {
                return new {
                    Left = d9,
                    Right = d10,
                    Equals = d9 == d10,
                    IsBefore = d9 < d10,
                    IsAfter = d9 > d10,
                };
            }

            return new { Error = "Cannot compute input" };
        }

        return TryParseDate(input);
    }

    public static object TryParseDate (string date) {
        date = date.Trim();
        try {
            return ISO8601.Parse(date);
        }
        catch (FormatException) {
            return new { Error = "Cannot parse input" };
        }
        catch (ArgumentOutOfRangeException) {
            return new { Error = "Invalid input" };
        }
        catch (NotImplementedException) {
            return new { Error = "Not implemented" };
        }
    }
}