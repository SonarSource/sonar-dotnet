using System;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class InvalidCases
    {
        public static async Task<string> FooAsync(string something) // Noncompliant
        {
            ArgumentNullException.ThrowIfNull(something);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

            await Task.Delay(1);
            return something + "foo";
        }

        public static async Task<string> ThrowExpressionAsync(string something) // FN #6369
        {
            _ = something ?? throw new ArgumentNullException();

            await Task.Delay(1);
            return something + "foo";
        }
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
    }
}
