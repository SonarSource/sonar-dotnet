using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Net6Poc
{
    internal class MsTestCases
    {
        // MSTest V1 doesn't have proper support for async so people are forced to use async void
        public void M()
        {
            [TestMethod] async void Get1() => await Task.FromResult(1);
            [TestMethod] async Task Get1s() => await Task.FromResult(1);
            async void Get2() => await Task.FromResult(2); // Compliant - FN
            async Task Get2s() => await Task.FromResult(2);

            Action a =[TestMethod] async () => { };
            Action b = async () => { };  // Compliant - FN
            Action c = [TestMethod] async () => { };  // Compliant - FN
            Func<Task> d =[TestMethod] async () => await Task.Delay(0);
            Func<Task> e = async () => await Task.Delay(0);
        }
    }

    public class Foo : EventArgs { }

    public record struct EventHandlerCasesInRecordStruct
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

        public void SomeMethod()
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

        async void MyMethod2(object o, Foo e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        private async void NotAHandler(object sender) // Noncompliant
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }
}
