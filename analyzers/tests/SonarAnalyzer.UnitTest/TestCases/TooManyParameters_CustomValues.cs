using System;
using System.Runtime.CompilerServices;
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
        public TooManyParameters(int p1, int p2, int p3, int p4) { } // Noncompliant
//                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        public void F1(int p1, int p2, int p3) { }

        public void F2(int p1, int p2, int p3, int p4) { } // Compliant, interface implementation

        public void F1(int p1, int p2, int p3, int p4) { } // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}

        public void F()
        {
            var v1 = new Action<int, int, int>(delegate (int p1, int p2, int p3) { Console.WriteLine(); });
            var v2 = new Action<int, int, int, int>(delegate (int p1, int p2, int p3, int p4) { Console.WriteLine(); }); // Noncompliant {{Delegate has 4 parameters, which is greater than the 3 authorized.}}
            var v3 = new Action(delegate { });
            var v4 = new Action<int, int, int>((int p1, int p2, int p3) => Console.WriteLine());
            var v5 = new Action<int, int, int, int>((int p1, int p2, int p3, int p4) => Console.WriteLine()); // Noncompliant {{Lambda has 4 parameters, which is greater than the 3 authorized.}}
//                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            var v6 = new Action<object, object, object>((p1, p2, p3) => Console.WriteLine());
            var v7 = new Action<object, object, object, object>((p1, p2, p3, p4) => Console.WriteLine()); // Noncompliant
            F2(1, 2, 3, 4);
        }

        // see https://github.com/SonarSource/sonar-dotnet/issues/1459
        // and https://github.com/SonarSource/sonar-dotnet/issues/8156
        // We should not raise for imported methods according to external definition.
        [DllImport("foo.dll")]
        public static extern void Extern(int p1, int p2, int p3, int p4); // Compliant, external definition

        public static extern void ExternNoAttribute(int p1, int p2, int p3, int p4); // Compliant

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void ExternNoStatic(int p1, int p2, int p3, int p4);           // Compliant
    }

    public interface If
    {
        void F1(int p1, int p2, int p3);
        void F2(int p1, int p2, int p3, int p4); // Noncompliant
    }

    public class MyWrongClass
    {
        public MyWrongClass(string a, string b, string c, string d, string e, string f, string g, string h) // Noncompliant {{Constructor has 8 parameters, which is greater than the 3 authorized.}}
//                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        {
        }

        public virtual void Method(int a, int b, int c, int d) // Noncompliant
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

        public override void Method(int a, int b, int c, int d) // Compliant, cannot be changed
        {
        }
    }

    public class SubClass2 : TooManyParameters
    {
        public SubClass2(int p1, int p2, int p3, string s1, string s2, string s3) // Compliant, base class has 3. This adds only 3 new parameters.
            : base(p1, p2, p3) { }

        public SubClass2(int p1, int p2, int p3, string s1, string s2, string s3, string s4) // Noncompliant {{Constructor has 4 new parameters, which is greater than the 3 authorized.}}
            : base(p1, p2, p3) { }
    }

    public class TooManyParametersLocalFunctions
    {
        public void MainMethod(int p1, int p2, int p3)
        {
            string OKNumberOfParameters1(int p1, int p2) => "";
            static void OKNumberOfParameters2(int p1, int p2, int p3) { }

            void TooManyParameters1(int p1, int p2, int p3, int p4) { } // Noncompliant {{Local function has 4 parameters, which is greater than the 3 authorized.}}
            static string TooManyParameters2(int p1, int p2, int p3, int p4, int p5) => ""; // Noncompliant {{Local function has 5 parameters, which is greater than the 3 authorized.}}
        }
    }
}
