using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class MyAttribute : Attribute { }

    class UnusedPrivateMember
    {
        public static void Main() { }

        private class MyOtherClass
        { }

        private class MyClass
        {
            internal MyClass(int i)
            {
                var x = (MyOtherClass)null;
                x = x as MyOtherClass;
                Console.WriteLine();
            }
        }

        private class Gen<T> : MyClass
        {
            public Gen() : base(1)
            {
                Console.WriteLine();
            }
        }

        public UnusedPrivateMember()
        {
            MyProperty = 5;
            MyEvent += UnusedPrivateMember_MyEvent;
            MyUsedEvent += UnusedPrivateMember_MyUsedEvent;
            new Gen<int>();
        }

        private void UnusedPrivateMember_MyUsedEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UnusedPrivateMember_MyEvent()
        {
            field3 = 5;
            throw new NotImplementedException();
        }
        private
            int field3; // Fixed
        private delegate void Delegate();
        private event Delegate MyEvent; //Fixed

        private event EventHandler<EventArgs> MyUsedEvent
        {
            add { }
            remove { }
        }

        private int MyProperty
        {
            get;
            set;
        }

        [My]
        private class Class1 { }
    }

    class NewClass1
    {
        // See https://github.com/SonarSource/sonar-csharp/issues/888
        static async Task Main() // Compliant - valid main method since C# 7.1
        {
            Console.WriteLine("Test");
        }
    }

    class NewClass2
    {
        static async Task<int> Main() // Compliant - valid main method since C# 7.1
        {
            Console.WriteLine("Test");

            return 1;
        }
    }

    class NewClass3
    {
        static async Task Main(string[] args) // Compliant - valid main method since C# 7.1
        {
            Console.WriteLine("Test");
        }
    }

    class NewClass4
    {
        static async Task<int> Main(string[] args) // Compliant - valid main method since C# 7.1
        {
            Console.WriteLine("Test");

            return 1;
        }
    }

    class NewClass5
    {
    }

    public static class MyExtension
    {
        private static void MyMethod<T>(this T self) { "".MyMethod<string>(); }
    }

    public class NonExactMatch
    {
        private static void M(int i) { }    // Compliant, might be called
        private static void M(string i) { } // Compliant, might be called

        public static void Call(dynamic d)
        {
            M(d);
        }
    }

    public class EventHandlerSample
    {
    }

    public partial class EventHandlerSample1
    {
        private void MyOnClick(object sender, EventArgs args) { } // Compliant, event handlers in partial classes are not reported
    }

    public class PropertyAccess
    {
        private int OnlyRead { get; }  // Fixed
        private int OnlySet { get; set; }
        private int OnlySet2 { set { } } // Fixed
        private int BothAccessed { get; set; }

        private int OnlyGet { get { return 42; } }

        public void M()
        {
            Console.WriteLine(OnlyRead);
            OnlySet = 42;
            (this.OnlySet2) = 42;

            BothAccessed++;

            int? x = 10;
            x = this?.OnlyGet;
        }
    }

    [Serializable]
    public sealed class GoodException : Exception
    {
        public GoodException()
        {
        }
        public GoodException(string message)
            : base(message)
        {
        }
        public GoodException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        private GoodException(SerializationInfo info, StreamingContext context) // Compliant because of the serialization
            : base(info, context)
        {
        }
    }

    public class FieldAccess
    {
        private object field1;
        private object field2; // Fixed
        private object field3;

        public FieldAccess()
        {
            this.field2 = field3 ?? this.field1?.ToString();
        }
    }

    // As S4487 will raise when a private field is written and not read, S1450 won't raise on these cases
    // These tests where finding issues before with S1450 and should find them with S4487 now
    public class TestsFormerS1450
    {
        private int F1 = 0; // Fixed

        public void M1()
        {
            ((F1)) = 42;
        }

        private int F5 = 0; // Fixed
        private int F6; // Fixed
        public void M2()
        {
            F5 = 42;
            F6 = 42;
        }

        private int F14 = 0; // Fixed
        public void M6(int F14)
        {
            this.F14 = 42;
        }
        private int F28 = 42; // Fixed
        public event EventHandler E1
        {
            add
            {
                F28 = 42;
            }
            remove
            {
            }
        }

        private int F36; // Fixed
        public void M15(int i) => F36 = i + 1;
    }
}
