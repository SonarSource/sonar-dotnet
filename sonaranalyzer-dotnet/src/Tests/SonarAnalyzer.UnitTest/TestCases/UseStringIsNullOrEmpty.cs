using System;

namespace Tests.TestCases
{
    class UseStringIsNullOrEmpty
    {
        public static readonly string StaticString = "";
        public const string ConstEmptyString = "";

        public void Test(string value, string otherValue)
        {
            if (value.Equals("")) // Noncompliant {{Use 'string.IsNullOrEmpty()' instead of comparing to empty string.}}
//              ^^^^^^^^^^^^^^^^
            { }

            if (StaticString.Equals("")) // Noncompliant {{Use 'string.IsNullOrEmpty()' instead of comparing to empty string.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^
            { }

            if (string.Empty.Equals(value)) // Noncompliant {{Use 'string.IsNullOrEmpty()' instead of comparing to empty string.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^
            { }

            if (value.Equals(string.Empty)) // Noncompliant {{Use 'string.IsNullOrEmpty()' instead of comparing to empty string.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^
            { }

            if (ConstEmptyString.Equals(value)) // Noncompliant {{Use 'string.IsNullOrEmpty()' instead of comparing to empty string.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            { }

            if ("".Equals(value)) // Noncompliant {{Use 'string.IsNullOrEmpty()' instead of comparing to empty string.}}
//              ^^^^^^^^^^^^^^^^
            { }

            if (value.Equals(ConstEmptyString)) // Noncompliant {{Use 'string.IsNullOrEmpty()' instead of comparing to empty string.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            { }

            if (value == "") // Compliant
            { }

            if (value.CompareTo("") == 0) // Compliant
            { }

            if (value.Equals(StaticString)) // Compliant
            { }

            if (value.Equals(null)) // Compliant
            { }

            if (value.Equals(otherValue)) // Compliant
            { }

            if ("some value".Equals(value)) // Compliant
            { }
        }
    }
}
