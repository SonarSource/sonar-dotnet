var arg = new nint(42); // Compliant, variable declaration
Foo(new nint(42)); // Compliant, constructor

for (nint i = 42; i < 420; i++) // Noncompliant
{ }

for (nuint i = 42; i < 420; i++) // Noncompliant
{ }

void Foo(nint v) { }
