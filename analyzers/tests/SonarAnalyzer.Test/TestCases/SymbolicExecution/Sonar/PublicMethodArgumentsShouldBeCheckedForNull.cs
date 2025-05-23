﻿using System;

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

        protected internal void NotCompliantCases_ProtectedInternal(object o)
        {
            o.ToString(); // Noncompliant
        }

        internal void CompliantCases_Internal(object o)
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

        public void NonCompliant1(object o)
        {
            var c = o?.ToString()?.IsNormalized();
            if (c == null)
            {
                o.GetType().GetMethods();  // Noncompliant
            }
        }

        public void Compliant1(object o)
        {
            var c = o?.ToString()?.IsNormalized();
            if (c != null)
            {
                o.GetType().GetMethods();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ValidatedNotNullAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FooAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class BarAttribute : Attribute { }

    public static class GuardExtensionClass
    {
        public static void GuardExtension([ValidatedNotNull] this string value) { }
        public static void GuardExtensionMoreAttributes([Foo][ValidatedNotNull][Bar] this string value) { }
        public static void GuardExtensionMoreArguments([ValidatedNotNull] this string value, string foo, string bar) { }
    }

    public class GuardedTests
    {
        public void Guarded(string s1, string s2, string s3, string s4, string s5, string s6, string s7)
        {
            Guard1(s1);
            s1.ToUpper();

            Guard2(s2, "s2");
            s2.ToUpper();

            Guard3("s3", s3);
            s3.ToUpper();

            Guard4(s4);
            s4.ToUpper();

            s5.GuardExtension();
            s5.ToUpper();

            s6.GuardExtensionMoreAttributes();
            s6.ToUpper();

            s7.GuardExtensionMoreArguments(null, null);
            s7.ToUpper();
        }

        public void Guard1<T>([ValidatedNotNull] T value) where T : class { }

        public void Guard2<T>([ValidatedNotNull] T value, string name) where T : class { }

        public void Guard3<T>(string name, [ValidatedNotNull] T value) where T : class { }

        public static void Guard4<T>([ValidatedNotNull] T value) where T : class { }
    }

    public class ReproIssue2476
    {
        public void Foo(params string[] infixes)
        {
            if (infixes == null || infixes.Length == 0)
            {
                infixes = new string[1];
            }
            else
            {
                Array.Resize(ref infixes, infixes.Length + 1);
            }
            // more stuff
        }

        public void Method(ref string s, int x) { }
        public void Method1(string infixes)
        {
            if (infixes != null)
            {
                Method(ref infixes, infixes.Length);
                var x = infixes.Length; // Noncompliant when passed by ref can be set to null
            }

        }

        public void Method2(string infixes)
        {
            if (infixes == null)
            {
                Method(ref infixes, infixes.Length); // Noncompliant
                var x = infixes.Length;
            }
        }

        public void Method3(string infixes)
        {
            Method(ref infixes, infixes.Length); // Noncompliant
            var x = infixes.Length; // Noncompliant
        }

        public void Method4(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException("inputString cannot be null or empty", contentType);
            }

            if (contentType.Equals("*"))
            {
                contentType = "*/*";
            }

            var parts = contentType.Split('/', ';');
        }

        public void Method5(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException("inputString cannot be null or empty", contentType);
            }

            if (contentType.Equals("*"))
            {
                contentType = "";
            }

            var parts = contentType.Split('/', ';');
        }

        public void Method6(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException("inputString cannot be null or empty", contentType);
            }

            if (contentType.Equals("*"))
            {
                contentType = null;
            }

            var parts = contentType.Split('/', ';'); // Noncompliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2591
    public class ReproIssue2591
    {
        public string FooWithStringTrim(string name)
        {
            if (name == null)
            {
                name = Guid.NewGuid().ToString("N");
            }

            return name.Trim(); // Noncompliant FP
        }

        public string FooWithStringJoin(string name)
        {
            if (name == null)
            {
                name = Guid.NewGuid().ToString("N");
            }

            return string.Join("_", name.Split(System.IO.Path.GetInvalidFileNameChars())); // Noncompliant FP
        }

        public string FooWithObject(object name)
        {
            if (name == null)
            {
                name = Guid.NewGuid().ToString("N");
            }

            return name.ToString(); // Noncompliant FP
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2670
    public class ReproIssue2670
    {
        public static void BooleanEqualityComparisonIsNullOrEmpty(string argument, bool b)
        {
            if (string.IsNullOrEmpty(argument) == true)
            {
                return;
            }
            if (b)
            {
                int index = argument.LastIndexOf('c'); // Noncompliant FP
            }
        }

        public static void BooleanEqualityComparisonWithIsNullOrWhiteSpace(string argument, bool b)
        {
            if (string.IsNullOrWhiteSpace(argument) == true)
            {
                return;
            }
            if (b)
            {
                int index = argument.LastIndexOf('c'); // Noncompliant FP
            }
        }

        public static void NoComparison(string argument, bool b)
        {
            if (string.IsNullOrEmpty(argument))
            {
                return;
            }
            if (b)
            {
                int index = argument.LastIndexOf('c');
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2775
    public class ReproWithIs
    {
        public override bool Equals(object obj)
        {
            var equals = (obj is string) && (obj.GetHashCode() == GetHashCode()); // Noncompliant FP
            if (equals)
            {
                // do stuff
            }
            return equals;
        }
    }

    public class ReproWithIsNullOrEmpty
    {
        public static string SanitiseIMDbId(string imdbId)
        {
            if (string.IsNullOrEmpty(imdbId))
                return string.Empty;

            if (imdbId.StartsWith(" "))
                imdbId = string.Concat("tt", imdbId);

            if (imdbId.Length != 9) // Noncompliant - FP
                imdbId = string.Empty;

            return imdbId;
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3400
namespace Repro_3400
{
    using System.Text;

    public class Repro_3400
    {
        public void ReassignedFromMethod(StringBuilder parameter)
        {
            parameter = Create();
            parameter.Capacity = 1; // Noncompliant FP
        }

        public void ReassignedFromConstructor(StringBuilder parameter)
        {
            parameter = new StringBuilder();
            parameter.Capacity = 1;
        }

        public void ReassignedFromMethodOut(out StringBuilder parameter)
        {
            parameter = Create();
            parameter.Capacity = 1; // Noncompliant FP
        }

        public void ReassignedFromConstructorOut(out StringBuilder parameter)
        {
            parameter = new StringBuilder();
            parameter.Capacity = 1;
        }

        private StringBuilder Create() => null;
    }
}
