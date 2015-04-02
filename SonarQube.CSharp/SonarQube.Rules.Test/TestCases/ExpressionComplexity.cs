namespace Tests.Diagnostics
{
    public class ExpressionComplexity
    {
        public ExpressionComplexity()
        {
            b = false ? (true ? (false ? (true ? 1 : 0) : 0) : 0) : 1; // Noncompliant

            c = true || false || true || false || false; // Noncompliant

            d = true && false && true && false && true && true; // Noncompliant

            call(
                a =>
                a = ((a ? 0 : 1) || a || true && false && true || false)); // Noncompliant

            for (i = a ? (b ? (c ? (d ? 1 : 1) : 1) : 1) : 1; i < a; i++) {} // Noncompliant

            bool[] foo = {
                true && true && true && true && true, // Noncompliant
                true && true && true && true
            };

            e = true | false | true | false;

            a = false ? (true ? (false ? 1 : 0) : 0) : 1;

            Foo foo = delegate () {
                bool a = true && true;
                bool b = true && true;
                bool c = true && true;
                bool d = true && true;
                bool e = true && true;
                bool f = true && true && true && true && true; // Noncompliant
            };

            Foo f = new Foo {
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
    }
}
