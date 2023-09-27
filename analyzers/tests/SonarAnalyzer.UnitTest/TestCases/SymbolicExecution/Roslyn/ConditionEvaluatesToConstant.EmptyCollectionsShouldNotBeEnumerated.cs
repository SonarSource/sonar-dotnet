using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Tests
    {

        private int Length { get; }

        void Test(List<string> foo)
        {
            bool result = foo.Contains("foo");

            // This FP is triggered due to S4158, checking for the Length property.
            if (this.Length < 0) // Noncompliant {{Change this condition so that it does not always evaluate to 'False'.}} FP
            { }
        }
    }
}
