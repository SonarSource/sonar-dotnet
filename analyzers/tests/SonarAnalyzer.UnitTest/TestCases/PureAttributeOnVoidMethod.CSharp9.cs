using System.Diagnostics.Contracts;

[Pure]
int Compliant(int age) => age;

[Pure] // Noncompliant {{Remove the 'Pure' attribute or change the method to return a value.}}
void Noncompliant()
{
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
