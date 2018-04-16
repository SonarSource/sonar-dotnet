using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Foo()
        {
            var result = new Guid(); // Noncompliant {{Please review this use of 'new Guid()'.}}
//                       ^^^^^^^^^^

            result = Guid.Empty;
            result = Guid.NewGuid();
            result = new Guid(new byte[0]);
        }
    }
}
