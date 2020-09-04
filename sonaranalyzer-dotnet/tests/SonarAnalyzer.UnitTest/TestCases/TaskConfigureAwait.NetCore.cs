using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class TaskConfigureAwait
    {
        public async void Test()
        {
            // This rule is not relevant for .NET Core
            // See https://github.com/SonarSource/sonar-dotnet/issues/2588

            await Task.Delay(1000); // Compliant, there's no SynchronizationContext in .NET Core
            await Task.Delay(1000).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(true);

            var t = Task.Delay(1000);

            await GetNumber(); // Compliant, there's no SynchronizationContext in .NET Core
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
