using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ObjectCreatedDropped
    {
        public object Method1()
        {
            new object(); // Noncompliant

            new ObjectCreatedDropped(); // Noncompliant

            var x = new object();

            var y = new ObjectCreatedDropped();

            var objects = new object[] { new object(), new ObjectCreatedDropped() };

            if (true)
                new Exception(); // Noncompliant

            var func = new Func<object>(() => new object());

            func = new Func<object>(() =>
            {
                var o = new object();
                new object(); // Noncompliant
                return o;
            });

            var xx = func();

            return new object();
        }
    }
}
