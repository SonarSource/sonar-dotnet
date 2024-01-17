using System;
using System.Globalization;
using System.Linq;
using AliasedDateTime = System.DateTime;
using static System.DateTime;

class Test
{
    readonly IFormatProvider formatProviderField = new CultureInfo("en-US");

    void DifferentSyntaxScenarios()
    {
        _ = DateTime.Parse("01/02/2000");                                               // Noncompliant
        //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        DateTime.Parse("01/02/2000");                                                   // Noncompliant

        // using static and using alias
        Parse("01/02/2000");                                                            // Noncompliant {{Use a format provider when parsing date and time.}}
        AliasedDateTime.Parse("01/02/2000");                                            // Noncompliant
        System.DateTime.Parse("01/02/2000");                                            // Noncompliant

        if (DateTime.TryParse("01/02/2000", out var parsedDate))                        // Noncompliant
        {
        }

        DateTime.Parse(provider: null, s: "02/03/2000", styles: DateTimeStyles.None);   // Noncompliant

        _ = DateTime.Parse("01/02/2000").AddDays(1);                                    // Noncompliant

        _ = new[] { "01/02/2000" }.Select(x => DateTime.Parse(x));                      // Noncompliant

        void InnerMethod()
        {
            DateTime.Parse("01/02/2000");                                               // Noncompliant
        }
    }

    void CallWithNullIFormatProvider()
    {
        DateTime.Parse("01/02/2000", null);                                         // Noncompliant
        DateTime.Parse("01/02/2000", null, DateTimeStyles.None);                    // Noncompliant

        DateTime.Parse("01/02/2000", (null));                                       // FN
        DateTime.Parse("01/02/2000", (true ? (IFormatProvider)null : null));        // FN

        IFormatProvider nullFormatProvider = null;
        DateTime.Parse("01/02/2000", nullFormatProvider);                           // FN
    }

    void CallWithFormatProvider()
    {
        DateTime.Parse("01/02/2000", CultureInfo.InvariantCulture);                 // Compliant
        DateTime.Parse("01/02/2000", CultureInfo.CurrentCulture);                   // Compliant
        DateTime.Parse("01/02/2000", CultureInfo.GetCultureInfo("en-US"));          // Compliant
        DateTime.Parse("01/02/2000", formatProviderField);                          // Compliant
        DateTime.Parse("01/02/2000", this.formatProviderField);                     // Compliant
    }

    void ParseMethodsOfNonTemporalTypes()
    {
        int.Parse("1");                                 // Compliant - this rule only deals with temporal types
        double.TryParse("1.1", out var parsedDouble);
    }
}

class CustomTypeCalledDateTime
{
    public struct DateTime
    {
        public static DateTime Parse(string s) => new DateTime();
    }

    CustomTypeCalledDateTime()
    {
        _ = DateTime.Parse("01/02/2000");               // Compliant - this is not System.DateTime
    }
}
