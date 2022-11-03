using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        private object field = null;
        private static object staticField = null;

        public void NotCompliantCases(object[] o)
        {
            o[1].ToString(); // Noncompliant {{Refactor this method to add validation of parameter 'o' before using it.}}
        }

        public void Compliant(object[] o)
        {
            if (o is [not null, not null])
            {
                o.ToString(); // Compliant
                o[1].ToString(); // Compliant
            }
        }
    }

    public interface ISomeInterface
    {
        public static virtual void NotCompliantCases(object o, Exception e)
        {
            o.ToString(); // Noncompliant {{Refactor this method to add validation of parameter 'o' before using it.}}
        }
    }
}
