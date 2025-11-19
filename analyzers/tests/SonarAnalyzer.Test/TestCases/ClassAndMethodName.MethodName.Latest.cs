namespace CSharp9
{
    public interface IMyInterface
    {
        void foo(); // Noncompliant {{Rename method 'foo' to match pascal case naming rules, consider using 'Foo'.}}
//           ^^^
        void Foo();
    }

    public record FooRecord : IMyInterface
    {
        void a() { } // Noncompliant
        void A() { } // Сompliant
        void foo() { } // Noncompliant
        void IMyInterface.foo() { } // Compliant, we can't change it
        void Foo() { }
        void IMyInterface.Foo() { }

        void Do_Some_Test() { }
        void Do_Some_Test_() { } // Noncompliant
        void ____() { } // Noncompliant {{Rename method '____' to match pascal case naming rules, trim underscores from the name.}}

        protected void Application_Start() { } // FN for a record

        public int MyPPPProperty { get; set; } // Noncompliant {{Rename property 'MyPPPProperty' to match pascal case naming rules, consider using 'MyPppProperty'.}}

        public void 你好() { }

        public int ArrowedProperty2
        {
            get => 41;
            init => foo();
        }
    }

    public record PositionalRecord(string Value)
    {
        void a() { } // Noncompliant
        void A() { } // Сompliant
        void foo() { } // Noncompliant
        void Foo() { } // Сompliant
    }

    public record Base
    {
        public virtual void foo() { } // Noncompliant
    }
    public record Derived : Base
    {
        public override void foo() // Compliant
        {
            base.foo();
        }
    }

    public record WithLocalFunctions
    {
        public void Method()
        {
            void foo() { } // Noncompliant {{Rename local function 'foo' to match pascal case naming rules, consider using 'Foo'.}}

            static void Do_Some_Test_() { } // Noncompliant {{Rename local function 'Do_Some_Test_' to match pascal case naming rules, trim underscores from the name.}}
        }
    }
}
namespace t1
{
    record FSM // Noncompliant {{Rename record 'FSM' to match pascal case naming rules, consider using 'Fsm'.}}
    {
    }
    record FSM2(string Param); // Noncompliant
}
namespace t4
{
    record AbcDEFgh { } // Compliant
    record Ab4DEFgh { } // Compliant
    record Ab4DEFGh { } // Noncompliant

    record _AbABaa { }  // Noncompliant

    record 你好 { }      // Compliant

    record AbcDEFgh2(string Param); // Compliant
    record Ab4DEFgh2(string Param); // Compliant
    record Ab4DEFGh2(string Param); // Noncompliant

    record _AbABaa2(string Param);  // Noncompliant

    record 你好2(string Param);      // Compliant
}

namespace TestSuffixes
{
    record IEnumerableExtensionsTest { }              // Noncompliant  {{Rename record 'IEnumerableExtensionsTest' to match pascal case naming rules, consider using 'EnumerableExtensionsTest'.}}
    record IEnumerableExtensionsTests { }             // Noncompliant

    record IEnumerableExtensionsTest2(string Param);  // Noncompliant
    record IEnumerableExtensionsTests2(string Param); // Noncompliant
}

namespace CSharp11
{
    public interface IMyInterface
    {
        static abstract void foo(); // Noncompliant {{Rename method 'foo' to match pascal case naming rules, consider using 'Foo'.}}
//                           ^^^
        static abstract void Foo();
    }

    public record FooRecord : IMyInterface
    {
        static void IMyInterface.foo() { } // Compliant, we can't change it
        static void IMyInterface.Foo() { }
    }
}
namespace CSharp10.t1
{
    record struct FSM // Noncompliant {{Rename record struct 'FSM' to match pascal case naming rules, consider using 'Fsm'.}}
    //            ^^^
    {
    }
    record struct FSM2(string Param); // Noncompliant
}
namespace CSharp10.t4
{
    record struct AbcDEFgh { } // Compliant
    record struct Ab4DEFgh { } // Compliant
    record struct Ab4DEFGh { } // Noncompliant

    record struct _AbABaa { }  // Noncompliant

    record struct 你好 { }      // Compliant

    record struct AbcDEFgh2(string Param); // Compliant
    record struct Ab4DEFgh2(string Param); // Compliant
    record struct Ab4DEFGh2(string Param); // Noncompliant

    record struct _AbABaa2(string Param);  // Noncompliant

    record struct 你好2(string Param);      // Compliant
}

namespace CSharp10.TestSuffixes
{
    record struct IEnumerableExtensionsTest { }              // Noncompliant
    record struct IEnumerableExtensionsTests { }             // Noncompliant

    record struct IEnumerableExtensionsTest2(string Param);  // Noncompliant
    record struct IEnumerableExtensionsTests2(string Param); // Noncompliant
}

namespace CSharp13
{
    public partial class PartialPropertyClass
    {
        public partial int MyPPPProperty { get; set; } // Noncompliant {{Rename property 'MyPPPProperty' to match pascal case naming rules, consider using 'MyPppProperty'.}}
        public partial int OtherPartialProperty { get; set; }
    }

    public partial class PartialPropertyClass
    {
        public partial int MyPPPProperty // Noncompliant
        {
            get => 42;
            set { }
        }
        public partial int OtherPartialProperty // Compliant
        {
            get => 42;
            set { }
        }
    }
}

namespace CSharp14
{
    public static class Extensions
    {
        extension(int number)

        {
            public static bool NonCCCOmpliant() => false;   // Noncompliant
            public static bool ThisIsCompliant() => true;   // Compliant

            public static bool NonCCCOmpliantProp => false; // Noncompliant
            public static bool ThisIsCompliantProp => true; // Compliant
        }
    }

    public class Events
    {
        public event System.EventHandler NonCCCOmpliant;    // Compliant FN
        public event System.EventHandler ThisIsCompliant;   // Compliant
    }

    public partial class PartialEvents
    {
        public partial event System.EventHandler NonCCCOmpliant;  // Compliant
        public partial event System.EventHandler ThisIsCompliant; // Compliant
    }
}
