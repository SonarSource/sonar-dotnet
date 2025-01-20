using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class InvalidCases
    {
        public static async Task<string> FooAsync(string something) // Noncompliant {{Split this method into two, one handling parameters check and the other handling the asynchronous code.}}
//                                       ^^^^^^^^
        {
            if (something == null) { throw new ArgumentNullException(nameof(something)); }
//                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{This ArgumentException will be raised only after observing the task.}}

            await Task.Delay(1);
            return something + "foo";
        }

        public static async Task<string> IndirectUsageAsync(string something) // Noncompliant
        {
            var exception = new ArgumentNullException(nameof(something));
            if (something == null)
                throw exception; // Secondary
            await Task.Delay(1);
            return something + "foo";
        }

        public static async Task<string> IndirectUsageWithMethodCallAsync(string something) // Noncompliant
        {
            if (something == null)
                throw GetArgumentExpression(nameof(something));
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
            await Task.Delay(1);
            return something + "foo";
        }

        private static ArgumentNullException GetArgumentExpression(string name)
        {
            return new ArgumentNullException(name);
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2665
        public async void OnSomeEvent(object sender, EventArgs args) // Compliant
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            await Task.Yield();
        }

        public void Foo(object sender, EventArgs args) { }
    }

    public class ValidCases
    {
        // https://github.com/SonarSource/sonar-dotnet/issues/6449
        public class Repro_6449
        {
            public Task<int[]> CheckAsync() => Task.FromResult(new int[] { 1 });
            public Task<int> Check2Async() => Task.FromResult(1);

            public async Task HasS4457Async(int request) // Compliant
            {
                var identifierType = (await CheckAsync()).FirstOrDefault(x => x == request);
                if (identifierType == 0)
                    throw new ArgumentException("message");
            }
        }

        public static Task FooAsync(string something)
        {
            if (something == null) { throw new ArgumentNullException(nameof(something)); }

            return FooInternalAsync(something);
        }

        private static async Task<string> FooInternalAsync(string something)
        {
            await Task.Delay(1);
            return something + "foo";
        }

        public async Task DoAsync() // Compliant - no args check
        {
            await Task.Delay(0);
        }

        public async Task FooAsync(int age) // Compliant - the exception doesn't derive from ArgumentException
        {
            if (age == 0)
            {
                throw new Exception("Wrong age");
            }

            await Task.Delay(0);
        }


        public static Task WithLocalFunctionAsync(string something) // Compliant - async part is declared in a sub method
        {
            if (something == null)
            {
                throw new ArgumentNullException(nameof(something));
            }

            return FooLocalFunctionAsync(something);

            async Task<string> FooLocalFunctionAsync(string s)
            {
                await Task.Delay(1);
                return s + "foo";
            }
        }

        public async Task WithFuncAsync()
        {
            Func<int, string> func =
                age =>
                {
                    if (age < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(age)); // Compliant - we don't know where/how the func is used
                    }

                    return "";
                };

            await Task.Delay(0);
        }

        public Task WithAsyncFunc(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            Func<Task> func = async () => await Task.Delay(0);

            return func();
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/1819
        public async Task DoSomethingAsync(string value)
        {
            await Task.Delay(0);

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2665
        private async void OnButtonPressed( object sender, EventArgs e) // Compliant (the compliant solution from rule specs does not apply here)
        {
            if(e == null)
                throw new ArgumentException(nameof(e));

            await Task.Delay(0);
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/4702
        private static async Task Main(string[] args)   // Compliant, should not raise on Main
        {
            if (args.Length == 0)
            {
                throw new ArgumentException(nameof(args));
            }
        }
    }
}
