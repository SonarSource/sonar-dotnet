using System;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class InvalidCases
    {
        public static async Task<string> FooAsync(string something) // FN
        {
            ArgumentNullException.ThrowIfNull(something); // FN sec

            await Task.Delay(1);
            return something + "foo";
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2665
        public async void OnSomeEvent(object sender, EventArgs args) // Compliant
        {
            ArgumentNullException.ThrowIfNull(sender);

            await Task.Yield();
        }

        public void Foo(object sender, EventArgs args) { }
    }

    public class ValidCases
    {
        public static Task FooAsync(string something)
        {
            ArgumentNullException.ThrowIfNull(something);

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
            ArgumentNullException.ThrowIfNull(something);

            return FooLocalFunctionAsync(something);

            async Task<string> FooLocalFunctionAsync(string s)
            {
                await Task.Delay(1);
                return s + "foo";
            }
        }

        public Task WithAsyncFunc(string s)
        {
            ArgumentNullException.ThrowIfNull(s);

            Func<Task> func = async () => await Task.Delay(0);

            return func();
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/1819
        public async Task DoSomethingAsync(string value)
        {
            await Task.Delay(0);

            ArgumentNullException.ThrowIfNull(value);
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2665
        private async void OnButtonPressed( object sender, EventArgs e) // Compliant (the compliant solution from rule specs does not apply here)
        {
            ArgumentNullException.ThrowIfNull(e);

            await Task.Delay(0);
        }
    }
}
