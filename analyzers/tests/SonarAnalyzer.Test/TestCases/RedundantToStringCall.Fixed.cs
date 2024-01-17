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
            var calledOnStringLiteral = "IAmAStringLiteral"; // Fixed
            var s = "foo";
            var t = "fee fie foe " + s;  // Fixed
            t = "fee fie foe " + s.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var u = "" + 1.ToString(); // Compliant, value type
            u = new object() + ""; // Fixed
            u = new object() // Fixed
                + new object().ToString(); // Fixed

            u += new object(); // Fixed
            u = 1.ToString() + 2;

            var x = 1 + 3;

            var v = string.Format("{0}",
                new object()); //Fixed

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
            var two = a + b.ToString();  // First occurance is False Positive, second is True Positive

            var four = a + b.ToString() + c + d;   // Fixed
            // Fixed
            // Fixed
            // Fixed
        }
    }
}
