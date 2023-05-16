using System;
using System.Globalization;

public class Program
{
    void Method(string value)
    {
        FormattableString.CurrentCulture($"Value: {value}"); // Noncompliant
        //                ^^^^^^^^^^^^^^
        FormattableString.Invariant($"Value: {value}"); // Noncompliant
        //                ^^^^^^^^^

        string.Create(CultureInfo.CurrentCulture, $"Value: {value}"); // Compliant
        string.Create(CultureInfo.InvariantCulture, $"Value: {value}"); // Compliant
    }
}
