using System;

namespace Tests.TestCases
{
    public class MyClass
    {
        void Utf8StringLiterals()
        {
            var a = "test"u8; // Compliant
        }
    }
}
