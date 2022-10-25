using System;

namespace Tests.TestCases
{
    class UseStringIsNullOrEmpty
    {
        public const string ConstEmptyString = "";
        public const string InterPolatedString = $"{ConstEmptyString}";

        public void Test(string value)
        {
            if (value.Equals(InterPolatedString)) // Noncompliant {{Use 'string.IsNullOrEmpty()' instead of comparing to empty string.}}
            { }
        }
    }
}
