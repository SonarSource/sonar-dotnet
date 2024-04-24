using System.Text.RegularExpressions;
using RealRegex = System.Text.RegularExpressions.Regex;

class UseRegexSafeIsMatchNonCompliant
{
    private Regex regex;
    private RealRegex realRegex;

    void InstanceRegex(string content)
    {
        regex.IsMatch(content);         // Noncompliant {{Use 'SafeIsMatch' instead.}}
        //    ^^^^^^^
        regex.IsMatch(content, 0);      // Noncompliant
        regex.Matches(content);         // Noncompliant {{Use 'SafeMatches' instead.}}
        regex.Matches(content, 0);      // Noncompliant
        regex.Match(content);           // Noncompliant {{Use 'SafeMatch' instead.}}
        regex.Match(content, 0);        // Noncompliant
        realRegex.IsMatch(content);     // Noncompliant
        realRegex.IsMatch(content, 0);  // Noncompliant
        realRegex.Matches(content);     // Noncompliant
        realRegex.Matches(content, 0);  // Noncompliant
        realRegex.Match(content);       // Noncompliant
        realRegex.Match(content, 0);    // Noncompliant
        Regex.IsMatch(content, "pattern");                          // Noncompliant {{Use 'SafeRegex.IsMatch' instead.}}
//      ^^^^^
        Regex.IsMatch(content, "pattern", RegexOptions.None);       // Noncompliant
        Regex.Matches(content, "pattern");                          // Noncompliant {{Use 'SafeRegex.Matches' instead.}}
        Regex.Matches(content, "pattern", RegexOptions.None);       // Noncompliant
        Regex.Match(content, "pattern");                            // Noncompliant {{Use 'SafeRegex.Match' instead.}}
        Regex.Match(content, "pattern", RegexOptions.None);         // Noncompliant
        RealRegex.IsMatch(content, "pattern");                      // Noncompliant
        RealRegex.IsMatch(content, "pattern", RegexOptions.None);   // Noncompliant
        RealRegex.Matches(content, "pattern");                      // Noncompliant
        RealRegex.Matches(content, "pattern", RegexOptions.None);   // Noncompliant
        RealRegex.Match(content, "pattern");                        // Noncompliant
        RealRegex.Match(content, "pattern", RegexOptions.None);     // Noncompliant
    }
}

class UseRegexSafeIsMatchCompliant
{
    private class Regex
    {
        public bool IsMatch(string input) => false;
        public MatchCollection Matches(string input) => null;
        public Match Match(string input) => null;
        public static bool IsMatch(string input, string pattern) => false;
        public static MatchCollection Matches(string input, string pattern) => null;
        public static Match Match(string input, string pattern) => null;
    }

    private Regex regex;

    void InstanceRegex(string content)
    {
        regex.IsMatch(content);
        regex.Matches(content);
        regex.Match(content);
        Regex.IsMatch(content, "pattern");
        Regex.Matches(content, "pattern");
        Regex.Match(content, "pattern");
    }
}
