using System;

partial record R
{
    protected partial void OnFoo(EventArgs e)
    {
        SomeEvent?.Invoke(this, e);
        SomeEvent?.Invoke(null, e); // Noncompliant {{Make the sender on this event invocation not null.}}
//                ^^^^^^^^^^^^^^^^
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

interface IMyInterface
{
    static virtual event EventHandler VirtualEvent;

    static virtual void VirtualMethod<T>(object sender, EventArgs e) where T : IMyInterface
    {
        T.VirtualEvent.Invoke(null, null);      // Noncompliant
        VirtualEvent.Invoke(null, null);        // Noncompliant
        T.VirtualEvent.Invoke(null, e);         // Compliant
        VirtualEvent.Invoke(null, e);           // Compliant
    }
}

partial class PartialEvents
{
    EventHandler handler;
    static EventHandler staticHandler;
    partial event EventHandler PartialEvent { add => handler += value; remove => handler -= value; }
    static partial event EventHandler PartialStaticEvent { add => staticHandler += value; remove => staticHandler -= value; }

    void Method(EventArgs e)
    {
        PartialEvent?.Invoke(null, e);          // Error [CS0079] The event 'PartialEvents.PartialEvent' can only appear on the left hand side of += or -=
        PartialStaticEvent?.Invoke(this, e);    // Error [CS0079] The event 'PartialEvents.PartialStaticEvent' can only appear on the left hand side of += or -=
        handler?.Invoke(null, e);               // Compliant
        handler?.Invoke(null, null);            // Noncompliant
        staticHandler?.Invoke(this, e);         // Compliant
        staticHandler?.Invoke(this, null);      // Noncompliant
    }
}
