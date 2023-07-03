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
        Parse("01/02/2000");                                                            // Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.Parse' method.}}
        AliasedDateTime.Parse("01/02/2000");                                            // Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.Parse' method.}}
        System.DateTime.Parse("01/02/2000");                                            // Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.Parse' method.}}

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

    void CallWithDeterministicFormatProviders()
    {
        DateTime.Parse("01/02/2000", CultureInfo.InvariantCulture);                 // Compliant
        DateTime.Parse("01/02/2000", CultureInfo.GetCultureInfo("en-US"));          // Compliant
        DateTime.Parse("01/02/2000", formatProviderField);                          // Compliant
        DateTime.Parse("01/02/2000", this.formatProviderField);                     // Compliant
    }

    void CallWithNonDeterministicFormatProviders()
    {
        DateTime.Parse("01/02/2000", CultureInfo.CurrentCulture);                   // Noncompliant
        DateTime.Parse("01/02/2000", CultureInfo.CurrentUICulture);                 // Noncompliant
        DateTime.Parse("01/02/2000", CultureInfo.DefaultThreadCurrentCulture);      // Noncompliant
        DateTime.Parse("01/02/2000", CultureInfo.DefaultThreadCurrentUICulture);    // Noncompliant
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
