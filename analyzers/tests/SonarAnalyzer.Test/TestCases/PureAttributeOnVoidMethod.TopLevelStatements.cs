using System.Diagnostics.Contracts;

[Pure]
int Compliant(int age) => age;

[Pure] // Noncompliant {{Remove the 'Pure' attribute or change the method to return a value.}}
void Noncompliant()
{
}
