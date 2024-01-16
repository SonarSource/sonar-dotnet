using System;

namespace Tests.Diagnostics
{
    interface IMyInterface
    {
        static virtual event EventHandler VirtualEvent
        {
            add => throw new Exception(); // Noncompliant
            remove => throw new InvalidOperationException(); // Compliant - allowed exception
        }

        static abstract event EventHandler AbstractEvent;
    }

    class MyClass : IMyInterface
    {
        public static event EventHandler AbstractEvent
        {
            add => throw new Exception(); // Noncompliant
            remove => throw new InvalidOperationException(); // Compliant - allowed exception
        }
    }
}
