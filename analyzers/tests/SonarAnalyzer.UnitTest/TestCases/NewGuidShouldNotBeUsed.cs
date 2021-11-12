using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Foo()
        {
            var g1 = new Guid(); // Noncompliant {{Use 'Guid.NewGuid()', 'Guid.Empty' or the constructor with arguments.}}
//                   ^^^^^^^^^^

            g1 = Guid.Empty;
            g1 = Guid.NewGuid();
            g1 = new Guid(new byte[0]);

            var g2 = default(Guid); // Noncompliant
//                   ^^^^^^^^^^^^^
            var guidQualifiedName = default(System.Guid); // Noncompliant

            var anotherTest = new StringBuilder(); // Checking that the rule raises issue only for the Guid new Object instatiation.
            var anotherTestDefault = default(long);
            var anotherTestDefaultQualifiedName = default(System.String);
        }

        public Guid Get() => default(Guid); // Noncompliant

        private T Test<T>() where T : new()
        {
            return new T(); // Covering the case where the MethodSymbol has not Type
        }
    }
}
