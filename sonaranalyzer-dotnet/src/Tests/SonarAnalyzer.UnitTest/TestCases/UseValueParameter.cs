using System;

namespace Tests.Diagnostics
{
    class UseValueParameter
    {
        private int count;
        public int Count
        {
            get { return count; }
            set { count = 3; } //Noncompliant
//          ^^^
        }
        public int Count2
        {
            get { return count; }
            set { count = value; }
        }
        public int Count3
        {
            //get { return count; }
            set //Noncompliant {{Use the 'value' parameter in this property set accessor declaration.}}
            {
                var value = 5;
                count = value;
            }
        }
        public int Count5
        {
            set
            {
                throw new Exception();
            }
        }

        public int Count4 => count;

        public int this[int i]
        {
            get
            {
                return 0;
            }
            set //Noncompliant
            {
                var x = 1;
            }
        }

        event EventHandler PreDrawEvent;

        event EventHandler IDrawingObject.OnDraw
        {
            add //Noncompliant {{Use the 'value' parameter in this event accessor declaration.}}
            {
                lock (PreDrawEvent)
                {
                }
            }
            remove
            {
                lock (PreDrawEvent)
                {
                    PreDrawEvent -= value;
                }
            }
        }
    }

    interface IFoo
    {
        int Foo { get; set; }

        event EventHandler Bar;
    }

    public class Foo : IFoo
    {
        public int Foo
        {
            get { return 42; }
            set { } // Compliant because EMPTY + interface implementation
        }

        public float Bar
        {
            get { return 42; }
            set { } // Noncompliant
        }

        public event EventHandler Bar
        {
            add { } // Compliant because EMPTY + interface implementation
            remove { } // Compliant because EMPTY + interface implementation
        }
    }
}
