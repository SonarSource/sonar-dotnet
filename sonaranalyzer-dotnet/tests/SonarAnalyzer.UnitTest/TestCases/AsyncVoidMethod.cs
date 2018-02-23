using System;

namespace Tests.Diagnostics
{
    public class Foo : EventArgs { }

    public class EventHandlerCases
    {
        async void MyMethod() { } // Noncompliant {{Return 'Task' instead.}}
//            ^^^^
        async void MyMethod(object sender, EventArgs args) { }

        async void MyMethod1(object o, EventArgs e) { }

        async void MyMethod2(object o, Foo e) { }

        public event EventHandler<bool> MyEvent;

        public EventHandlerCases()
        {
            MyEvent += EventHandlerCases_MyEvent;
        }

        private async void EventHandlerCases_MyEvent(object sender, bool e)
        {
        }
    }

    public class UwpCases
    {
        // A lot of classes/interfaces in UWP do not inherit from EventArgs so we had to change the detection mechanism
        // See issue https://github.com/SonarSource/sonar-csharp/issues/704
        private interface ISuspendingEventArgs { }

        async void MyOtherMethod1(object o, ISuspendingEventArgs args) { }
        private async void OnSuspending(object sender, ISuspendingEventArgs e) { }
    }
}
