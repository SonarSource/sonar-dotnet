using System;
using System.Collections.Generic;

public record EventInvocations
{
    public event EventHandler MyEvent1;
    public event EventHandler MyEvent2;
    public event EventHandler MyEvent3;
    public event EventHandler MyEvent4;
    public event EventHandler MyEvent5;
    public event EventHandler MyEvent6;
    public event EventHandler MyEvent7; // Noncompliant

    public event Action<object, EventArgs> MyAction1;
    public event Action<object, EventArgs> MyAction2;
    public event Action<object, EventArgs> MyAction3;
    public event Action<object, EventArgs> MyAction4;
    public event Action<object, EventArgs> MyAction5;
    public event Action<object, EventArgs> MyAction6;
    public event Action<object, EventArgs> MyAction7;  // Noncompliant

    public void InvokeAll()
    {
        MyEvent1(this, EventArgs.Empty);
        MyEvent2.Invoke(this, EventArgs.Empty);
        MyEvent3.DynamicInvoke(this, EventArgs.Empty);
        MyEvent4.BeginInvoke(this, EventArgs.Empty, null, null);
        this.MyEvent5(this, EventArgs.Empty);
        MyEvent6?.Invoke(this, EventArgs.Empty);

        MyAction1(this, EventArgs.Empty);
        MyAction2.Invoke(this, EventArgs.Empty);
        MyAction3.DynamicInvoke(this, EventArgs.Empty);
        MyAction4.BeginInvoke(this, EventArgs.Empty, null, null);
        this.MyAction5(this, EventArgs.Empty);
        MyAction6?.Invoke(this, EventArgs.Empty);
    }
}

public record EventInvocationsPositionalDeclaration(string Value)
{
    public event EventHandler MyEvent1;
    public event EventHandler MyEvent2; // Noncompliant

    public event Action<object, EventArgs> MyAction1;
    public event Action<object, EventArgs> MyAction2; // Noncompliant

    public void InvokeAll()
    {
        MyEvent1(this, EventArgs.Empty);
        MyAction1(this, EventArgs.Empty);
    }
}

class UninvokedEventDeclaration<T>
{
    private interface IMyInterface<T1>
    {
        static abstract event EventHandler<T1> Event8;
    }

    private class Nested : IMyInterface<T>
    {
        private event EventHandler<T> Event5, // Noncompliant {{Remove the unused event 'Event5' or invoke it.}}
//                                    ^^^^^^
            Event6; // Noncompliant

        public static event EventHandler<T> Event7; // Noncompliant
        public static event EventHandler<T> Event8;

        private UninvokedEventDeclaration<int> f;

        public void RegisterEventHandler(Action<object, EventArgs> handler)
        {
            Event7 += (o, a) => { };
            Event8 += (o, a) => { };
        }

        public void RaiseEvent()
        {
            if (Event5 != null)
            {
            }
        }
    }
}

public partial class PartialEvents
{
    private EventHandler compliant;
    private EventHandler nonCompliant;
    public partial event EventHandler Compliant;    // Compliant https://sonarsource.atlassian.net/browse/NET-2821
    public partial event EventHandler NonCompliant; // FN, this rule doesn't inspect events with add/remove accessors (yet)

    public virtual void RaiseEvent()
    {
        compliant.Invoke(this, EventArgs.Empty);
    }
}
