using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        private object field = null;
        private static object staticField = null;

        public void NotCompliantCases(object o, Exception e)
        {
            o.ToString(); // Noncompliant {{Refactor this method to add validation of parameter 'o' before using it.}}

            Bar(o); // Compliant, we care about dereference only

            throw e; // Noncompliant
        }

        public void Bar(object o) { }

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

            object v = null;
            v.ToString(); // Compliant, we don't care about local variables

            field.ToString(); // Compliant

            Program.staticField.ToString(); // Compliant
        }

        public void MoreCompliantCases(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
            {
                s1.ToString(); // Noncompliant, could be null
            }
            else
            {
                s1.ToString(); // Compliant
            }

            if (string.IsNullOrWhiteSpace(s2))
            {
                s2.ToString(); // Noncompliant, could be null
            }
            else
            {
                s2.ToString(); // Compliant
            }
        }

        public Program(int i) { }

        public Program(string s) : this(s.Length) { }   // Noncompliant {{Refactor this constructor to avoid using members of parameter 's' because it could be null.}}
    }

    public class GuardedTests
    {
        public void Guarded(string s1, string s2, string s3)
        {
            Guard1(s1);
            s1.ToUpper();

            Guard2(s2, "s2");
            s2.ToUpper();

            Guard3("s3", s3);
            s3.ToUpper();
        }

        public void Guard1<T>([ValidatedNotNull]T value) where T : class { }

        public void Guard2<T>([ValidatedNotNull]T value, string name) where T : class { }

        public void Guard3<T>(string name, [ValidatedNotNull]T value) where T : class { }

        [AttributeUsage(AttributeTargets.Parameter)]
        public sealed class ValidatedNotNullAttribute : Attribute { }
    }
}
