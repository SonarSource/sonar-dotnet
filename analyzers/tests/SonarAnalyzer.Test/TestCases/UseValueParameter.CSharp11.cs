using System;

namespace Tests.Diagnostics
{
    interface IFoo
    {
        static abstract int Foo1 { get; set; }

        static abstract event EventHandler Bar4;
    }

    public class Foo : IFoo
    {
        public static int Foo1
        {
            get { return 42; }
            set { } // Compliant because interface implementation
        }

        public static float Bar2
        {
            get { return 42; }
            set { } // Noncompliant
        }

        public static float[] Bar3
        {
            get { return null; }
            set
            {
                if (value is [1, .., 42])
                {

                }
            }
        }

        public static event EventHandler Bar4
        {
            add { } // Compliant because interface implementation
            remove { } // Compliant because interface implementation
        }
    }
}
