using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public static class EntryPoint
    {
        public static void Main()
        {
            // Build with OutputKind.ConsoleApplication needs an entry point
        }
    }

    public class TaskConfigureAwait
    {
        public async void Test()
        {
            await Task.Delay(1000); // Compliant, this rule only makes sense in libraries
            await Task.Delay(1000).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(true);

            var t = Task.Delay(1000);

            await GetNumber(); // // Compliant, this rule only makes sense in libraries
            await GetNumber().ConfigureAwait(false);
            await GetNumber().ConfigureAwait(true);

            var t2 = GetNumber();
        }

        private Task<int> GetNumber()
        {
            return Task.FromResult(5);
        }
    }
}
