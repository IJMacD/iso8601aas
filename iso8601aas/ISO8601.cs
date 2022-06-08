public abstract class ISO8601 {
    public abstract string Type { get; }

    public abstract string Canonical { get; }

    public static ISO8601 Parse (string spec) {
        if (spec.IndexOf('T') > 0) {
            var parts = spec.Split('T');

            if (parts.Length > 2) {
                throw new FormatException();
            }

            var datePart = ISO8601Date.Parse(parts[0]);
            var timePart = ISO8601Time.Parse("T" + parts[1]);

            return ISO8601DateTime.FromDateAndTime(datePart, timePart);
        }

        try {
            return ISO8601Date.Parse(spec);
        }
        catch (FormatException) {
            return ISO8601Time.Parse(spec);
        }
    }
}