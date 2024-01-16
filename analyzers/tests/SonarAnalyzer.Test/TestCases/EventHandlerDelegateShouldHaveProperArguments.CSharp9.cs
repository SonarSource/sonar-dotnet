using System;

namespace Tests.Diagnostics
{
    partial record R
    {
        public event EventHandler SomeEvent;
        public static event EventHandler SomeStaticEvent;

        protected partial void OnFoo(EventArgs e);
    }

    partial record R
    {
        protected partial void OnFoo(EventArgs e)
        {
            SomeEvent?.Invoke(this, e);
            SomeEvent?.Invoke(null, e); // Noncompliant {{Make the sender on this event invocation not null.}}
//                    ^^^^^^^^^^^^^^^^
            SomeEvent?.Invoke(this, null); // Noncompliant {{Use 'EventArgs.Empty' instead of null as the event args of this event invocation.}}
            SomeEvent?.Invoke(null, null); // Noncompliant
                                           // Noncompliant@-1

            SomeStaticEvent?.Invoke(null, e);
            SomeStaticEvent?.Invoke(this, e); // Noncompliant {{Make the sender on this static event invocation null.}}
            SomeStaticEvent?.Invoke(null, null); // Noncompliant {{Use 'EventArgs.Empty' instead of null as the event args of this event invocation.}}
            SomeStaticEvent?.Invoke(this, null); // Noncompliant
                                                 // Noncompliant@-1

            SomeEvent(null, e); // Noncompliant
            SomeStaticEvent(null, null); // Noncompliant
        }
    }
}
