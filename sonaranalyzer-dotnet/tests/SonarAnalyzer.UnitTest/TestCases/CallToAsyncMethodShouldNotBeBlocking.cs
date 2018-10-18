using System.Threading;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class Program
    {
        private static async Task<int> GetFooAsync()
        {
            await Task.Delay(1000);

            return 42;
        }

        private static Task<string> GetNameAsync()
        {
            return Task.Run(() => "George");
        }
        void nop(int i) { }
        public void ResultExamples()
        {
            var x = GetFooAsync().Result; // Noncompliant {{Replace this use of 'Task.Result' with 'await'.}}
//                  ^^^^^^^^^^^^^^^^^^^^
            GetFooAsync().GetAwaiter().GetResult(); // Noncompliant {{Replace this use of 'Task.GetAwaiter.GetResult' with 'await'.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Task.Delay(0).GetAwaiter().GetResult(); // Noncompliant

            // Compliant - the following constructions don't cause deadlock
            nop(Task.Run(GetFooAsync).Result);
            Task.Run(GetFooAsync).GetAwaiter().GetResult();
            Task.Factory.StartNew(GetFooAsync).GetAwaiter().GetResult();
            Task.Factory.StartNew(GetFooAsync).Unwrap().GetAwaiter().GetResult();
            nop((((Task.Factory.StartNew(GetFooAsync).Unwrap()))).Result);

            // FP - the following cases should be valid
            var y = GetNameAsync().Result; // Noncompliant
            GetNameAsync().GetAwaiter().GetResult(); // Noncompliant
        }

        public void WaitExamples()
        {
            GetFooAsync().Wait(); // Noncompliant {{Replace this use of 'Task.Wait' with 'await'.}}
//          ^^^^^^^^^^^^^^^^^^

            // Compliant - the following constructions don't cause deadlock
            Task.Run(GetFooAsync).Wait();
            Task.Factory.StartNew(GetFooAsync).Wait();

            // FP - the following cases should be valid
            GetNameAsync().Wait(); // Noncompliant
        }

        private void WaitAnyExamples()
        {
            Task.WaitAny(GetFooAsync()); // Noncompliant {{Replace this use of 'Task.WaitAny' with 'await Task.WhenAny'.}}
//          ^^^^^^^^^^^^
        }

        private void WaitAllExamples()
        {
            Task.WaitAll(GetFooAsync()); // Noncompliant {{Replace this use of 'Task.WaitAll' with 'await Task.WhenAll'.}}
//          ^^^^^^^^^^^^
        }

        private async Task SleepAsync()
        {
            Thread.Sleep(10); // Noncompliant {{Replace this use of 'Thread.Sleep' with 'await Task.Delay'.}}
//          ^^^^^^^^^^^^
        }

        private void SleepVoid()
        {
            Thread.Sleep(10); // Compliant - method call is not async
        }

        private Task SleepTask()
        {
            Thread.Sleep(10); // Compliant - method call is not async

            return Task.CompletedTask;
        }

        public static void Main(string[] args) // All of the calls from Main methods are allowed
        {
            var x = GetFooAsync().Result;
            GetFooAsync().Wait();
            GetFooAsync().GetAwaiter().GetResult();
            Task.Delay(0).GetAwaiter().GetResult();
            Task.WaitAny(GetFooAsync());
            Task.WaitAll(GetFooAsync());
            Thread.Sleep(10);
        }
    }
}
