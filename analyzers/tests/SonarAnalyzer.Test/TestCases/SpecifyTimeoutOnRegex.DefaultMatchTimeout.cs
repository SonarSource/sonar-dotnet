using System.Text.RegularExpressions;
using System;

AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));

void RegexPattern(string input)
{
    _ = new Regex(".+@.+", RegexOptions.None);  // Compliant - REGEX_DEFAULT_MATCH_TIMEOUT is set process-wide (NET-1626)
    _ = Regex.IsMatch(input, "[0-9]+");         // Compliant - REGEX_DEFAULT_MATCH_TIMEOUT is set process-wide (NET-1626)
}
