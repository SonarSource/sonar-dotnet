using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class UninvokedEventDeclaration<T>
    {
        public event Action<object, EventArgs> Event1 //Noncompliant {{Remove this unused event or invoke it.}}
        {
            add { }
            remove { }
        }
        private event Action<object, EventArgs> Event2;
        private event EventHandler<T> Event3,
            Event4; // Compliant, S1144 reports on it

        public void RegisterEventHandler(Action<object, EventArgs> handler)
        {
            Event1 += handler; //we register some event handlers
            var x = new Nested();
            x.RegisterEventHandler(null);
            x.RaiseEvent();
        }

        private abstract class MyBase<T1>
        {
            public abstract event EventHandler<T1> Event7;
        }

        private interface IMyInterface<T1>
        {
            event EventHandler<T1> Event8;
        }

        private class Nested : MyBase<T>, IMyInterface<T>
        {
            private event EventHandler<T> Event5, // Noncompliant
//                                        ^^^^^^
                Event6; // Compliant, S1144 reports on it

            public override event EventHandler<T> Event7;
            public event EventHandler<T> Event8;

            private UninvokedEventDeclaration<int> f;

            public void RegisterEventHandler(Action<object, EventArgs> handler)
            {
                f.Event1 += handler; //we register some event handlers
                Event7 += (o, a) => { };
                Event8 += (o, a) => { };
            }

            public void RaiseEvent()
            {
                if (Event5 != null)
                {
                    f.Event2(this, EventArgs.Empty);
                    f.Event3(this, 0);
                }
            }
        }
    }

    class MyClass
    {
        public event EventHandler MyEvent1;
        public event Action<int> MyEvent2;

        class Nested
        {
            public static event EventHandler MyEvent;
            public static event EventHandler MyEvent3 = MyEvent;
            public static event EventHandler MyEvent4;

            public void M()
            {
                var v = new MyClass();

                object x;
                x = MyClass.Nested.MyEvent3;

                Console.WriteLine(Nested.MyEvent4);

                if (v.MyEvent1 != null)
                {
                    v.MyEvent1.DynamicInvoke(null, null);
                    v.MyEvent1.Invoke(null, null);
                }

                v.MyEvent2?.DynamicInvoke(42);
                v.MyEvent2?.Invoke(42);
            }
        }
    }

    struct StructTest
    {
        public event Action<object, EventArgs> Event1; //Noncompliant

        public void RegisterEventHandler()
        {
            Event1 += StructTest_Event1;
        }

        private void StructTest_Event1(object arg1, EventArgs arg2)
        {
            throw new NotImplementedException();
        }
    }
}
