Repro5430.SomeClass x;
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

// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/5430
namespace Repro5430
{
    public class SomeClass
    {
        private SomeClass(object a, string b) { }
        private SomeClass(string a, string b) { }
        public SomeClass(string a, params string[] bs) { }

        private int PrivateOverload(object a, string b) => 1492;
        public int PrivateOverload(string a, params string[] bs) => 1606;

        protected int ProtectedOverload(object a, string b) => 1493;
        public int ProtectedOverload(string a, params string[] bs) => 1607;

        private protected int PrivateProtectedOverload(object a, string b) => 1494;
        public int PrivateProtectedOverload(string a, params string[] bs) => 1608;

        protected internal int ProtectedInternalOverload(object a, string b) => 1495;
        public int ProtectedInternalOverload(string a, params string[] bs) => 1609;

        internal int InternalOverload(object a, string b) => 1496;
        public int InternalOverload(string a, params string[] bs) => 1610;

        protected virtual int OverriddenAsProtected(object a, string b) => 1497;
        public int OverriddenAsProtected(string a, params string[] bs) => 1611;

        protected int ShadowedAsPublic(object a, string b) => 1498;
        public int ShadowedAsPublic(string a, params string[] bs) => 1612;

        protected int ShadowedAsProtectedInternal(object a, string b) => 1499;
        public int ShadowedAsProtectedInternal(string a, params string[] bs) => 1613;
    }
}
