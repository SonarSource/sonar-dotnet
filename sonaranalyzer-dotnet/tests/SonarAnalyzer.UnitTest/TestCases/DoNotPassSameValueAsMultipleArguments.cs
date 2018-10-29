using System;

namespace Tests.Diagnostics
{
    interface IA { }
    class A : IA { }

    class Foo
    {
        public void Foo2(decimal a, decimal b) { }
        public void Foo2(double a, double b) { }
        public void Foo2(bool a, bool b) { }
        public void Foo2(string a, string b) { }
        public void Foo2(int a, int b) { }

        public void DifferentUsages(IA a, A b) { }
        public void Foo2(A a, A b) { }

        public void Foo5(int a, int b, int c, int d, int e) { }
        public void Bar(string a, int b) { }

        public void FooInt(int x, int y) { }

        private string x;

        public void Test()
        {
            int x = 0, y = 1;

            Foo5(x, x, x, x, x);
//                  ^ Noncompliant    {{Verify that this is the intended value; it is the same as the 1st argument.}}
//               ^ Secondary@-1
//                     ^ Noncompliant@-2    {{Verify that this is the intended value; it is the same as the 1st argument.}}
//               ^ Secondary@-3
//                        ^ Noncompliant@-4    {{Verify that this is the intended value; it is the same as the 1st argument.}}
//               ^ Secondary@-5
//                           ^ Noncompliant@-6    {{Verify that this is the intended value; it is the same as the 1st argument.}}
//               ^ Secondary@-7

            Foo5(x, 1, 1, 1, x); // Noncompliant {{Verify that this is the intended value; it is the same as the 1st argument.}}
// Secondary@-1
            Foo5(1, x, 1, 1, x); // Noncompliant {{Verify that this is the intended value; it is the same as the 2nd argument.}}
// Secondary@-1
            Foo5(1, 1, x, 1, x); // Noncompliant {{Verify that this is the intended value; it is the same as the 3rd argument.}}
// Secondary@-1
            Foo5(1, 1, 1, x, x); // Noncompliant {{Verify that this is the intended value; it is the same as the 4th argument.}}
// Secondary@-1

            Foo2(true, true);
            bool b = true;
            Foo2(b, b); // Noncompliant
// Secondary@-1

            Bar(this.x, x);

            FooInt(x, y);
            Foo2(x.ToString(), y.ToString());
            Foo2("x", "x");
            Foo2(@"x", "x");
            Foo2("x", $"x");
            Foo2(x.ToString(), x.ToString()); // Noncompliant
// Secondary@-1

            Foo foo1 = null, foo2 = null;
            Foo2(foo1.x, foo1.x); // Noncompliant
// Secondary@-1
            Foo2(foo1.x, /* My comment */ foo1.x); // Noncompliant
// Secondary@-1;
            Foo2(foo1.x, foo2.x);

            Foo2(-1, -1);
            Foo2(1.0, 1.0);
            Foo2(1m, 1m);

            A a = new A();
            DifferentUsages(a, a);
            Foo2(a, a); // Noncompliant
// Secondary@-1;
        }
    }
}
