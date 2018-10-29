using System;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class InvalidCases
    {
        public static async Task<string> FooAsync(string something) // Noncompliant {{Split this method into two, one handling parameters check and the other handling the asynchronous code.}}
//                                       ^^^^^^^^
        {
            if (something == null) { throw new ArgumentNullException(nameof(something)); }
//                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

            await Task.Delay(1);
            return something + "foo";
        }

        public async void OnSomeEvent(object sender, EventArgs args) // Noncompliant - it might looks weird to throw from some event method but that's valid syntax
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender)); // Secondary
            }

            await Task.Yield();
        }
    }

    public class ValidCases
    {
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

        // See https://github.com/SonarSource/sonar-csharp/issues/1819
        public async Task DoSomethingAsync(string value)
        {
            await Task.Delay(0);

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
    }
}
