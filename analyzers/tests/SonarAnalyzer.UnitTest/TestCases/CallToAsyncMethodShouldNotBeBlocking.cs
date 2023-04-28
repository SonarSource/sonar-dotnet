using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

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

        public void WaitExamples(UnrelatedType unrelated)
        {
            GetFooAsync().Wait(); // Noncompliant {{Replace this use of 'Task.Wait' with 'await'.}}
//          ^^^^^^^^^^^^^^^^^^

            // Compliant - the following constructions don't cause deadlock
            Task.Run(GetFooAsync).Wait();
            Task.Factory.StartNew(GetFooAsync).Wait();

            // FP - the following cases should be valid
            GetNameAsync().Wait(); // Noncompliant

            unrelated.Wait();   // Compliant
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
            // Action<Task<T>>
            var a = task.ContinueWith(completedTask =>
            {
                var result = completedTask.Result; // Compliant, task is already completed at this point.
            });

            // Action<Task<T>, object>
            var b = task.ContinueWith((completedTask, state) =>
            {
                var result = completedTask.Result; // Compliant, task is already completed at this point.
            }, null);

            // Func<Task<T>, object, TResult>
            return task.ContinueWith((completedTask, state) =>
            {
                return completedTask.Result; // Compliant, task is already completed at this point.
            }, null);
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

        static async Task AccessAwaited(string[] args)
        {
            var getValue1Task = GetValueTask(1);
            var getValue2Task = GetValueTask(2);
            var getValue3Task = GetValueTask(3);
            var getValue4Task = GetValueTask(4);

            await Task.WhenAll(getValue1Task).ConfigureAwait(false);
            TimeSpan ts = TimeSpan.FromMilliseconds(150);
            getValue2Task.Wait(ts);             // Noncompliant
            Task.WaitAll(getValue3Task);        // Noncompliant
            getValue4Task.RunSynchronously();   // Compliant FN

            var result1 = getValue1Task.Result;  // Compliant, task is already completed at this point.
            var result2 = getValue2Task.Result;  // Compliant, task is already completed at this point.
            var result3 = getValue3Task.Result;  // Compliant, task is already completed at this point.
            var result4 = getValue4Task.Result;  // Compliant, task is already completed at this point.
        }

        static async Task<int[]> AccessResultInLinq(Task<int>[] tasks)
        {
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result).ToArray(); // Noncompliant FP https://github.com/SonarSource/sonar-dotnet/issues/7053
        }

        static async Task<int> AccessResultInElementAccessor(Task<int>[] tasks)
        {
            await Task.WhenAll(tasks);
            return tasks[0].Result; // Noncompliant FP https://github.com/SonarSource/sonar-dotnet/issues/7053
        }

        static async Task SubsequentChecksAreNotDisabledByAwait(string[] args)
        {
            var getValue1Task = GetValueTask(1);

            await Task.WhenAll(getValue1Task).ConfigureAwait(false);
            TimeSpan ts = TimeSpan.FromMilliseconds(150);
            getValue1Task.Wait(ts);             // Noncompliant
            Task.WaitAll(getValue1Task);        // Noncompliant

            var result1 = getValue1Task.Result;  // Compliant, task is already completed at this point.
        }

        static async Task AccessAwaitedWaitAllFP(string[] args)
        {
            var getValue1Task = GetValueTask(1);
            var getValue2Task = GetValueTask(2);
            var getValue3Task = GetValueTask(3);
            var getValue4Task = GetValueTask(4);

            var tasks = new List<Task<int>>();
            tasks.Add(getValue1Task);
            tasks.Add(getValue2Task);

            Task.WaitAll(tasks.ToArray());                             // Noncompliant
            Task.WaitAll(new[] { getValue3Task, getValue4Task });      // Noncompliant

            var result1 = getValue1Task.Result;                        // Noncompliant FP, task is already completed at this point.
            var result2 = getValue2Task.Result;                        // Noncompliant FP, task is already completed at this point.
            var result3 = getValue3Task.Result;                        // Noncompliant FP, task is already completed at this point.
            var result4 = getValue4Task.Result;                        // Noncompliant FP, task is already completed at this point.
        }

        static async Task AccessNotAwaited(string[] args)
        {
            var getValue1Task = GetValueTask(1);
            var getValue2Task = GetValueTask(2);
            var getValue3Task = GetValueTask(3);
            var getValue4Task = GetValueTask(4);
            var getValue5Task = GetValueTask(5);
            var getValue6Task = GetValueTask(6);
            var getValue7Task = GetValueTask(7);
            var getValue8Task = GetValueTask(8);

            await Task.WhenAll(getValue1Task).ConfigureAwait(false);
            // getValue2Task skipped

            TimeSpan ts = TimeSpan.FromMilliseconds(150);
            getValue3Task.Wait(ts);              // Noncompliant
            // getValue4Task skipped

            Task.WaitAll(getValue5Task);         // Noncompliant
            // getValue6Task skipped

            getValue7Task.RunSynchronously();
            // getValue8Task skipped

            var result1 = getValue1Task.Result;  // Compliant, task is already completed at this point.
            var result2 = getValue2Task.Result;  // Noncompliant {{Replace this use of 'Task.Result' with 'await'.}}
//                        ^^^^^^^^^^^^^^^^^^^^
            var result3 = getValue3Task.Result;  // Compliant, task is already completed at this point.
            var result4 = getValue4Task.Result;  // Noncompliant {{Replace this use of 'Task.Result' with 'await'.}}
//                        ^^^^^^^^^^^^^^^^^^^^
            var result5 = getValue5Task.Result;  // Compliant, task is already completed at this point.
            var result6 = getValue6Task.Result;  // Noncompliant {{Replace this use of 'Task.Result' with 'await'.}}
//                        ^^^^^^^^^^^^^^^^^^^^
            var result7 = getValue7Task.Result;  // Compliant, task is already completed at this point.
            var result8 = getValue8Task.Result;  // Noncompliant {{Replace this use of 'Task.Result' with 'await'.}}
//                        ^^^^^^^^^^^^^^^^^^^^
        }

        static async Task NotAnAwait(string[] args)
        {
            var getValue1Task = GetValueTask(1);
            var getValue2Task = GetValueTask(2);
            await TaskLike.WhenAll(getValue1Task, getValue2Task);
            await TaskLike.When(getValue1Task, getValue2Task);
            var result1 = getValue1Task.Result;  // Noncompliant
//                        ^^^^^^^^^^^^^^^^^^^^
            var result2 = getValue2Task.Result;  // Noncompliant
//                        ^^^^^^^^^^^^^^^^^^^^
        }

        static async Task NoAwaitAtAll(string[] args)
        {
            var getValue1Task = GetValueTask(1);
            var getValue2Task = GetValueTask(2);
            Console.Write("");
            var result1 = getValue1Task.Result;  // Noncompliant
//                        ^^^^^^^^^^^^^^^^^^^^
            var result2 = getValue2Task.Result;  // Noncompliant
//                        ^^^^^^^^^^^^^^^^^^^^
        }

        static async Task BranchingWhenAll(string[] args, int intValue)
        {
            var getValue1Task = GetValueTask(1);
            var getValue2Task = GetValueTask(2);

            if (intValue == 41)
            {
                await TaskLike.WhenAll(getValue1Task, getValue2Task);
            }
            else
            {
                var result1 = getValue1Task.Result;  // Noncompliant
//                            ^^^^^^^^^^^^^^^^^^^^
                var result2 = getValue2Task.Result;  // Noncompliant
//                            ^^^^^^^^^^^^^^^^^^^^
            }
        }

        static async Task TaskFromResultFP(string[] args)
        {
            var getValue1Task = Task.FromResult(1);
            var getValue2Task = Task.FromResult(2);
            var result1 = getValue1Task.Result;  // Noncompliant FP, since Task.FromResult results completed task
//                        ^^^^^^^^^^^^^^^^^^^^
            var result2 = getValue2Task.Result;  // Noncompliant FP, since Task.FromResult results completed task
//                        ^^^^^^^^^^^^^^^^^^^^

            await Task.WhenAll(getValue1Task, getValue2Task).ConfigureAwait(false);
        }

        public static Task<int> GetValueTask(int num)
        {
            return Task.Run(() => num);
        }

        public void TaskIsNull(Task<long>[] arr)
        {
            Task.WaitAll(arr[0]);   // Noncompliant
            var x = arr[0].Result;  // Noncompliant FP
        }

        public class TaskLike
        {
            public static Task WhenAll(params Task[] tasks) { return null; }
            public static Task When(params Task[] tasks) { return null; }
        }

        [FunctionName("Sample")]
        public static void S6422_AzureFunction()
        {
            var x = GetFooAsync().Result; // Noncompliant {{Replace this use of 'Task.Result' with 'await'. Do not perform blocking operations in Azure Functions.}}
        }
    }

    public class UnrelatedType
    {
        public void Wait() { }
    }
}
