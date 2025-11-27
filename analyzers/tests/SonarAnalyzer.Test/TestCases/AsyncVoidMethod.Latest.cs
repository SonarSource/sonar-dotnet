using System;
using System.Threading.Tasks;

    public class Sample : EventArgs { }

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

        async void MyMethod2(object o, Sample e)
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

        async void MyMethod2(object o, Sample e)
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

        async void MyMethod2(object o, Sample e)
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

public record struct EventHandlerCasesInRecordStruct
{
    async void MyMethod() // Noncompliant {{Return 'Task' instead.}}
//        ^^^^
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

    async void MyMethod2(object o, Sample e)
    {
        await Task.Run(() => Console.WriteLine("test"));
    }

    public event EventHandler<bool> MyEvent;

    public void SomeMethod()
    {
        MyEvent += EventHandlerCases_MyEvent;
    }

    private async void EventHandlerCases_MyEvent(object sender, bool e)
    {
        await Task.Run(() => Console.WriteLine("test"));
    }

    private async void NotAHandler(object sender) // Noncompliant
//                ^^^^
    {
        await Task.Run(() => Console.WriteLine("test"));
    }
}

public record struct EventHandlerCasesInPositionalRecordStruct(string Param)
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

    async void MyMethod2(object o, Sample e)
    {
        await Task.Run(() => Console.WriteLine("test"));
    }

    private async void NotAHandler(object sender) // Noncompliant
    {
        await Task.Run(() => Console.WriteLine("test"));
    }
}

public interface IVirtualMethodInterface
{
    static abstract void SomeMethod1();

    static virtual async void SomeVirtualMethod() // Compliant (virtual member)
    {
        return;
    }
}

public class SomeClass : IVirtualMethodInterface
{
    public static async void SomeMethod1() // Compliant as it comes from the interface
    {
        return;
    }

    public static async void SomeMethod2() // Noncompliant
    {
        return;
    }
}

public static class Extensions
{
    extension(Sample e)
    {
        async void NonCompliant() // Noncompliant {{Return 'Task' instead.}}
//            ^^^^
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void Compliant(object sender, EventArgs args)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void NonCompliant(object sender)    // Noncompliant
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }
}
