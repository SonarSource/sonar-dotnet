using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class Foo : EventArgs { }

    public class EventHandlerCases
    {
        async void MyMethod()   // Noncompliant {{Return 'Task' instead.}}
//            ^^^^
        {
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

        static async void OnValueChanged()  // Compliant, has OnXxx name
        {
        }

        async void OnX() { }

        async void O() { }          // Noncompliant
        async void On() { }         // Noncompliant
        async void Onboard() { }    // Noncompliant
        async void ToX() { }        // Noncompliant
        async void ONCAPS() { }     // Noncompliant
        async void On3People() { }  // Noncompliant, 3People is not a valid event name
        async void On_Underscore() { }  // Noncompliant
        async void onEvent() { }    // Noncompliant

        async void Onřád() { }      // Noncompliant
        async void OnŘád() { }

        async void OnΘ() { }        // Compliant, Uppercase Theta
        async void Onθ() { }        // Noncompliant, Lowercase Theta


        static async void OnEventNameChanged(BindableObject bindable, object oldValue, object newValue) // Compliant, Xamarin style, has OnXxx name
        {
            // Property changed implementation goes here
        }

        private async void ArbreDesClefs_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args) { }    // Compliant, doesn't have sender as object and doesn't inherit from EventArgs, but looks like it by argument names


        // Substitute for reference to Xamarin.Forms, Windows.UI.Xaml.Controls
        public class BindableObject { }
        public class TreeView { }
        public class TreeViewItemInvokedEventArgs { }   // Type doesn't inherit from event args
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
