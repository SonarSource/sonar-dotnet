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
            throw new ArgumentException { Source = null }; // Noncompliant
            throw new ArgumentNullException(); // Noncompliant
            throw new ArgumentOutOfRangeException(); // Noncompliant
            throw new DuplicateWaitObjectException(); // Noncompliant
            throw new CustomArgumentException(); // Compliant - ignored from analysis
        }

        void Foo0(int a)
        {
            var exception = new ArgumentNullException(); // Noncompliant
            throw exception;
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

        void Foo6(int a, int @b)
        {
            throw new ArgumentNullException(nameof(a));
            throw new ArgumentNullException(nameof(Foo5)); // Noncompliant
            throw new ArgumentOutOfRangeException(nameof(Foo5)); // Noncompliant
            throw new DuplicateWaitObjectException(nameof(Foo5)); // Noncompliant

            throw new ArgumentNullException("a");
            throw new ArgumentNullException("@a"); // Noncompliant
            throw new ArgumentNullException(nameof(a));
            throw new ArgumentNullException(nameof(@a));
            throw new ArgumentNullException("b");
            throw new ArgumentNullException(nameof(b));
            throw new ArgumentNullException(nameof(@b));
        }

        void Foo7(int a)
        {
            Action<int> res =
                i =>
                {
                    throw new ArgumentNullException("i");
                    // https://github.com/SonarSource/sonar-dotnet/issues/8319
                    throw new ArgumentNullException("a"); // Noncompliant FP - we are just looking at most direct parent definition
                    throw new ArgumentOutOfRangeException("a"); // Noncompliant FP
                    throw new DuplicateWaitObjectException("a"); // Noncompliant FP
                };
        }

        void Foo8(int a)
        {
            Action<int, int> res =
                (i, j) =>
                {
                    throw new ArgumentNullException("i");
                    // https://github.com/SonarSource/sonar-dotnet/issues/8319
                    throw new ArgumentNullException("a"); // Noncompliant FP - we are just looking at most direct parent definition
                    throw new ArgumentOutOfRangeException("a"); // Noncompliant FP
                    throw new DuplicateWaitObjectException("a"); // Noncompliant FP
                };
        }

        void NullMessage(string item, int Length)
        {
            string item2;
            throw new ArgumentOutOfRangeException(nameof(item), item, null);
            throw new ArgumentOutOfRangeException(nameof(item2), item2, null); // Noncompliant
            throw new ArgumentOutOfRangeException(nameof(item2.Length), Length, null); // Compliant, just weird
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
                throw new ArgumentNullException("Foo9"); // Compliant - it's a property name
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
            // See https://github.com/SonarSource/sonar-dotnet/issues/1867
            throw new ArgumentNullException(null, string.Empty); // Noncompliant {{The parameter name '' is not declared in the argument list.}}
        }

        void Bar3(int a)
        {
            // See https://github.com/SonarSource/sonar-dotnet/issues/1867
            throw new ArgumentNullException("", string.Empty); // Noncompliant {{The parameter name '' is not declared in the argument list.}}
        }

        void Bar4(int a)
        {
            // See https://github.com/SonarSource/sonar-dotnet/issues/1867
            throw new ArgumentNullException("   ", string.Empty); // Noncompliant {{The parameter name '   ' is not declared in the argument list.}}
        }
    }

    class ReproForIssue2488
    {
        private string input;
        public string Response
        {
            get => "";
            set => this.input = value ?? throw new ArgumentNullException(nameof(value));
        }
        public string Request
        {
            get => this.input;
            set => this.input = value ?? throw new ArgumentNullException(nameof(this.Request));
        }

        public string Request2
        {
            get => this.input;
            set => this.input = value ?? throw new ArgumentNullException(nameof(Request2));
        }
    }

    class ReproForIssue2469
    {
        public bool this[string a] => throw new ArgumentNullException(nameof(a)); // Compliant
        public bool this[int a] => throw new ArgumentNullException("c"); // Noncompliant
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4180
    public class Repro_4180
    {
        private string field = null;
        public void Method(MissingType argument) // Error [CS0246]
        {
            var str = "xxx";
            throw new ArgumentNullException(nameof(argument));  // Compliant. Was FP until Roslyn 3.11.0 / VS 16.10.1.
            throw new ArgumentNullException(nameof(str));       // Noncompliant {{The parameter name 'str' is not declared in the argument list.}}
            throw new ArgumentNullException(nameof(field));     // Noncompliant {{The parameter name 'field' is not declared in the argument list.}}
            throw new ArgumentNullException(nameof(argument.argument)); // Compliant
            throw new ArgumentNullException(nameof(str.argument));      // Error [CS1061] Compliant, argument is missing member without a symbol
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4423
    public class Repro_4423
    {
        public void InsideLocalFunction()
        {
            Something(null);

            void Something(string localArg)
            {
                throw new ArgumentNullException(nameof(localArg));   // Compliant
            }
        }

        public void ValidationDoneByLocalMethod(string methodArg)
        {
            ValidateLocal();

            void ValidateLocal()
            {
                if (methodArg == null)
                {
                    throw new ArgumentNullException(nameof(methodArg)); // Noncompliant FP, we should add this specific pattern as an exception (see: https://community.sonarsource.com/t/s3928-with-local-function-false-positive-or-expected-behavior/56003)
                }
            }
        }

        public void LocalMethodValidatingMethodArgument(string methodArg)
        {
            ValidateLocal();
            SendItToSomewhere(ValidateLocal, "Definitely not null"); // This scenario makes the ValidateLocal non-compliant

            void ValidateLocal()
            {
                if (methodArg == null)
                {
                    throw new ArgumentNullException(nameof(methodArg));   // Noncompliant
                }
            }
        }

        public void SendItToSomewhere(Action a, string methodArg)
        {
            if(methodArg != null)
            {
                a(); // This would throw very confusing message: Value cannot be null. (Parameter 'methodArg')'
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/5226
    public class Repro_5226
    {
        public void NamedInverted(int a)
        {
            throw new ArgumentOutOfRangeException(paramName: nameof(a), message: "Sample message");
        }

        public void NamedNotInverted(int a)
        {
            throw new ArgumentOutOfRangeException(message: "Sample message", paramName: nameof(a));
        }

        public void ShuffledPositions(int a)
        {
            throw new ArgumentOutOfRangeException(actualValue: nameof(a), message: "Sample message", paramName: "randomString"); // Noncompliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8319
    public class Repro_8319
    {
        public void Bar(int x)
        {
            Wrapper(() =>
            {
                throw new ArgumentException("blah", nameof(x)); // Noncompliant FP
            });
        }

        static void Wrapper(Action action) => action();
    }
}
