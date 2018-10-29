namespace Tests.Diagnostics
{
    public class C1
    {
        public virtual void MyNotOverriddenMethod() { }
    }
    internal class Partial1Part //Fixed
    {
        void Method() { }
    }
    struct PartialStruct //Fixed
    {
    }
interface PartialInterface //Fixed
    {
    }

    internal partial class Partial2Part
    {
    }

    internal partial class Partial2Part
    {
        public virtual void MyOverriddenMethod() { }
        public virtual int Prop { get; set; }
    }
    internal class Override : Partial2Part
    {
        public override void MyOverriddenMethod() { }
    }
    sealed class SealedClass : Partial2Part
    {
        public override void MyOverriddenMethod() { } //Fixed
        public override int Prop { get; set; } //Fixed
    }

    internal class BaseClass<T>
    {
        public virtual string Process(string input)
        {
            return input;
        }
    }

    internal class SubClass : BaseClass<string>
    {
        public override string Process(string input)
        {
            return "Test";
        }
    }

    unsafe class UnsafeClass
    {
        int* pointer;
    }

    class UnsafeClass2 // Fixed
    {
        int num;
    }
    class UnsafeClass3 // Fixed
    {
        void M() // Fixed
        {

        }
    }

    class Point
    {
        public int x;
        public int y;
    }

    class Class4
    {
        unsafe interface MyInterface
        {
            int* Method(); // Fixed
        }

        private unsafe delegate void MyDelegate(int* p);
        private delegate void MyDelegate2(int i); // Fixed

        class Inner { } // Fixed

        event MyDelegate MyEvent; // Fixed
        unsafe event MyDelegate MyEvent2
        {
            add
            {
                int* p;
            }
            remove { }
        }

        ~Class4() // Fixed
        {
        }
        void M()
        {
            Point pt = new Point();
            unsafe
            {
                fixed (int* p = &pt.x)
                {
                    *p = 1;
                }
            }

            unsafe
            {
                var i = 1;
                int* p = &i;
            }
        }
    }

    public class Foo
    {
        public class Bar
        {
            public class Baz // Fixed
            {
            }
        }
    }

    public unsafe class Foo2
    {
        public class Bar // Fixed
        {
            private int* p;

            public class Baz // Fixed
            {
                private int* p2;
            }
        }
    }

    public class Checked
    {
        public static void M()
        {
            checked
            {
                {
                    var z = 1 + 4;
                    var y = unchecked(1 +
                        4); // Fixed
                }
            }

            {
                var f = 5.5;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = 5.5 + 4; // Fixed
            }

            unchecked
            {
                var f = 5.5;
                var y = 5 + 4; // Fixed
            }

            checked
            {
                var x = (uint)10;
                var y = (int)x;
            }

            {
                var x = 10;
                var y = (double)x;
            }

            checked
            {
                var x = 10;
                x += int.MaxValue;
            }
        }
    }

    public unsafe struct FixedArraySample
    {
        private fixed int a[10];
    }

    public class StackAlloc
    {
        private unsafe void M()
        {
            var x = stackalloc int[100];
        }
    }

    public class UnsafeHidden
    {
        private unsafe int* Method() { return null; }
        private unsafe void Method2(int* p) { }
        public unsafe void M1()
        {
            Method();
        }

        private unsafe int* Prop { get; set; }
        public unsafe void M2()
        {
            Method2(Prop);
        }
    }

    public class UnsafeParameter
    {
        public unsafe delegate void Unsafe(int* x);

        public unsafe void Method()
        {
            Unsafe u = (a) => { };
        }
    }
}
