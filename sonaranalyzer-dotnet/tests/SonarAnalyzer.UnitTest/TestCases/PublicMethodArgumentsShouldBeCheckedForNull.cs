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

    public static class GuardExtensionClass
    {
        public static void GuardExtension([ValidatedNotNull]this string value) { }
    }

    public class GuardedTests
    {
        public void Guarded(string s1, string s2, string s3, string s4, string s5)
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
            s5.ToUpper(); // Noncompliant - FP for extensions having the [ValidatedNotNull] attribute
        }

        public void Guard1<T>([ValidatedNotNull]T value) where T : class { }

        public void Guard2<T>([ValidatedNotNull]T value, string name) where T : class { }

        public void Guard3<T>(string name, [ValidatedNotNull]T value) where T : class { }

        public static void Guard4<T>([ValidatedNotNull]T value) where T : class { }
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

    public class ReproIssue2670
    {
        public static void BooleanEqualityComparison(string argument, bool b)
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

namespace CSharp8
{
    public class NullCoalescenceAssignment
    {
        public void NullCoalescenceAssignment_NotNull(string s1)
        {
            s1 ??= "N/A";
            s1.ToString(); // Compliant
        }

        public void NullCoalescenceAssignment_Null(string s1)
        {
            s1 ??= null;
            s1.ToString(); // Noncompliant
        }

        public void InsideIf(string str)
        {
            if (str != null)
            {
                str ??= null;
                str.ToString(); // Compliant, we know str is not null
            }

            if (str == null)
            {
                str ??= "foo";
                str.ToString(); // Compliant, assigned foo
            }
        }
    }

    public interface IWithDefaultMembers
    {
        decimal Count { get; set; }
        decimal Price { get; set; }

        void Reset(string s)
        {
            s.ToString(); // Noncompliant
        }
    }

    public class LocalStaticFunctions
    {
        public void Method(object arg)
        {
            string LocalFunction(object o)
            {
                return o.ToString(); // Compliant - FN: local functions are not supported by the CFG
            }

            static string LocalStaticFunction(object o)
            {
                return o.ToString(); // Compliant - FN: local functions are not supported by the CFG
            }
        }
    }

    public class Address
    {
        public string Name { get; }

        public string State { get; }

        public void Deconstruct(out string name, out string state) =>
            (name, state) = (Name, State);
    }

    public class Person
    {
        public string Name { get; }

        public Address Address { get; }

        public void Deconstruct(out string name, out Address address) =>
            (name, address) = (Name, Address);
    }

    public class SwitchExpressions
    {
        public void OnlyDiscardBranch_Noncompliant(string s, bool b)
        {
            var result = b switch
            {
                _ => s.ToString() // Noncompliant
            };
        }

        public void MultipleBranches_Noncompliant(string s, int val)
        {
            var result = val switch
            {
                1 => "a",
                2 => s.ToString(), // Noncompliant
                _ => "b"
            };
        }

        public void Nested_Noncompliant(string s, int val, bool condition)
        {
            var result = val switch
            {
                1 => "a",
                2 => condition switch
                {
                    _ => s.ToString() // Noncompliant
                },
                _ => "b"
            };
        }

        public void MultipleBranches_HandleNull(string s, int val)
        {
            var result = s switch
            {
                null => s.ToString(), // Noncompliant
                _ => s.ToString() // Compliant as null was already handled
            };
        }

        public void MultipleBranches_Compliant(string s, int val)
        {
            var result = val switch
            {
                1 => "a",
                2 => s == null ? string.Empty : s.ToString(),
                _ => "b"
            };
        }

        public string MultipleBranches_PropertyPattern(Address address, string s)
        {
            return address switch
            {
                {State: "WA" } addr => s.ToString(), // Noncompliant
                _ => string.Empty
            };
        }

        public string MultipleBranches_PropertyPattern_FP(string s)
        {
            return s switch
            {
                { Length: 5 } => s.ToString(), // Noncompliant - FP we know that the length is 5 so the string cannot be null
                _ => string.Empty
            };
        }

        public string MultipleBranches_RecursivePattern(Person person, string s)
        {
            return person switch
            {
                { Address: {State: "WA" } } pers => s.ToString(), // Noncompliant
                _ => string.Empty
            };
        }

        public string MultipleBranches_TuplePattern(Address address, string s)
         {
             return address switch
             {
                 var (name, state) => s.ToString(), // Compliant - FN
                 _ => string.Empty
             };
         }

        public string MultipleBranches_WhenClause(Address address, string s)
        {
            return address switch
            {
                Address addr when addr.Name.Length > 0 => s.ToString(), // Noncompliant
                Address addr when addr.Name.Length == s.Length => string.Empty, // Noncompliant
                _ => string.Empty
            };
        }

        public string MultipleBranches_VarDeclaration(Address address, string s)
        {
            return address switch
            {
                Address addr => s.ToString(), // Noncompliant
                _ => string.Empty
            };
        }

        public string TwoBranches_NoDefault(bool condition, string s)
        {
            return condition switch
            {
                true => s.ToString(), // Noncompliant
                false => s.ToString() // Noncompliant
            };
        }
    }

    public class SwitchStatement
    {
        public void Test(string s)
        {
            switch (s)
            {
                case null:
                    break;

                default:
                    s.ToString(); // Compliant - the null is handled by the case null branch.
                    break;
            }
        }
    }
}
