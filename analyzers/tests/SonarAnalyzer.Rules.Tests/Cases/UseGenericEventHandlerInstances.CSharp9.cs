using System;

public delegate void DelegateEventHandler(object sender, EventArgs e);

record WithEvents
{
    public event DelegateEventHandler DelegateEvent; // Noncompliant {{Refactor this delegate to use 'System.EventHandler<TEventArgs>'.}}
//               ^^^^^^^^^^^^^^^^^^^^

    public event EventHandler CanExecuteChanged; // Compliant
}
