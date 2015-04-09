using System;

namespace Tests.Diagnostics
{
    public class ExpressionComplexity
    {
        public bool M()
        {
            var a1 = false;
            var b1 = false ? (true ? (false ? (true ? 1 : 0) : 0) : 0) : 1; // Noncompliant

            var c1 = true || false || true || false || false; // Noncompliant

            var d1 = true && false && true && false && true && true; // Noncompliant

            call(
                a =>
                a = ((a1 ? false : true) || a1 || true && false && true || false)); // Noncompliant

            for (var i = a1 ? (b1==0 ? (c1 ? (d1 ? 1 : 1) : 1) : 1) : 1; i < 1; i++) {} // Noncompliant

            bool[] foo = {
                true && true && true && true && true, // Noncompliant
                true && true && true && true
            };

            var e2 = true | false | true | false;

            var a2 = false ? (true ? (false ? 1 : 0) : 0) : 1;

            var foo2 = new Action(delegate () {
                bool a = true && true;
                bool b = true && true;
                bool c = true && true;
                bool d = true && true;
                bool e = true && true;
                bool f = true && true && true && true && true; // Noncompliant
            });

            var f2 = new Foo2 {
                a = true && true,
                b = true && true,
                c = true && true,
                d = true && true,
            };

            return call(
                true && true && true,
                true && true && true,
                true && // Noncompliant
                true &&
                true &&
                true &&
                true &&
                true);
        }

        private bool call(bool v1, bool v2, bool v3)
        {
            throw new NotImplementedException();
        }

        private bool call(Func<object, object> p0)
        {
            throw new NotImplementedException();
        }
    }

    public class Foo2
    {
        public bool a { get; set; }
        public bool b { get; set; }
        public bool d { get; set; }
        public bool c { get; set; }
    }
}
