// Fix for https://github.com/SonarSource/sonar-dotnet/issues/6836
public partial class Program // Compliant: partial of the compiler-generated Program class (top-level statements) in a separate file
{
    private Program() { }
}

public partial class NotProgram { } // Noncompliant {{Move 'NotProgram' into a named namespace.}}
