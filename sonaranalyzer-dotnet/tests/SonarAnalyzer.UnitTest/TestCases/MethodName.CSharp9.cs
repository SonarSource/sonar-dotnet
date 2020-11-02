namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        void foo(); // Noncompliant {{Rename method 'foo' to match pascal case naming rules, consider using 'Foo'.}}
//           ^^^
        void Foo();
    }

    public record FooRecord : IMyInterface
    {
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
