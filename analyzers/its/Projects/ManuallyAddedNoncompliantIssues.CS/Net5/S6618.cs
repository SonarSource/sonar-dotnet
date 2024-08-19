using System;

namespace Net5
{
    public class S6618
    {
        public void Method(string value, FormattableString formString)
        {
            FormattableString.CurrentCulture($"Value: {value}"); // Compliant - string.Create with IFormatProvider parameter is not available before .NET 6.0 
        }
    }
}