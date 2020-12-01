using System;
using System.Threading;
using System.Threading.Tasks;

// All of the calls from Main methods are allowed
var x = GetFooAsync().Result; // Noncompliant FP
GetFooAsync().Wait(); // Noncompliant FP
GetFooAsync().GetAwaiter().GetResult(); // Noncompliant FP
Task.Delay(0).GetAwaiter().GetResult(); // Noncompliant FP
Task.WaitAny(GetFooAsync()); // Noncompliant FP
Task.WaitAll(GetFooAsync()); // Noncompliant FP
Thread.Sleep(10); // Noncompliant FP

Task<int> anotherTask = null;
var b = anotherTask.Result; // Noncompliant FP

Task Foo(Task<string> task)
{
    Action<Task<string>, object> continuation;
    return task.ContinueWith(state: null, continuationAction: (Task<string> _, object _) =>
    {
        Task<int> anotherTask = null; // Pretend to compute something
        var b = anotherTask.Result; // Noncompliant, this task is not safe inside ContinueWith
    });
}

static async Task<int> GetFooAsync()
{
    await Task.Delay(1000);

    return 42;
}
