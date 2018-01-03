using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        private object field = null;
        private static object staticField = null;

        public void NotCompliantCases(object o, Exception e)
        {
            o.ToString(); // Noncompliant

            Bar(o); // Compliant, we care about dereference only

            throw e; // Noncompliant
        }

        protected void NotCompliantCases_Nonpublic(object o)
        {
            o.ToString(); // Noncompliant
        }

        private void CompliantCases_Private(object o)
        {
            o.ToString(); // Compliant, not public
        }

        private void CompliantCases_Internal(object o)
        {
            o.ToString(); // Compliant, not public
        }

        public void CompliantCases(bool b, object o1, object o2, object o3, object o4, Exception e)
        {
            if (o1 != null)
            {
                o1.ToString(); // Compliant, we did the check
            }

            o2 = o2 ?? new object();
            o2.ToString(); // Compliant, we coalesce

            if (o3 == null)
            {
                throw new Exception();
            }

            o3.ToString(); // Compliant, we did the check

            if (e != null)
            {
                throw e; // Compliant
            }

            o4?.ToString(); // Compliant, conditional operator

            b.ToString(); // Compliant, bool cannot be null

            var v = null;
            v.ToString(); // Compliant, we don't care about local variables

            field.ToString(); // Compliant

            Program.staticField.ToString(); // Compliant
        }
    }
}
