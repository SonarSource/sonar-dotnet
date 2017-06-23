using System;

namespace Tests.Diagnostics
{
    public class Web { } // Noncompliant {{Change the name of that type to be different from an existing namespace.}}
//               ^^^
    public enum IO { x };
//              ^^
    public delegate void Runtime(); // Noncompliant
//                       ^^^^^^^

    public interface Linq { } // Noncompliant
//                   ^^^^

    interface System { } // Compliant

    private interface Data { } // Compliant


    interface { }
}
