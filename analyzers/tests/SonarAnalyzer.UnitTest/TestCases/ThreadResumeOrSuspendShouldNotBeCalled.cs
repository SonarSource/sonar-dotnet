using System.Threading;

namespace Tests.Diagnostics
{
    class Program
    {
        void Foo()
        {
            Thread.CurrentThread.Suspend(); // Noncompliant {{Refactor the code to remove this use of 'Thread.Suspend'.}}
//                               ^^^^^^^
            Thread.CurrentThread.Resume(); // Noncompliant{{Refactor the code to remove this use of 'Thread.Resume'.}}
//                               ^^^^^^

            var thread = Thread.CurrentThread;
            thread.Suspend(); // Noncompliant
            thread?.Suspend(); // Noncompliant
        }
    }
}
