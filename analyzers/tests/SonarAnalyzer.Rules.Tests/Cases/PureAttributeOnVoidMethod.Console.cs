// version: CSharp9
using System.Diagnostics.Contracts;
[Pure]
int Compliant(int age) => age;

[Pure] // Compliant - FN
void Noncompliant()
{
}

record Record
{
    Record()
    {
        [Pure] // Compliant - FN
        void LocalFunction()
        {
            [Pure] // Compliant - FN
            void NestedLocalFunction()
            {
            }
        }
    }
}
