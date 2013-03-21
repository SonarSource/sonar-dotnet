using System;

class Foo
{
    void foo() { }     // Non-Compliant
    void Foo.foo() {}  // Non-Compliant
    void Foo() { }     // Compliant
    void Foo.Foo() {}  // Compliant
}

interface IFoo
{
    void foo();        // Non-Compliant
    void Foo();        // Compliant
}
