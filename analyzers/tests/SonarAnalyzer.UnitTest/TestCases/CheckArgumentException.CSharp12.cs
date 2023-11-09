using System;

// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7714
public class PrimaryConstructor(string s, string q)
{
    private readonly string _s = s ?? throw new ArgumentNullException(nameof(s)); // Compliant: s is a class parameter

    public string GetQ()
    {
        ArgumentNullException.ThrowIfNull(s, "something"); // FN
        ArgumentNullException.ThrowIfNull(s, nameof(q)); // Compliant
        return q ?? throw new ArgumentNullException(nameof(q)); // Compliant: q is a class parameter
    }

    public string GetQLambda() => q ?? throw new ArgumentNullException(nameof(q)); // Compliant: q is a class parameter
}

public struct PrimaryConstructorStruct(string s, string q)
{
    private readonly string _s = s ?? throw new ArgumentNullException(nameof(s)); // Compliant: s is a class parameter

    public string GetQ()
    {
        return q ?? throw new ArgumentNullException(nameof(q)); // Compliant: q is a class parameter
    }

    public string GetQLambda() => q ?? throw new ArgumentNullException(nameof(q)); // Compliant: q is a class parameter
}

public class PrimaryConstructor2(string Prop)
{
    public class TestClass
    {
        public static string B { get; }

        // https://github.com/SonarSource/sonar-dotnet/issues/8319
        public string A { get; } = B ?? throw new ArgumentNullException(nameof(Prop)); // Noncompliant FP: we are checking only the first parent (TestClass) 
    }
}
