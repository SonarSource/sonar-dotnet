using System;

namespace Tests.Diagnostics
{
    public class MyClass
    {
        public static MyClass operator +(MyClass c, string s)
        {
            return null;
        }
    }
    public class MyBase { }

    public class RedundantToStringCall : MyBase
    {
        public RedundantToStringCall()
        {
            var calledOnStringLiteral = "IAmAStringLiteral".ToString(); // Noncompliant
            var s = "foo";
            var t = "fee fie foe " + s.ToString();  // Noncompliant {{There's no need to call 'ToString()' on a string.}}
//                                    ^^^^^^^^^^^
            t = "fee fie foe " + s.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var u = "" + 1.ToString(); // Compliant, value type
            u = new object().ToString() + ""; // Noncompliant
            u = new object().ToString() // Noncompliant
                + new object().ToString(); // Noncompliant, note: this is why we don't have a global fix

            u += new object().ToString(); // Noncompliant {{There's no need to call 'ToString()', the compiler will do it for you.}}
            u = 1.ToString() + 2;

            var x = 1 + 3;

            var v = string.Format("{0}",
                new object().ToString()); //Noncompliant

            v = string.Format("{0}",
                1.ToString()); // Compliant, value type

            u = 1.ToString();

            var m = new MyClass();
            s = m.ToString() + "";

            s = base.ToString() + "";
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3665
    public class Repro_3665
    {
        public void Repro(Uri a, Uri b, Uri c, Uri d)
        {
            var two = a.ToString() + b.ToString();  // First occurance is False Positive, second is True Positive
//                     ^^^^^^^^^^^
//                                    ^^^^^^^^^^^@-1

            var four = a.ToString() + b.ToString() + c.ToString() + d.ToString();   // Noncompliant, a.ToString() is a False Positive
            // Noncompliant@-1 // b is TP
            // Noncompliant@-2 // c is TP
            // Noncompliant@-3 // d is TP
        }
    }
}
