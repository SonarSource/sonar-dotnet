using System;

namespace Net6Poc.TestCases
{
    internal class TestCases
    {
        [Generic<int>()] // Noncompliant
        public void M()
        {
            [Generic<int>()] int A() => 1; // Noncompliant
            [Generic<int>] int B() => 1;

            [Generic<int>(), Obsolete()] int C() => 1; // Noncompliant
                                                       // Noncompliant@-1
            [GenericAttribute<int>] int D() => 1;

            Action a = [Generic<int>] async () => { };
            Action b = [Generic<int>()] async () => { }; // Noncompliant
        }
    }

    public class GenericAttribute<T> : Attribute { }
}
