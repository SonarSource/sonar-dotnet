using System;

namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        public static event EventHandler SomeStaticEvent;

        public static virtual void VirtualMethod(object sender, EventArgs e)
        {
            SomeStaticEvent?.Invoke(null, e);
            SomeStaticEvent?.Invoke(sender, e); // Noncompliant {{Make the sender on this static event invocation null.}}
            SomeStaticEvent?.Invoke(null, null); // Noncompliant {{Use 'EventArgs.Empty' instead of null as the event args of this event invocation.}}
            SomeStaticEvent?.Invoke(sender, null); // Noncompliant
                                                   // Noncompliant@-1

            SomeStaticEvent(null, null); // Noncompliant
        }

        public static abstract void AbstractMethod(EventHandler handler, EventArgs e);
    }

    public class MyClass : IMyInterface
    {
        public static void AbstractMethod(EventHandler handler, EventArgs e)
        {
            handler?.Invoke(new object(), e);
            handler?.Invoke(new object(), null); // Noncompliant {{Use 'EventArgs.Empty' instead of null as the event args of this event invocation.}}
        }
    }
}
