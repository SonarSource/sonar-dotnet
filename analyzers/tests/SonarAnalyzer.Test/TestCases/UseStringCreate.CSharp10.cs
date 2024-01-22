using System;
using System.Globalization;

class Program
{
    void Method(string value, FormattableString formString)
    {
        FormattableString.CurrentCulture($"Value: {value}"); // Noncompliant {{Use "string.Create" instead of "FormattableString".}}
        //                ^^^^^^^^^^^^^^
        FormattableString.Invariant($"Value: {value}"); // Noncompliant {{Use "string.Create" instead of "FormattableString".}}
        //                ^^^^^^^^^

        string.Create(CultureInfo.CurrentCulture, $"Value: {value}");    // Compliant
        string.Create(CultureInfo.InvariantCulture, $"Value: {value}");  // Compliant

        FormattableString.CurrentCulture(formString); // Compliant
        FormattableString.Invariant(formString);      // Compliant

        var classImplementingIFormattable = new ClassImplementingIFormattable();
        classImplementingIFormattable.CurrentCulture($"Value: {value}"); // Compliant
        classImplementingIFormattable.Invariant();                       // Compliant
    }

    class ClassImplementingIFormattable : IFormattable
    {
        public string ToString(string? format, IFormatProvider? formatProvider) => "";
        public string CurrentCulture(FormattableString formattable) => "";
        public string Invariant() => "";
    }
}

class CustomFormattableString
{
    static class FormattableString
    {
        public static string CurrentCulture(string formattable) => "";
        public static string Invariant(string formattable) => "";
    }

    void Test(string value)
    {
        FormattableString.CurrentCulture($"Value: {value}"); // Compliant
        FormattableString.Invariant($"Value: {value}");      // Compliant
    }
}
