using System;
using System.Diagnostics.Contracts;

namespace Tests.TestCases
{
    public class PureAttributes
    {
        private int age;

        [Pure] // Noncompliant
        void WithExplicitInParamater(in int age)
        {
            this.age = age;
        }
    }
}

interface IInterface
{
    [Pure] // Noncompliant {{Remove the 'Pure' attribute or change the method to return a value.}}
    public static virtual void DoSomething1(int a) { }

    [Pure] // Noncompliant {{Remove the 'Pure' attribute or change the method to return a value.}}
    public static abstract void DoSomething2(int a);

    [Pure]
    public static virtual int DoSomething3(int a) { return 5; }
}

record Record
{
    Record()
    {
        [Pure] // Noncompliant
//       ^^^^
        void LocalFunction()
        {
            [Pure] // Noncompliant
            void NestedLocalFunction()
            {
            }
        }

        [PureAttribute] // Noncompliant
        void LocalFunction2()
        {
        }

        [Pure]
        int LocalFunction3()
        {
            return 42;
        }

        [Pure]
        void LocalFunction4(ref int param)
        {
        }
    }
}

namespace UserDefined
{
    public class PureAttribute : System.Attribute { }
}


// https://github.com/SonarSource/sonar-dotnet/issues/5117
public class Repro_5117
{
    public void Method()
    {
        [System.Diagnostics.Contracts.Pure] // Noncompliant
        void LocalFunctionPure()
        {
        }

        [UserDefined.Pure] // Compliant, it's user defined attribute
        void LocalFunctionUserDefinedAttribute()
        {
        }

        [System.Diagnostics.Contracts.ContractAbbreviator]  // Qualified name was throwing NRE
        void LocalFunctionOther()
        {
        }
    }
}

public static class Extensions
{
    extension (string s)
    {
        [Pure] // Noncompliant
        public void NonCompliant() { }  

        [Pure]
        public string Compliant() => "42";
    }
}
