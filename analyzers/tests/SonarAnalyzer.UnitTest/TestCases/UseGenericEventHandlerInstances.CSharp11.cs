using System;
using System.Windows.Input;

namespace Tests.Diagnostics
{
    public delegate void DelegateEventHandler(object sender, EventArgs e);

    interface IFoo
    {
        static abstract event DelegateEventHandler FooFieldEvent; // Noncompliant
    }

    public class Foo : IFoo
    {
        public static event DelegateEventHandler FooFieldEvent;
    }
}
