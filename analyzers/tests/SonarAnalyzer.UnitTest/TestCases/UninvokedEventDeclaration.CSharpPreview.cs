using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class UninvokedEventDeclaration<T>
    {
        private interface IMyInterface<T1>
        {
            static abstract event EventHandler<T1> Event8;
        }

        private class Nested : IMyInterface<T>
        {
            private event EventHandler<T> Event5, // Noncompliant {{Remove the unused event 'Event5' or invoke it.}}
//                                        ^^^^^^
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
}
