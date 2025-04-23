using System;

namespace Tests.Diagnostics
{
    public interface IDrawing
    {
        event EventHandler OnDraw;
    }

    class UseValueParameter : IDrawing
    {
        private int count;
        public int Count
        {
            get { return count; }
            set { count = 3; } // Noncompliant {{Use the 'value' contextual keyword in this property set accessor declaration.}}
//          ^^^
        }
        public int Count2
        {
            get { return count; }
            set { count = value; }
        }
        public int Count3
        {
            set // Noncompliant
            {
                var val = 5;
                count = val;
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
            set // Noncompliant
            {
                var x = 1;
            }
        }

        event EventHandler PreDrawEvent;

        event EventHandler IDrawing.OnDraw
        {
            add // Noncompliant {{Use the 'value' contextual keyword in this event accessor declaration.}}
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
        int Foo1 { get; set; }

        event EventHandler Bar3;
    }

    public class Foo : IFoo
    {
        public virtual int Foo1
        {
            get { return 42; }
            set { } // Compliant because interface implementation
        }

        public virtual float Bar2
        {
            get { return 42; }
            set { } // Noncompliant
        }

        public virtual event EventHandler Bar3
        {
            add { } // Compliant because interface implementation
            remove { } // Compliant because interface implementation
        }
    }

    public class Bar : Foo
    {
        public override int Foo1
        {
            get { return 42; }
            set { } // Compliant because interface implementation
        }

        public override float Bar2
        {
            get { return 42; }
            set { } // Noncompliant
        }

        public override event EventHandler Bar3
        {
            add { } // Compliant because interface implementation
            remove { } // Compliant because interface implementation
        }
    }

    // implement interface using arrow syntax
    public class Baz : IFoo
    {
        private int foo1;

        public int Foo1
        {
            get => 42;
            set => foo1 = 0; // Noncompliant
        }

        public event EventHandler Bar3
        {
            add => throw new Exception();
            remove => throw new Exception();
        }
    }

    public class NewSyntax
    {
        public int ReadonlyGetter { get; }

        public int ArrowedProperty => 1;

        private int field;
        public int ArrowedAccessors
        {
            get => field;
            set => field = value;
        }

        private string id;

        public string Id
        {
            get => null;
            set => id = ""; // Noncompliant
        }

        public int A
        {
            set => throw new Exception();
        }

        public event EventHandler E
        {
            add => throw new Exception();
            remove => throw new Exception();
        }
    }
}
