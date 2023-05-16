using System;
using System.Globalization;

public class Program
{
    public void Method(string value)
    {
        FormattableString.CurrentCulture($"Value: {value}"); // Noncompliant
        //                ^^^^^^^^^^^^^^
        FormattableString.Invariant($"Value: {value}"); // Noncompliant
        //                ^^^^^^^^^

        string.Create(CultureInfo.CurrentCulture, $"Value: {value}"); // Compliant
        string.Create(CultureInfo.InvariantCulture, $"Value: {value}"); // Compliant

        var classImplementingIFormattable = new ClassImplementingIFormattable();
        classImplementingIFormattable.CurrentCulture($"Value: {value}"); // Compliant
        classImplementingIFormattable.Invariant($"Value: {value}"); // Compliant
    }

    public class ClassImplementingIFormattable : IFormattable
    {
        public string ToString(string? format, IFormatProvider? formatProvider) => "";
        public string CurrentCulture(FormattableString formattable) => "";
        public string Invariant(FormattableString formattable) => "";
    }
}
