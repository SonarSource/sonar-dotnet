using System;
using System.Collections.Generic;

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}

namespace Tests.Diagnostics
{
    record Program
    {
        void Foo1(int a)
        {
            throw new ArgumentNullException("foo"); // Noncompliant {{The parameter name 'foo' is not declared in the argument list.}}
        }

        void Foo3(int a)
        {
            ArgumentException exception = new ("a", "foo"); // FN arguments have been inverted
            throw exception;
        }

        void Foo4(int a)
        {
            Action<int, int> res =
                (_, _) =>
                {
                    throw new ArgumentNullException("_");
                    throw new ArgumentNullException("a"); // Noncompliant - we are just looking at most direct parent definition
                };
        }

        public int Foo9
        {
            init
            {
                throw new ArgumentNullException("value"); // Noncompliant FP
                throw new ArgumentNullException("foo"); // Noncompliant
            }
        }

        public string this[int a, int b]
        {
            init
            {
                throw new ArgumentNullException("a");
                throw new ArgumentNullException("value"); // Noncompliant FP
            }
        }

        void ComplexCase(int a)
        {
            Action<int> simple = b =>
                {
                    Action<int, int> parenthesized = (_, _) =>
                    {
                        throw new ArgumentNullException("a"); // Noncompliant
                    };
                };
        }
    }

    class ReproForIssue2488
    {
        private string input;
        public string Response
        {
            get => "";
            init => this.input = value ?? throw new ArgumentNullException(nameof(value)); // Noncompliant FP
        }
        public string Request
        {
            get => this.input;
            init => this.input = value ?? throw new ArgumentNullException(nameof(this.Request)); // Noncompliant
        }
    }
}
