using System;
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
            var name = nameof(Thread.Sleep);
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

        public void Run(Task<int> task)
        {
            Action<Task<int>> arg = (action) =>
            {
                var ret = action.Result; // Noncompliant FP, we do not track actions which are used on ContinueWith
            };

            task.ContinueWith(arg);

            var safeTask = Task.FromResult(42);
            var a = safeTask.Result; // Noncompliant FP, we don't track source of the task
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2413
        public Task<string> Run(Task<string> task)
        {
            return task.ContinueWith(completedTask =>
            {
                return completedTask.Result; // Compliant, task is already completed at this point.
            });
        }

        public Task<string> RunParenthesizedLambdaExpression(Task<string> task)
        {
            return task.ContinueWith((completedTask) =>
            {
                return completedTask.Result; // Compliant, task is already completed at this point.
            });
        }

        public Task<string> TaskResultInFunctionCall(Task<string> task)
        {
            return task.ContinueWith(completedTask =>
            {
                return string.Format("Result: {0}", completedTask.Result); // Compliant, task is already completed at this point.
            });
        }

        public Task<string> MultipleTasks(Task<string> task)
        {
            return task.ContinueWith(completedTask =>
            {
                Task<int> anotherTask = null; // Pretend to compute something
                var b = anotherTask.Result; // Noncompliant, this task is not safe inside ContinueWith

                return string.Format("Result: {0}", completedTask.Result); // Compliant, task is already completed at this point.
            });
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2794
        public override string ToString()
        {
            return nameof(Task<object>.Result); // Compliant, nameof() does not execute async code.
        }
    }
}
