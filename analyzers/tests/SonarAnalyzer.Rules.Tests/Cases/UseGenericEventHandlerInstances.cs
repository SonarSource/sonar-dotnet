using System;
using System.Windows.Input;

namespace Tests.Diagnostics
{
    public delegate void DelegateEventHandler(object sender, EventArgs e);
    public delegate void DelegateEventHandler2(object sender, int e);

    class Program
    {
        public event DelegateEventHandler DelegateEvent; // Noncompliant {{Refactor this delegate to use 'System.EventHandler<TEventArgs>'.}}
//                   ^^^^^^^^^^^^^^^^^^^^
        public event DelegateEventHandler2 DelegateEvent2; // Noncompliant

        public event DelegateEventHandler DelegateEvent3 // Noncompliant
        {
            add { }
            remove { }
        }

        public event EventHandler<EventArgs> CorrectEvent;
        public event EventHandler OtherCorrectEvent;
    }

    class SomeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }

    interface IFoo
    {
        event DelegateEventHandler FooFieldEvent; // Noncompliant
    }

    public class Foo : IFoo
    {
        public event DelegateEventHandler FooFieldEvent;

        public virtual event DelegateEventHandler FooEvent // Noncompliant
        { add { } remove { } }
    }

    public class SubFoo : Foo
    {
        public override event DelegateEventHandler FooEvent;
    }
}