using System;

namespace Tests.Diagnostics
{
    class Foo
    {
        public EventHandler Bar { get; }
    }

    class Program
    {
        public event EventHandler SomeEvent;
        public static event EventHandler SomeStaticEvent;

        protected void OnFoo(EventArgs e)
        {
            SomeEvent?.Invoke(this, e);
            SomeEvent?.Invoke(null, e); // Noncompliant {{Make the sender on this event invocation not null.}}
//                    ^^^^^^^^^^^^^^^^
            SomeEvent?.Invoke(this, null); // Noncompliant {{Use 'EventArgs.Empty' instead of null as the event args of this event invocation.}}
            SomeEvent?.Invoke(null, null); // Noncompliant
                                           // Noncompliant@-1

            SomeEvent(null, e); // Noncompliant {{Make the sender on this event invocation not null.}}
//          ^^^^^^^^^^^^^^^^^^
            SomeEvent.Invoke(this, e);

            SomeStaticEvent?.Invoke(null, e);
            SomeStaticEvent?.Invoke(this, e); // Noncompliant {{Make the sender on this static event invocation null.}}
            SomeStaticEvent?.Invoke(null, null); // Noncompliant {{Use 'EventArgs.Empty' instead of null as the event args of this event invocation.}}
            SomeStaticEvent?.Invoke(this, null); // Noncompliant
                                                 // Noncompliant@-1

            SomeStaticEvent(this, e); // Noncompliant {{Make the sender on this static event invocation null.}}


            SomeEvent?.Invoke(default(object), e); // Compliant - we don't handle default(T)
            SomeEvent?.Invoke(this, default(EventArgs)); // Compliant - we don't handle default(T)
        }

        public event EventHandler<ResolveEventArgs> SomeOtherEvent;

        protected void OnBar(ResolveEventArgs e)
        {
            SomeOtherEvent?.Invoke(null, e); // Noncompliant
        }

        protected void OnFooBar(EventArgs e)
        {
            SomeEvent(); // Error [CS7036] - Invalid syntax
            SomeEvent(null, null, null); // Error [CS1593] - Invalid syntax
        }

        protected void OnEvent()
        {
            Foo foo = new Foo();

            foo?.Bar?.Invoke(this, EventArgs.Empty); // Should not cause a StackOverflow
        }
    }
}
