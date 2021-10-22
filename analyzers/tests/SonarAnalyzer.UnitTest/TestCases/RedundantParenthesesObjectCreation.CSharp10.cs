using System;

namespace Net6Poc.TestCases
{
    internal class TestCases
    {
        [NonGeneric()] int A() => 1; // Noncompliant
        public void M()
        {
            [NonGeneric()] int A() => 1; // Noncompliant
            [NonGeneric] int B() => 1;

            [NonGeneric()] int C() => 1; // Noncompliant
            [NonGeneric(), Obsolete] int D() => 1; // Noncompliant

            Action a = [NonGeneric()] async () => { }; // Noncompliant
            Action b = [NonGeneric] async () => { };
        }
    }

    public class NonGenericAttribute : Attribute { }
}
