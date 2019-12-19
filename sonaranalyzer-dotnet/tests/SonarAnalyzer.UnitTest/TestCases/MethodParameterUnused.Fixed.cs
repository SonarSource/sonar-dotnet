using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    internal interface MyPrivateInterface
    {
        void MyPrivateMethod(int a);
    }

    public class BasicTests : MyPrivateInterface
    {
        private BasicTests(int a) : this(a, 42) // Compliant
        { }

        private BasicTests(
            int a
            ) // Fixed
        {
            Console.WriteLine(a);

            Action<string> x = WriteLine;
            Action<string> y = WriteLine<int>;
        }

        private void BasicTest1(int a) { } // Compliant
        void BasicTest2(int a) { } // Compliant
        private void BasicTest3(int a) { } // Compliant
        public void Caller()
        {
            BasicTest3(42); // Doesn't make it compliant
        }

        void MyPrivateInterface.MyPrivateMethod(int a) // Compliant
        {
        }

        public int MyMethod(int a) => a; // Compliant

        private static void WriteLine(string format) { } // Compliant
        private static void WriteLine<T>(string format) { } // Compliant

        void Foo(string a) // Compliant because only throws NotImplementedException
        {
            throw new NotImplementedException();
        }

        public void Foo(int arg1) // Compliant
        {
            // Empty on purpose
        }

        void DoSomething(int a, int b) => throw new NotImplementedException();
    }

    class MainEntryPoints1
    {
        static void Main(string[] args) // Compliant because Main is ignored + empty method
        {
        }
    }

    class MainEntryPoints2
    {
        static async Task Main(string[] args) // Compliant - new main syntax
        {
            Console.WriteLine("Test");
        }
    }

    class MainEntryPoints3
    {
        static async Task<int> Main(string[] args) // Compliant - new main syntax
        {
            Console.WriteLine("Test");
            return 1;
        }
    }

    class MainEntryPoints4
    {
        static async Task<string> Main() // Fixed
        {
            Console.WriteLine("Test");
            return "";
        }
    }

    public class Program1
    {
        static void Main(string[] args) // Compliant because Main is ignored + only a throw NotImplemented
        {
            throw new NotImplementedException();
        }
    }

    public class Program2
    {
        static void Main(string[] args) // Compliant because Main is ignored
        {
            Console.WriteLine("foo");
        }
    }

    public class FooBar
    {
        public FooBar(string a) // Compliant
        {
        }
    }

    public class AnyAttribute : Attribute { }

    public static class Extensions
    {
        private static void MyMethod(this string s,
            int i) // Compliant
        {

        }

        [Any]
        private static void MyMethod2(this string s,
            int i) // Compliant because of the attribute
        {

        }

        private static int Add(this string s, int a, int b) //Unused extension owner is ignored
        {
            return a + b;
        }

        private static int AddedLength(this string s, int a ) //Fixed
        {
            return s.Length + a;
        }

    }

    abstract class BaseAbstract
    {
        public abstract void M3(int a); //okay
    }
    class Base
    {
        public virtual void M3(int a) //okay
        {
        }
    }
    interface IMy
    {
        void M4(int a);
    }

    class MethodParameterUnused : Base, IMy
    {
        private void M1(int a) // Compliant
        {
        }

        void M1Bis(
            int a,
                        int c
            ) // Fixed
        {
            var result = a + c;
        }

        private void M1okay(int a)
        {
            Console.Write(a);
        }

        public virtual void M2(int a)
        {
        }

        public override void M3(int a) //okay
        {
        }

        public void M4(int a) //okay
        { }

        private void MyEventHandlerMethod(object sender, EventArgs e) //okay, event handler
        { }
        private void MyEventHandlerMethod(object sender, MyEventArgs e) //okay, event handler
        { }

        class MyEventArgs : EventArgs { }
    }

    class MethodAsEvent
    {
        delegate void CustomDelegate(string arg1, int arg2);
        event CustomDelegate SomeEventAdd;
        event CustomDelegate SomeEventSub;

        public MethodAsEvent()
        {
            SomeEventAdd += MyMethodAdd;
            SomeEventSub -= MyMethodSub;
        }

        private void MyMethodAdd(string arg1, int arg2) // Compliant
        {
        }

        private void MyMethodSub(string arg1, int arg2) // Compliant
        {
        }
    }

    class MethodAssignedToActionFromInitializer
    {
        private static void MyMethod1(int arg) { } // Compliant, because of the below assignment

        public System.Action<int> MyReference = MyMethod1;
    }

    class MethodAssignedToActionFromInitializerQualified
    {
        private static void MyMethod2(int arg) { } // Compliant, because of the below assignment

        public System.Action<int> MyReference = MethodAssignedToActionFromInitializerQualified.MyMethod2;
    }

    class MethodAssignedToFromVariable
    {
        private static void MyMethod3(int arg) { } // Compliant, because of the below assignment

        public void Foo()
        {
            System.Action<int> MyReference;
            MyReference = MyMethod3;
        }
    }

    class MethodAssignedToFromVariableQualified
    {
        private static void MyMethod4(int arg) { } // Compliant, because of the below assignment

        public void Foo()
        {
            System.Action<int> MyReference;
            MyReference = new System.Action<int>(MethodAssignedToFromVariableQualified.MyMethod4);
        }
    }

    partial class MethodAssignedToActionFromPartialClass
    {
        private static void MyMethod5(int arg) { } // Compliant, because of the below assignment

        private static void MyNonCompliantMethod(int arg) { }
    }

    partial class MethodAssignedToActionFromPartialClass
    {
        public System.Action<int> MyReference = MethodAssignedToActionFromPartialClass.MyMethod5;
    }

    public class Dead
    {
        private int Method1(int p) => (new Func<int>(() => { p = 10; return p; }))(); // Not reporting on this

        private void Method2(int p)
        {
            var x = true;
            if (x)
            {
                p = 10;
                Console.WriteLine(p);
            }

            Console.WriteLine(p);
        }

        public void Method3_Public(int p) // Compliant
        {
            var x = true;
            if (x)
            {
                p = 10;
                Console.WriteLine(p);
            }
        }

        private void Method3(int p) // Fixed
        {
            var x = true;
            if (x)
            {
                p = 10;
                Console.WriteLine(p);
            }

            Action<int> a = new Action<int>(Method4);
        }

        private void Method4(int p) // Fixed
        {
            var x = true;
            if (x)
            {
                p = 10;
                Console.WriteLine(p);
            }
            else
            {
                p = 11;
            }
        }

        private void Method5_Out(out int p)
        {
            var x = true;
            if (x)
            {
                p = 10;
                Console.WriteLine(p);
            }
            else
            {
                p = 11;
            }
        }

        private int Method6_LocalFunctions(int usedInLocalFunction)   // Compliant
        {
            int LocalFunction(int seed) => usedInLocalFunction + seed;
            int BadIncA() => usedInLocalFunction + 1;   // Fixed
            int BadIncB(int seed)             // Fixed
            {
                seed = 1;
                return usedInLocalFunction + seed;
            }

            return LocalFunction(42) + BadIncA(42) + BadIncB(42);
        }
    }

    public class Intermediate : ISerializable // Error [CS0535]
    { }

    [Serializable]
    public class ProperImplementedSerializableClass : Intermediate
    {
        private string value;

        private ProperImplementedSerializableClass(SerializationInfo info, StreamingContext context) // Compliant, because using the streaming context is not required for properly implementing the serializable constructor.
        {
            value = info.GetString("Value");
        }
    }

    [Serializable]
    public class NotProperImplementedSerializableClass : Intermediate
    {
        private StreamingContextStates state;

        private NotProperImplementedSerializableClass(StreamingContext context) // Fixed
        {
            state = context.State;
        }
    }

    [Serializable]
    public class NotProperImplementedSerializableClass2
    {
        private string value;

        private NotProperImplementedSerializableClass2(SerializationInfo info ) // Fixed
        {
            value = info.GetString("Value");
        }
    }

    public class EffectiveAccessibility
    {
        private class Inner
        {
            public void Method(int a
                ) // Fixed
            {
                Console.WriteLine(a);
            }
        }
    }

    public class ReproGithubIssue2010
    {
        static int PatternMatch(StringSplitOptions splitOptions, int i)
        {
            switch (splitOptions)
            {
                case StringSplitOptions.None
                    when i > 0:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}
