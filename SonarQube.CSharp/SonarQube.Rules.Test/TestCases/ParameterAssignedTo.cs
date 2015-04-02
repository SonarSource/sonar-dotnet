using System;

namespace Tests.Diagnostics
{
    public class ParameterAssignedTo
    {
        void f1(int a)
        {
            a = 42; // Noncompliant
        }

        void f2(int a)
        {
            int tmp = a;
            tmp = 42;
        }

        void f3(ref int a)
        {
            a = 42;
        }

        void f4(out int a)
        {
            a = 42;
        }

        void f5()
        {
        }

        void f6(int a, int b, int c, int d, int e)
        {
          b = 42; // Noncompliant
          e = 0; // Noncompliant
        }

        void f7(this int a)
        {
          a = 42; // Noncompliant
        }

        void f8(Func<int, int> param)
        {
            var l1 = delegate(out int b, int c, ref int d)
            {
                b = 0;
                c = 0; // Noncompliant
                d = 0;
            };

            param = 42; // Noncompliant

            var l2 += (out int foo1, int foo2, ref int foo3) => {
                foo1 = 0;
                foo2 = 0; // Noncompliant
            };

            f8((int x) => x = 0); // Noncompliant
            f8((x) => x = 0); // Noncompliant
            f8(
                (x) =>
                {
                    return 0;
                });
            f8(
                (x) =>
                {
                    x = 0; // Noncompliant
                    return 0;
                });
            f8(
                (int x) =>
                {
                    return 0;
                });
            f8(
                (int x) =>
                {
                    x = 0; // Noncompliant
                    return 0;
                });
        }

        public int this[int index]
        {
            get
            {
                index = 1; // Noncompliant
                return 0;
            }
            set
            {
                index = 1;  // Noncompliant
                value = 45; // Noncompliant
            }
        }

        public delegate void SomeEventHandler();

        public event SomeEventHandler OnSomeEvent;

        public void f8()
        {
            OnSomeEvent += delegate { };
        }
    }
}
