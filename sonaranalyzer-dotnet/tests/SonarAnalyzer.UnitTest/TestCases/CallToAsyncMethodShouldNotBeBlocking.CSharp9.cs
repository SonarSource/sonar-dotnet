using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class Program
    {
        public Task Foo(Task<string> task)
        {
            Action<Task<string>, object> continuation;
            return task.ContinueWith(state: null, continuationAction: (Task<string> _, object _) =>
            {
                Task<int> anotherTask = null; // Pretend to compute something
                var b = anotherTask.Result; // Noncompliant, this task is not safe inside ContinueWith
            });
        }
    }
}
