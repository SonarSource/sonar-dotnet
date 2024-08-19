using System;
using System.Collections.Generic;

public interface IS4004
{
    static abstract IDictionary<object, object> Items { get; set; } // Noncompliant
    static abstract ICollection<string> CollectionInit { get; }
}

public class S4004InterfaceImplicit : IS4004
{
    public static IDictionary<object, object> Items { get; set; }  // Compliant enforced by interface (https://github.com/SonarSource/sonar-dotnet/issues/2606)

    public static ICollection<string> CollectionInit { get; }
}
