using System;
using System.IO;

namespace Tests.Diagnostics
{
    public interface IFoo { }
    class FooBase : IFoo { }
    class Foo1 : FooBase { }
    public struct MyStruct { }

    public abstract class TestCases // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 8 to the maximum authorized 1 or less.}}
//                        ^^^^^^^^^
    {
        // ================================================================================
        // ==== FIELDS
        // ================================================================================
        private IFoo field1 = new FooBase();
        private FooBase field2 = Method3();
        private static IFoo field3 = Property1;
        private int field4; // Primitives don't count
        private MyStruct str;
        private System.Threading.Tasks.Task myTask;
        Action myAction;
        Func<int> myFunct;
        unsafe int* myPointer;


        // ================================================================================
        // ==== PROPERTIES
        // ================================================================================
        private static Foo1 Property1 { get; }

        public IFoo Property2 { get; set; }

        public IFoo Property3
        {
            get
            {
                return new FooBase();
            }
        }

        public IFoo Property4
        {
            set
            {
                var x = value.ToString();
            }
        }

        public IFoo Property5 => Method3();



        // ================================================================================
        // ==== EVENTS
        // ================================================================================
        public event EventHandler Event1
        {
            add
            {
                var x = Method3();
            }
            remove
            {
                IFoo xx = Method3();
            }
        }



        // ================================================================================
        // ==== CTORS
        // ================================================================================
        public TestCases()
        {
            var x = new object();
            Stream y = new System.IO.FileStream("", System.IO.FileMode.Open);
        }



        // ================================================================================
        // ==== DTORS
        // ================================================================================
        ~TestCases()
        {
            Stream y;
            y = new FileStream("", FileMode.Open);
        }



        // ================================================================================
        // ==== METHODS
        // ================================================================================
        IDisposable Method1(object o)
        {
            Stream y = new FileStream("", FileMode.Open);
            return y;
        }

        Stream Method2() => new FileStream("", FileMode.Open);
        private static FooBase Method3() => null;

        protected abstract IFoo Method4();
    }

    public class OutterClass
    {
        InnerClass whatever = new InnerClass();

        public class InnerClass // Noncompliant
        {
            public Stream stream = new FileStream("", FileMode.Open);
        }
    }
}
