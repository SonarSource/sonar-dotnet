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
