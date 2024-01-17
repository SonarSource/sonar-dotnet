using System;
using System.Threading;
using System.Threading.Tasks;

// All the calls from top level statements are allowed.
var x = GetFooAsync().Result;
GetFooAsync().Wait();
GetFooAsync().GetAwaiter().GetResult();
Task.Delay(0).GetAwaiter().GetResult();
Task.WaitAny(GetFooAsync());
Task.WaitAll(GetFooAsync());
Thread.Sleep(10);

Task<int> anotherTask = null;
var b = anotherTask.Result;

Task Foo(Task<string> task)
{
    Action<Task<string>, object> continuation;
    return task.ContinueWith(state: null, continuationAction: (Task<string> _, object _) =>
    {
        Task<int> anotherTask = null;
        var b = anotherTask.Result;
    });
}

static async Task<int> GetFooAsync()
{
    await Task.Delay(1000);

    return 42;
}

public class Test
{
    Task FooInClass(Task<string> task)
    {
        Action<Task<string>, object> continuation;
        return task.ContinueWith(state: null, continuationAction: (Task<string> _, object _) =>
        {
            Task<int> anotherTask = null; // Pretend to compute something
            var b = anotherTask.Result; // Noncompliant, this task is not safe inside ContinueWith
        });
    }
}
