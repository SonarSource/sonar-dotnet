using System;
using System.Text.RegularExpressions;

// No process-wide regex timeout is configured here (the AppDomain key is unrelated), so issues are still raised.
AppDomain.CurrentDomain.SetData("SOME_OTHER_SETTING", TimeSpan.FromMilliseconds(100));

void RegexPattern(string input)
{
    _ = new Regex(".+@.+", RegexOptions.None);  // Noncompliant
    _ = Regex.IsMatch(input, "[0-9]+");         // Noncompliant
}
