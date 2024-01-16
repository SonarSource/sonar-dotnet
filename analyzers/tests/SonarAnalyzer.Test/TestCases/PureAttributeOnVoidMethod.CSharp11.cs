using System.Diagnostics.Contracts;

interface IInterface
{
    [Pure] // Noncompliant {{Remove the 'Pure' attribute or change the method to return a value.}}
    public static virtual void DoSomething1(int a) { }

    [Pure] // Noncompliant {{Remove the 'Pure' attribute or change the method to return a value.}}
    public static abstract void DoSomething2(int a);

    [Pure]
    public static virtual int DoSomething3(int a) { return 5; }
}
