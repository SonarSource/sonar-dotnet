using System.Text.RegularExpressions;
using System;

AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));

void RegexPattern(string input)
{
    _ = new Regex(".+@.+", RegexOptions.None);  // Noncompliant, FP REGEX_DEFAULT_MATCH_TIMEOUT is set in the AppDomain
    _ = Regex.IsMatch(input, "[0-9]+");         // Noncompliant, FP REGEX_DEFAULT_MATCH_TIMEOUT is set in the AppDomain
}
