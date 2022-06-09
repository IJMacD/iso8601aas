public abstract class ISO8601 {
    public abstract string Type { get; }

    public abstract string Canonical { get; }

    public static ISO8601 Parse (string spec) {
        try {
            return (ISO8601DateTime)spec;
        }
        catch {
            try {
                return ISO8601Date.Parse(spec);
            }
            catch (FormatException) {
                return ISO8601Time.Parse(spec);
            }
        }
    }
}