using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class Foo : EventArgs { }

    public class EventHandlerCases
    {
        async void MyMethod() {  // Noncompliant {{Return 'Task' instead.}}
//            ^^^^
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod(object sender, EventArgs args)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod2(object o, Foo e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        public event EventHandler<bool> MyEvent;

        public EventHandlerCases()
        {
            MyEvent += EventHandlerCases_MyEvent;
        }

        private async void EventHandlerCases_MyEvent(object sender, bool e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }

    public class UwpCases
    {
        // A lot of classes/interfaces in UWP do not inherit from EventArgs so we had to change the detection mechanism
        // See issue https://github.com/SonarSource/sonar-dotnet/issues/704
        private interface ISuspendingEventArgs { }

        async void MyOtherMethod1(object o, ISuspendingEventArgs args)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
        private async void OnSuspending(object sender, ISuspendingEventArgs e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }
}

