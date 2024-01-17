SomeClass x;
x = new ("1");              // Compliant, can't see non-param overload
x = new ("1", "s1");        // Compliant, can't see non-param overload
x = new (null, "s1");       // Compliant, can't see non-param overload
x = new ("1", "s1", "s2");  // Compliant, can't see non-param overload
x = new (null, "s1", "s2"); // Compliant, can't see non-param overload

x.PrivateOverload("42");                           // Compliant
x.PrivateOverload("42", "s1");                     // Compliant
x.PrivateOverload(null, "s1");                     // Compliant
x.PrivateOverload(null, new[] { "s2" });           // Compliant
x.PrivateOverload("42", "s1", "s2");               // Compliant

x.ProtectedOverload("s1");                         // Compliant
x.ProtectedOverload("s1", "s2");                   // Compliant
x.ProtectedOverload(null, "s2");                   // Compliant
x.ProtectedOverload(null, new[] { "s2" });         // Compliant
x.ProtectedOverload("42", "s1", "s2");             // Compliant

x.PrivateProtectedOverload("s1");                  // Compliant
x.PrivateProtectedOverload("s1", "s2");            // Compliant
x.PrivateProtectedOverload(null, "s2");            // Compliant
x.PrivateProtectedOverload(null, new[] { "s2" });  // Compliant
x.PrivateProtectedOverload("42", "s1", "s2");      // Compliant

x.ProtectedInternalOverload("s1");                 // Compliant
x.ProtectedInternalOverload("s1", "s2");           // Noncompliant
x.ProtectedInternalOverload(null, "s2");           // Noncompliant
x.ProtectedInternalOverload(null, new[] { "s2" }); // Compliant
x.ProtectedInternalOverload("42", "s1", "s2");     // Compliant

x.InternalOverload("s1");                          // Compliant
x.InternalOverload("s1", "s2");                    // Noncompliant
x.InternalOverload(null, "s2");                    // Noncompliant
x.InternalOverload(null, new[] { "s2" });          // Compliant
x.InternalOverload("42", "s1", "s2");              // Compliant

public class SomeClass
{
    private SomeClass(object a, string b) { }
    private SomeClass(string a, string b) { }
    public SomeClass(string a, params string[] bs) { }

    private int PrivateOverload(object a, string b) => 42;
    public int PrivateOverload(string a, params string[] bs) => 42;

    protected int ProtectedOverload(object a, string b) => 42;
    public int ProtectedOverload(string a, params string[] bs) => 42;

    private protected int PrivateProtectedOverload(object a, string b) => 42;
    public int PrivateProtectedOverload(string a, params string[] bs) => 42;

    protected internal int ProtectedInternalOverload(object a, string b) => 42;
    public int ProtectedInternalOverload(string a, params string[] bs) => 42;

    internal int InternalOverload(object a, string b) => 42;
    public int InternalOverload(string a, params string[] bs) => 42;

    protected virtual int OverriddenAsProtected(object a, string b) => 42;
    public int OverriddenAsProtected(string a, params string[] bs) => 42;

    protected int ShadowedAsPublic(object a, string b) => 42;
    public int ShadowedAsPublic(string a, params string[] bs) => 42;

    protected int ShadowedAsProtectedInternal(object a, string b) => 42;
    public int ShadowedAsProtectedInternal(string a, params string[] bs) => 42;
}
