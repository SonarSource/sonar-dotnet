using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public class TooManyParameters : If
    {
        public int this[int a, int b, int c, int d]
        {
            get
            {
                return a;
            }
        }

        ~TooManyParameters()
        {
        }

        public TooManyParameters(int p1, int p2, int p3) { }
        public TooManyParameters(int p1, int p2, int p3, int p4) { }

        public void F1(int p1, int p2, int p3) { }

        public void F2(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10) { }

        public void F1(int p1, int p2, int p3, int p4) { }

        public void F()
        {
            var v1 = new Action<int, int, int>(delegate(int p1, int p2, int p3) { Console.WriteLine(); });
            var v2 = new Action<int, int, int, int>(delegate(int p1, int p2, int p3, int p4) { Console.WriteLine(); });
            var v3 = new Action(delegate { });
            var v4 = new Action<int, int, int>((int p1, int p2, int p3) => Console.WriteLine());
            var v5 = new Action<int, int, int, int>((int p1, int p2, int p3, int p4) => Console.WriteLine());
            var v6 = new Action<object, object, object>((p1, p2, p3) => Console.WriteLine());
            var v7 = new Action<object, object, object, object>((p1, p2, p3, p4) => Console.WriteLine());
            F2(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        // see https://github.com/SonarSource/sonar-dotnet/issues/1459
        // We should not raise for imported methods according to external definition.
        [DllImport("foo.dll")]
        public static extern void Extern(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10); // Compliant, external definition
    }

    public interface If
    {
        void F1(int p1, int p2, int p3);
        void F2(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10); // Noncompliant  {{Method has 10 parameters, which is greater than the 7 authorized.}}
    }

    public class MyWrongClass
    {
        public MyWrongClass(string a, string b, string c, string d, string e, string f, string g, string h) // Noncompliant {{Constructor has 8 parameters, which is greater than the 7 authorized.}}
//                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        {
        }
    }

    public class SubClass : MyWrongClass
    {
        // See https://github.com/SonarSource/sonar-dotnet/issues/1015
        // We should not raise when parent base class forces usage of too many args
        public SubClass(string a, string b, string c, string d, string e, string f, string g, string h) // Compliant (base class requires them)
            : base(a, b, c, d, e, f, g, h)
        {
        }

        public SubClass()
            : base("", "", "", "", "", "", "", "")
        { }
    }

    public class SubClass2 : TooManyParameters
    {
        public SubClass2(int p1, int p2, int p3, string s1, string s2)
            : base(p1, p2, p3)
        {

        }
    }

    public class WithLocalFunctions
    {
        public void Method()
        {
            void F1(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10) { } // Noncompliant
            static void F2(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10) { } // Noncompliant
        }
    }
}
