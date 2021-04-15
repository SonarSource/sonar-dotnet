namespace Tests.Diagnostics
{
    public class C1
    {
        public virtual void MyNotOverriddenMethod() { }
    }
    internal partial class Partial1Part //Noncompliant
//           ^^^^^^^
    {
        void Method() { }
    }
    partial struct PartialStruct //Noncompliant {{'partial' is gratuitous in this context.}}
    {
    }
partial interface PartialInterface //Noncompliant
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
        public override sealed void MyOverriddenMethod() { } //Noncompliant {{'sealed' is redundant in this context.}}
//                      ^^^^^^
        public override sealed int Prop { get; set; } //Noncompliant
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

    unsafe class UnsafeClass2 // Noncompliant
//  ^^^^^^
    {
        int num;
    }
    unsafe class UnsafeClass3 // Noncompliant {{'unsafe' is redundant in this context.}}
    {
        unsafe void M() // Noncompliant
//      ^^^^^^
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
            unsafe int* Method(); // Noncompliant
        }

        private unsafe delegate void MyDelegate(int* p);
        private unsafe delegate void MyDelegate2(int i); // Noncompliant

        unsafe class Inner { } // Noncompliant

        unsafe event MyDelegate MyEvent; // Noncompliant
        unsafe event MyDelegate MyEvent2
        {
            add
            {
                int* p;
            }
            remove { }
        }

        unsafe ~Class4() // Noncompliant
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
                unsafe // Noncompliant
                {
                    unsafe // Noncompliant
                    {
                        var i = 1;
                        int* p = &i;
                    }
                }
            }
        }
    }

    public class Foo
    {
        public class Bar
        {
            public unsafe class Baz // Noncompliant
            {
            }
        }
    }

    public unsafe class Foo2
    {
        public unsafe class Bar // Noncompliant
        {
            private int* p;

            public unsafe class Baz // Noncompliant
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
                checked // Noncompliant
//              ^^^^^^^
                {
                    var z = 1 + 4;
                    var y = unchecked(1 +
                        unchecked(4)); // Noncompliant
//                      ^^^^^^^^^
                }
            }

            checked // Noncompliant {{'checked' is redundant in this context.}}
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
                var y = unchecked(5.5 + 4); // Noncompliant
            }

            unchecked
            {
                var f = 5.5;
                var y = unchecked(5 + 4); // Noncompliant
            }

            checked
            {
                var x = (uint)10;
                var y = (int)x;
            }

            checked // Noncompliant
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
