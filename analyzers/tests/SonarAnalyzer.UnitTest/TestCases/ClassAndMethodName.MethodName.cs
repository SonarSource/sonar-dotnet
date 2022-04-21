namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        void foo(); // Noncompliant {{Rename method 'foo' to match pascal case naming rules, consider using 'Foo'.}}
//           ^^^
        void Foo();
    }

    public class FooClass : IMyInterface
    {
        void a() { } // Noncompliant
        void A() { } // Compliant
        void foo() { } // Noncompliant
        void IMyInterface.foo() { } // Compliant, we can't change it
        void Foo() { }
        void IMyInterface.Foo() { }

        void
        bar() // Noncompliant
        { }

        public event MyEventHandler foo_bar;
        public delegate void MyEventHandler();

        void Do_Some_Test() { }
        void Do_Some_Test_() { } // Noncompliant
        void ____() { } // Noncompliant {{Rename method '____' to match pascal case naming rules, trim underscores from the name.}}
        protected void Application_Start() { }

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern int ____MessageBox(int h, string m, string c, int type); // Compliant

        public int MyPPPProperty { get; set; } // Noncompliant {{Rename property 'MyPPPProperty' to match pascal case naming rules, consider using 'MyPppProperty'.}}

        public void Should_define_convention_that_returns_metadata_module_type() { } // Compliant

        public void Should_Define_Convention_That_Returns_Metadata_Module_Type() { } // Compliant

        public void Should_return_JSON_serialized_querystring() { } // Compliant

        public void IsLocal_should_return_true_if_userHostAddr_is_localhost_IPV4() { } // Compliant

        public void 你好() { }

        public void Łódź() { }

        public int ArrowedMethod() => 42;

        public int ArrowedProperty1 => 42;

        public int ArrowedProperty2
        {
            get => 41;
            set => bar();
        }
    }

    [System.Runtime.InteropServices.ComImport(),
     System.Runtime.InteropServices.Guid("00000000-0000-0000-0000-000000000001")]
    public abstract class MMMM
    {
        public abstract void MMMMMethod(); // Compliant
    }

    public class Base
    {
        public virtual void foo() { } // Noncompliant
    }
    public class Derived : Base
    {
        public override void foo() // Compliant
        {
            base.foo();
        }
    }

    public partial class SomeClass
    {
        partial void MY_METHOD()
        {
        }
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/2290
    public class AllowTwoLettersAcronyms
    {
        public void IOStream() { }
        public void AddUIIntegrationTypes() { }
        public void MyEOF() { } // Noncompliant
        public void MyEOFile() { }
    }

    public class WithLocalFunctions
    {
        public void Method()
        {
            void foo() { } // Noncompliant {{Rename local function 'foo' to match pascal case naming rules, consider using 'Foo'.}}

            static void Do_Some_Test_() { } // Noncompliant {{Rename local function 'Do_Some_Test_' to match pascal case naming rules, trim underscores from the name.}}
        }
    }

    public class Invalid
    {
        public int () => 42; // Error [CS1519,CS8124,CS1519]
    }
}
