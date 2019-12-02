using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public static class ParameterAssignedToStatic
    {
        static void f7(this int a)
        {
            a = 42; // Noncompliant
//          ^

            try
            {

            }
            catch (Exception exc)
            {
                exc = new Exception(); // Noncompliant {{Introduce a new variable instead of reusing the parameter 'exc'.}}
                var v = 5;
                v = 6;
                throw exc;
            }
        }

        static void f0(this string x)
        {
            string y = x;
            x = "x"; // compliant, but weird
        }
    }

    public class ParameterAssignedTo
    {
        void f00(string x)
        {
            int tmp = x.Length;
            x = "foo";
        }

        void f01(string x)
        {
            int tmp = x.Length;
            tmp = 5;
            x = "1";
        }

        void f02(int x)
        {
            f1(x);
            x = 1;
        }

        void f03(int x)
        {
            x += x;
        }

        void f04(int x)
        {
            x -= x;
        }

        void f05(int x)
        {
            x *= x;
        }

        void f06(int x)
        {
            x <<= x;
        }

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

        static void f5()
        {
        }

        void f6(int a, int b, int c, int d, int e)
        {
            b = 42; // Noncompliant
            e = 0; // Noncompliant
        }


        delegate void d1(out int b, int c, ref int d);

        private event d1 e;

        void f8(Func<int, int> param)
        {
            d1 dd = delegate (out int b, int c, ref int d)
            {
                b = 0;
                c = 0; // Noncompliant
                d = 0;
            };

            param = i => 42; // Noncompliant

            e += delegate (out int foo1, int foo2, ref int foo3)
            {
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

        public void f9()
        {
            OnSomeEvent += delegate { };
        }

        public void f10(Func<int, int> param)
        {
            Func<int, int> tmp = param;
            param = i => 42;
        }

        public void f11(int a, int b, int c, int d, bool e, int f)
        {
            a++;
            ++b;
            --c;
            d--;
            !e; // Error [CS0201] - expression used as statement
            ~f; // Error [CS0201] - expression used as statement
        }

        public int f12(int param)
        {
            param = param + 1;

            return param;
        }

        public void f13(string x)
        {
            var baz = x == null;
            if (baz)
            {
                x = "";
            }
        }

        public void f14(string x)
        {
            (((x))) = ""; // Noncompliant
        }

        public void f15(string x)
        {
            string y = ((x));
            ((x)) = "";
        }

        public string f16(string x)
        {
            if (x == null)
            {
                x = "";
            }
            return x;
        }

        public string f17(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = "(empty)";
            }

            return text;
        }

    }

    public class ExceptionHandling
    {
        void foo()
        {
            var list = new List<string>();
            try { }
            catch (Exception e)
            {
                while (e != null)
                {
                    list.Add(FormatMessage(e));
                    e = e.InnerException;
                }
            }
        }

        void quix(Exception e)
        {
            if (e != null)
            {
                FormatMessage(e);
                e = new Exception("");
            }
        }

        void serialized()
        {
            try
            {

            }
            catch (Exception exSerial)
            {
                FormatMessage(exSerial);
            }
            try
            {

            }
            catch (Exception exSerial)      //Same name as previous statement
            {
                exSerial = new Exception("Obfuscation");    //Noncompliant
            }
        }

        void nested()
        {
            try
            {

            }
            catch (Exception exOuter)
            {
                try
                {
                    FormatMessage(exOuter);
                }
                catch (Exception exInner)
                {
                    exOuter = new Exception("Compliant");
                    exInner = exOuter;  //Noncompliant
                }
            }
        }

        private string FormatMessage(Exception e) => "";
    }
}
