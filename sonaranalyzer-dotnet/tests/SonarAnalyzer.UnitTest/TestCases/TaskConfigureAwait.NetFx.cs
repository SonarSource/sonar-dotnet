using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class TaskConfigureAwait
    {
        public async void Test()
        {
            await Task.Delay(1000); // Noncompliant {{Add '.ConfigureAwait(false)' to this call to allow execution to continue in any thread.}}
//                ^^^^^^^^^^^^^^^^
            await Task.Delay(1000).ConfigureAwait(false); // Compliant
            await Task.Delay(1000).ConfigureAwait(true); // Compliant, we assume that there is a reason to explicitly specify context switching

            var t = Task.Delay(1000);

            await GetNumber(); // Noncompliant {{Add '.ConfigureAwait(false)' to this call to allow execution to continue in any thread.}}
//                ^^^^^^^^^^^

            await GetNumber().ConfigureAwait(false); // Compliant
            await GetNumber().ConfigureAwait(true); // Compliant

            var t2 = GetNumber();
        }

        private Task<int> GetNumber()
        {
            return Task.FromResult(5);
        }
    }
}
