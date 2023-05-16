using System;
using System.Globalization;

public class Program
{
    void Method(string value)
    {
        FormattableString.CurrentCulture($"Value: {value}"); // Compliant (applies to .NET versions after .NET 6, when these string.Create overloads were introduced)
        FormattableString.Invariant($"Value: {value}"); // Compliant (applies to .NET versions after .NET 6, when these string.Create overloads were introduced)
    }
}
