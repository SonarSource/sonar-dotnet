public record Web { } // Noncompliant
public record Accessibility { } // Noncompliant
public record CompliantName { }
public record Runtime(string Parameter) { } // Noncompliant
record IO { } // Compliant (it's not public)

namespace Tests.Diagnostics
{
    public record Linq { } // Noncompliant
}
