using System;

namespace Tests.Diagnostics
{
    public class C1
    {
        public virtual void MyNotOverriddenMethod() { }
    }
    internal partial class Partial1Part //Fixed
    {
        void Method() { }
    }
    partial struct PartialStruct //Fixed
    {
    }
    partial interface PartialInterface //Fixed
    {
    }

    internal partial class Partial2Part
    {
    }

    internal abstract partial class Partial2Part
    {
        public virtual void MyOverriddenMethod() { }
        public virtual int Prop { get; set; }
        public abstract int this[int counter] { get; }
        protected abstract event EventHandler<EventArgs> MyEvent;
        protected abstract event EventHandler<EventArgs> MyEvent2;
    }

    internal class Override : Partial2Part
    {
        public override void MyOverriddenMethod() { }

        public override int this[int counter]
        {
            get { return 0; }
        }

        protected override event EventHandler<EventArgs> MyEvent;
        protected override event EventHandler<EventArgs> MyEvent2
        {
            add { }
            remove { }
        }

        public enum SomeEnumeration
        {

        }
    }

    sealed class SealedClass : Partial2Part
    {
        public override sealed void MyOverriddenMethod() { } // Fixed
        public override sealed int Prop { get; set; } // Fixed

        public override sealed int this[int counter] // Fixed
        {
            get { return 0; }
        }

        protected override sealed event EventHandler<EventArgs> MyEvent; // Fixed
        protected override sealed event EventHandler<EventArgs> MyEvent2 // Fixed
        {
            add { }
            remove { }
        }
    }

    abstract class AbstractClass : Partial2Part
    {
        public override sealed void MyOverriddenMethod() { }
        public override sealed int Prop { get; set; }

        public override sealed int this[int counter]
        {
            get { return 0; }
        }

        protected override sealed event EventHandler<EventArgs> MyEvent;
        protected override sealed event EventHandler<EventArgs> MyEvent2
        {
            add { }
            remove { }
        }
    }

    sealed class SealedClassWithoutRedundantKeywordOnMembers : Partial2Part
    {
        public override void MyOverriddenMethod() { }

        public override int Prop { get; set; }

        public override int this[int counter]
        {
            get { return 0; }
        }

        protected override event EventHandler<EventArgs> MyEvent;
        protected override event EventHandler<EventArgs> MyEvent2
        {
            add { }
            remove { }
        }
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
                checked // Fixed
                {
                    var z = 1 + 4;
                    var y = unchecked(1 +
                        unchecked(4)); // Fixed
                }
            }

            checked // Fixed
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
                var y = unchecked(5.5 + 4); // Fixed
            }

            checked // Fixed
            {
                var f = 5.5;
                var x = 5 + "somestring";
            }

            unchecked
            {
                var f = 5.5;
                var y = unchecked(5 + 4); // Fixed
            }

            checked
            {
                var x = (uint)10;
                var y = (int)x;
            }

            checked // Fixed
            {
                var x = 10;
                var y = (double)x;
            }

            checked
            {
                var x = 10;
                x += int.MaxValue;
            }

            checked
            {
                var x = 10;
                x = -int.MaxValue;
            }

            checked
            {
                var x = 10;
                x = -5;
            }

            checked // Fixed
            {
                var x = 10;
                x = -"1"; // Error [CS0023]
            }

            checked // Fixed
            {
                var x = 10;
                x = +5;
            }

            checked  // Fixed
            {
                var x = 10;
                var y = 5 % x;
            }

            checked
            {
                var x = (uint)null; // Error [CS0037]
                var y = (int)x;
            }

            checked
            {
                var x = (SomeClass)5; // Error [CS0246]
                var y = (int)x;
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

        public void WithUndefinedInvocation()    // Fixed
        {
            Undefined();        // Error [CS0103]: The name 'Undefined' does not exist in the current context
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

    public class UnsafeCtor
    {
        public UnsafeCtor()
        {
        }
    }
}
