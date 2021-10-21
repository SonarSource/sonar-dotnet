using System;

namespace Net6Poc.TestCases
{
    internal class TestCases
    {
        [Generic<int>()] // Noncompliant
        public void M()
        {
            [NonGeneric()] int A() => 1; // Noncompliant
            [NonGeneric] int B() => 1;

            [NonGeneric(), GenericAttribute<int>()] int C() => 1; // Noncompliant
                                                                  // Noncompliant@-1
            [NonGeneric(), GenericAttribute<int>] int D() => 1; // Noncompliant

            Action a =[NonGeneric()] async () => { }; // Noncompliant
            Action b =[NonGeneric] async () => { };
            Action c = [Generic<int>()] async () => { }; // Noncompliant
            Func<int> d =[NonGeneric()] () => 1; // Noncompliant
            Func<int> e =[Generic<int>]() => 1;
        }
    }

    public class GenericAttribute<T> : Attribute { }
    public class NonGenericAttribute : Attribute { }
}
