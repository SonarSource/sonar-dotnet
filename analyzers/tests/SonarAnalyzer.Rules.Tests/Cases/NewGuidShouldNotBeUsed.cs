using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Foo()
        {
            var result = new Guid(); // Noncompliant {{Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this Guid instantiation.}}
//                       ^^^^^^^^^^

            result = Guid.Empty;
            result = Guid.NewGuid();
            result = new Guid(new byte[0]);
        }
    }
}
