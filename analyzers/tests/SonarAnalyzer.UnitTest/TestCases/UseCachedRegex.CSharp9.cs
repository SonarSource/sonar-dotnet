using System.Text.RegularExpressions;

public class UseCachedRegex
{
    const string IMMUTABLE_REGEX_PATTERN = "^[a-zA-Z]$";
    readonly string READONLY_REGEX_PATTERN = "^[a-zA-Z]$";

    Regex MutableCachedRegex = new ("^[a-zA-Z]$"); // ???
    static Regex StaticMutableCachedRegex = new ("^[a-zA-Z]$"); // ???
    string mutableRegexPattern = "^[a-zA-Z]$";

    public Regex PropertyCachedRegex { get; set; } = new (IMMUTABLE_REGEX_PATTERN); // Compliant

    void Compliant(string input)
    {
        Regex myRegex = new ($"^.+{input}.+$"); // Compliant
        myRegex = new ($"^.+" + input + ".+$"); // Compliant
        myRegex = new(mutableRegexPattern); // Compliant
        PropertyCachedRegex ??= new Regex("^[a-zA-Z]$"); // Compliant
        PropertyCachedRegex ??= PropertyCachedRegex ?? new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex ??= new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex ??= MutableCachedRegex ?? new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex ??= new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex ??= StaticMutableCachedRegex ?? new Regex("^[a-zA-Z]$"); // Compliant
    }

    void Noncompliant()
    {
        Regex myRegex = new ("^[a-zA-Z]$"); // Noncompliant
        //              ^^^^^^^^^^^^^^^^^^

        myRegex = new ("^[a-zA-Z]$"); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^

        myRegex = new (IMMUTABLE_REGEX_PATTERN); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        myRegex = new (READONLY_REGEX_PATTERN); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        myRegex ??= new Regex("^[a-zA-Z]$"); // Noncompliant
        //          ^^^^^^^^^^^^^^^^^^^^^^^
        myRegex ??= myRegex ?? new Regex("^[a-zA-Z]$"); // Noncompliant
        //                     ^^^^^^^^^^^^^^^^^^^^^^^

        MutableCachedRegex = new ("^[a-zA-Z]$"); // Noncompliant
        StaticMutableCachedRegex = new ("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex = new ("^[a-zA-Z]$"); // Noncompliant

        UseRegex(new ("^[a-zA-Z]$")); // Noncompliant
        //       ^^^^^^^^^^^^^^^^^^

        void UseRegex(Regex regex) { }
    }
}

public partial class UseCachedRegexFP
{
#if NET7_0_OR_GREATER
    [GeneratedRegex("^[a-zA-Z]$")]
    private static partial Regex CreateCachedRegex();
#else
    private static Regex CreateCachedRegex() => new Regex("^[a-zA-Z]$", RegexOptions.Compiled); // FP
#endif
}
