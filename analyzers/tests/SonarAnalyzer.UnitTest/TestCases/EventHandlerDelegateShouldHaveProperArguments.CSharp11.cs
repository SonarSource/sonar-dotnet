using System;

namespace Tests.Diagnostics
{
    interface IMyInterface
    {
        static event EventHandler SomeStaticEvent;

        static abstract event EventHandler AbstractEvent;

        static virtual event EventHandler VirtualEvent
        {
            add => SomeStaticEvent += value;
            remove => SomeStaticEvent -= value;
        }

        static virtual void VirtualMethod(object sender, EventArgs e)
        {
            SomeStaticEvent?.Invoke(null, e);
            SomeStaticEvent?.Invoke(sender, e);     // Noncompliant {{Make the sender on this static event invocation null.}}
            SomeStaticEvent?.Invoke(null, null);    // Noncompliant {{Use 'EventArgs.Empty' instead of null as the event args of this event invocation.}}
            SomeStaticEvent?.Invoke(sender, null);  // Noncompliant
                                                    // Noncompliant@-1

            SomeStaticEvent(null, null);            // Noncompliant
        }

        static abstract void AbstractMethod(EventHandler handler, EventArgs e);
    }

    class MyClass : IMyInterface
    {
        private static event EventHandler PreAbstractEvent;
        public static event EventHandler AbstractEvent
        {
            add => PreAbstractEvent += value;
            remove => PreAbstractEvent -= value;
        }

        public static void AbstractMethod(EventHandler handler, EventArgs e)
        {
            handler?.Invoke(new object(), e);
            handler?.Invoke(new object(), null);            // Noncompliant {{Use 'EventArgs.Empty' instead of null as the event args of this event invocation.}}

            PreAbstractEvent?.Invoke(new object(), e);      // Noncompliant {{Make the sender on this static event invocation null.}}
            PreAbstractEvent?.Invoke(new object(), null);   // Noncompliant {{Use 'EventArgs.Empty' instead of null as the event args of this event invocation.}}
                                                            // Noncompliant@-1
        }
    }
}
