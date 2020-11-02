var x = 1; // top level statement

public record StringUtils // Compliant. We should not encourage people to use records as helper classes.
{
    public static string Concatenate(string s1, string s2)
    {
        return s1 + s2;
    }
    public static string Prop { get; set; }
}
