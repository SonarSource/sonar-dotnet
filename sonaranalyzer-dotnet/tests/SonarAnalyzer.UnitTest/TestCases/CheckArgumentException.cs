using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class CustomArgumentException : ArgumentException
    {
        public CustomArgumentException()
        {
        }
        public CustomArgumentException(string message)
            : base(message)
        {
        }
        public CustomArgumentException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public CustomArgumentException(string message, string paramName)
            : base(message, paramName)
        {
        }
        public CustomArgumentException(string message, string paramName, Exception inner)
            : base(message, paramName, inner)
        {
        }
    }

    class Program
    {
        public int Bar { get; set; }

        void Foo(int a)
        {
            throw new ArgumentException(); // Noncompliant {{Use a constructor overloads that allows a more meaningful exception message to be provided.}}
            throw new ArgumentNullException(); // Noncompliant
            throw new ArgumentOutOfRangeException(); // Noncompliant
            throw new DuplicateWaitObjectException(); // Noncompliant
            throw new CustomArgumentException(); // Compliant - ignored from analysis
        }

        void Foo1(int a)
        {
            throw new ArgumentException("foo"); // Compliant - foo is the message

            throw new ArgumentNullException("foo"); // Noncompliant {{The parameter name 'foo' is not declared in the argument list.}}
            throw new ArgumentOutOfRangeException("foo"); // Noncompliant
            throw new DuplicateWaitObjectException("foo"); // Noncompliant
        }

        void Foo2(int a)
        {
            throw new ArgumentException("a", "foo"); // Noncompliant {{ArgumentException constructor arguments have been inverted.}}
            throw new ArgumentNullException("foo", "a"); // Noncompliant {{ArgumentException constructor arguments have been inverted.}}
            throw new ArgumentOutOfRangeException("foo", "a"); // Noncompliant
            throw new DuplicateWaitObjectException("foo", "a"); // Noncompliant
        }

        void Foo3(int a, Program p)
        {
            throw new ArgumentException("foo", "a");
            throw new ArgumentException("foo", "p.Bar");
            throw new ArgumentException("foo", "p.Test"); // Compliant - cannot detect if sub-element exists
            throw new ArgumentException("foo", "test.Test"); // Noncompliant
            throw new ArgumentNullException("a", "foo");
            throw new ArgumentOutOfRangeException("a", "foo");
            throw new DuplicateWaitObjectException("a", "foo");
        }

        void Foo4(int a)
        {
            var paramName = "foo";
            throw new ArgumentNullException(paramName); // Compliant because we can't validate non const-string
            throw new ArgumentOutOfRangeException(paramName); // Compliant because we can't validate non const-string
            throw new DuplicateWaitObjectException(paramName); // Compliant because we can't validate non const-string
        }

        void Foo5(int a)
        {
            const string paramName = "a";
            throw new ArgumentNullException(paramName); // Compliant
            throw new ArgumentOutOfRangeException(paramName); // Compliant
            throw new DuplicateWaitObjectException(paramName); // Compliant
        }

        void Foo6(int a)
        {
            throw new ArgumentNullException(nameof(Foo5)); // Noncompliant
            throw new ArgumentOutOfRangeException(nameof(Foo5)); // Noncompliant
            throw new DuplicateWaitObjectException(nameof(Foo5)); // Noncompliant
        }

        void Foo7(int a)
        {
            Action<int> res =
                i =>
                {
                    throw new ArgumentNullException("i");
                    throw new ArgumentNullException("a"); // Noncompliant - we are just looking at most direct parent definition
                    throw new ArgumentOutOfRangeException("a"); // Noncompliant
                    throw new DuplicateWaitObjectException("a"); // Noncompliant
                };
        }

        void Foo8(int a)
        {
            Action<int, int> res =
                (i, j) =>
                {
                    throw new ArgumentNullException("i");
                    throw new ArgumentNullException("a"); // Noncompliant - we are just looking at most direct parent definition
                    throw new ArgumentOutOfRangeException("a"); // Noncompliant
                    throw new DuplicateWaitObjectException("a"); // Noncompliant
                };
        }

        void NullMessage(string item)
        {
            string item2;
            throw new ArgumentOutOfRangeException(nameof(item), item, null);
            throw new ArgumentOutOfRangeException(nameof(item2), item2, null); // Noncompliant
        }

        public int Foo9
        {
            get
            {
                throw new ArgumentNullException("value"); // Noncompliant
            }
            set
            {
                throw new ArgumentNullException("value"); // Compliant - value exists in property setters
                throw new ArgumentNullException("foo"); // Noncompliant
            }
        }

        public string this[int a, int b]
        {
            get
            {
                throw new ArgumentNullException("a");
                throw new ArgumentNullException("value"); // Noncompliant
            }
            set
            {
                throw new ArgumentNullException("a");
                throw new ArgumentNullException("value");
            }
        }

        void ComplexCase(int a)
        {
            Action<int> simple = b =>
                {
                    Action<int, int> parenthesized = (c, d) =>
                    {
                        throw new ArgumentNullException("a"); // Noncompliant
                        throw new ArgumentNullException("b"); // Noncompliant
                    };
                };
        }

        void Bar2(int a)
        {
            // See https://github.com/SonarSource/sonar-csharp/issues/1867
            throw new ArgumentNullException(null, string.Empty); // Noncompliant
        }
    }
}
