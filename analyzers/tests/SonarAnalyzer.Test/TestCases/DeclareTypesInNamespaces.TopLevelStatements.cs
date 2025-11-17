var x = 1;

public partial class Program { } // Compliant: https://github.com/SonarSource/sonar-dotnet/issues/5660

class Foo // Noncompliant {{Move 'Foo' into a named namespace.}}
{
    class InnerFoo { } // Compliant: we want to report only on the outer class
}

namespace Tests.Diagnostics
{
    class Program { }   // Compliant
}
