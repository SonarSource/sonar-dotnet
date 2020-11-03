void LocalFoo<T1, T2, T3>() { }
void LocalBar<T1, T2, T3, T4>() { } // Compliant - FN

record Bar
{
    public void Foo<T1, T2, T3>() { }
    public void Foo<T1, T2, T3, T4>() { } // Noncompliant  {{Reduce the number of generic parameters in the '.Foo' method to no more than the 3 authorized.}}
//              ^^^
// Although an issue is raised, the above message is not correct. There should be 'Bar.Foo' instead of '.Foo'.
}

record Bar<T1, T2, T3> { }
record Bar<T1, T2, T3, T4> { } // Compliant - FN
