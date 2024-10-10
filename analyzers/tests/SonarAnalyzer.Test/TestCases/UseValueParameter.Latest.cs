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

    record UseValueParameter
    {
        int count;

        public int Count
        {
            get { return count; }
            init { count = 3; } // Noncompliant {{Use the 'value' contextual keyword in this property init accessor declaration.}}
        }

        public int Count2
        {
            get { return count; }
            set  // Noncompliant
            {
                void Foo(int value)
                {
                    count = value;
                }
            }
        }

        public int Count3
        {
            get => 42;
            set  // Noncompliant
            {
                System.Func<int, int> f = value => value;
            }
        }

        public string FirstName { get; init; } = "Foo";

        public string LastName { get => string.Empty; init { } } // Noncompliant

        public int this[int i]
        {
            get => 0;
            init // Noncompliant
            {
                var x = 1;
            }
        }
    }

    public partial class PartialProperty
    {
        private int count;
        public partial int Property1 { get; set; }
        public partial int Property2 { get; init; }
        private partial int this[int index] { set; }
    }
}
