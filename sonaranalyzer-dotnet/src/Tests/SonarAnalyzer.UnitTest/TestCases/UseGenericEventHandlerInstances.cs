using System;

namespace Tests.Diagnostics
{
    public delegate void DelegateEventHandler(object sender, EventArgs e);
    public delegate void DelegateEventHandler2(object sender, int e);

    class Program
    {
        public event DelegateEventHandler DelegateEvent; // Noncompliant {{Refactor this delegate to use 'System.EventHandler<TEventArgs>'.}}
//                   ^^^^^^^^^^^^^^^^^^^^
        public event DelegateEventHandler2 DelegateEvent2; // Noncompliant

        public event EventHandler<EventArgs> CorrectEvent;
    }
}