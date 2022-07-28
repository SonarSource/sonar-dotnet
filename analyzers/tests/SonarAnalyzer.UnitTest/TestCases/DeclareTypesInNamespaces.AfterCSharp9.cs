var x = 1;

public partial class Program { } // Compliant, See: https://github.com/SonarSource/sonar-dotnet/issues/5660

public class Program { } // Error [CS0260]: Missing partial modifier on declaration of type 'Program'; another partial declaration of this type exists on line 5
                         // Noncompliant@-1

class Foo // Noncompliant {{Move 'Foo' into a named namespace.}}
{
    class InnerFoo { } // Compliant - we want to report only on the outer class
}

record Bar // Noncompliant
{
    record InnerBar { } // Compliant - we want to report only on the outer record
}

record PositionalRecord(string FirstParam, string SecondParam); // Noncompliant

public interface InterfaceInTopLevelStatement { } // Noncompliant

namespace Tests.Diagnostics
{
    class Program { }

    record Record { }

    record PositionalRecordInNamespace(string FirstParam, string SecondParam);

    struct MyStruct { }

    public interface MyInt { }

    public enum Enu
    {
        Test
    }
}
