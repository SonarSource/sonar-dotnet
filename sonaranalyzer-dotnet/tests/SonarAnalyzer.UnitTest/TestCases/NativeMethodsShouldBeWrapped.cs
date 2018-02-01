using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        private static T DoSomething<T>(T x) => x;

        extern private static void Extern0(); // Compliant

        extern private static void Extern1(string s, int x); // Compliant

        extern public static void Extern2(string s, int x); // Noncompliant {{Make this native method private and provide a wrapper.}}
//                                ^^^^^^^

        extern internal protected static void Extern3(string s, int x); // Noncompliant
//                                            ^^^^^^^


        extern private static int Extern4(string s); // Compliant

        public static void Wrapper1(string s, int x) // Noncompliant {{Make this wrapper for native method 'Extern1' less trivial.}}
        {
            Extern1(s, x);
        }

        public static void Wrapper2() // Compliant, no arguments
        {
            Extern0();
        }

        public static int Wrapper3() // Compliant, no arguments
        {
            return DoSomething(false) // simulate some check
                ? Extern0()
                : 5;
        }

        public static void Wrapper4(string s, int x) // Compliant, more than one statement
        {
            Extern1(s, x);
            Extern1(s, x);
        }

        public static void Wrapper5(string s, int x) // Compliant, more than one statement
        {
            if (string.IsNullOrEmpty(s) || x < 0)
            {
                return;
            }
            Extern1(s, x);
        }

        public static void Wrapper6(string s, int x) // Compliant, parameters are not directly passed
        {
            Extern1(s != null ? s : string.Empty, x > 0 ? x : 100);
        }

        public static void Wrapper7(string s, int x) // Compliant, parameters are not directly passed
        {
            Extern1(DoSomething(s), DoSomething(x));
        }

        public static void Wrapper8(string s, int x) => // Noncompliant {{Make this wrapper for native method 'Extern1' less trivial.}}
            Extern1(s, x);

        public static void Wrapper9(string s, int x) => // Compliant, parameters are not directly passed
            Extern1(s.Length > 0 ? s : string.Empty, x > 0 ? x : 100);

        public static void Wrapper10(string s, int x) => // Compliant, parameters are not directly passed
            Extern1(DoSomething(s), DoSomething(x));

        public static int Wrapper11() => // Compliant, no arguments
            DoSomething(false) ? Extern0() : 5; // simulate some check

        private class PrivateClass
        {
            extern public static void Extern(); // Compliant, container class is private
        }

        internal class PrivateClass
        {
            extern public static void Extern(); // Compliant, container class is internal
        }
    }

    public class InvalidSyntax
    {
        extern public void Extern1
        extern public void Extern2;
        extern private void Extern3(int x);
        public void Wrapper
        {
            Extern3(x);
        }
        public void Wrapper(
        {
            Extern3(x);
        }
        public void Wrapper()
        {
            Extern3(x);
        }
    }
}
