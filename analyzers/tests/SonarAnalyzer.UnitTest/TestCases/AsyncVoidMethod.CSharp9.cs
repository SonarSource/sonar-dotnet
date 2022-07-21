using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class Foo : EventArgs { }

    public record EventHandlerCasesInRecord
    {
        async void MyMethod() // Noncompliant {{Return 'Task' instead.}}
//            ^^^^
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod(object sender, EventArgs args)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod1(object o, EventArgs e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod2(object o, Foo e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        public event EventHandler<bool> MyEvent;

        public EventHandlerCasesInRecord()
        {
            MyEvent += EventHandlerCases_MyEvent;
        }

        private async void EventHandlerCases_MyEvent(object sender, bool e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        private async void NotAHandler(object sender) // Noncompliant
//                    ^^^^
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }

    public record EventHandlerCasesInPositionalRecord(string Param)
    {
        async void MyMethod() // Noncompliant
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod(object sender, EventArgs args)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod1(object o, EventArgs e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod2(object o, Foo e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        private async void NotAHandler(object sender) // Noncompliant
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }

    public interface EventHandlerCasesInInterface
    {
        async void MyMethod() // Compliant because it can be implemented as a non async method
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod(object sender, EventArgs args)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod1(object o, EventArgs e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod2(object o, Foo e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        private async void NotAHandler(object sender) // Noncompliant
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }

    public interface ISomeInterface
    {
        event EventHandler<bool> MyEvent;

        public void SomeMethod()
        {
            MyEvent += EventHandlerCases_MyEvent;
        }

        private async void EventHandlerCases_MyEvent(object sender, bool e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }

    public class Reproducer5432
    {
        public void SomeMethod()
        {
            var _timer = new System.Threading.Timer(RunOnceAsync);
        }

        private async void RunOnceAsync(object? _) // Compliant, see: https://github.com/SonarSource/sonar-dotnet/issues/5432
        {
        }
    }
}
