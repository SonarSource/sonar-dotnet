namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        void foo(); // Noncompliant {{Rename method 'foo' to match camel case naming rules, consider using 'Foo'.}}
//           ^^^
        void Foo();
    }

    public class FooClass : IMyInterface
    {
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
        void ____() { } // Noncompliant {{Rename method '____' to match camel case naming rules, trim underscores from the name.}}
        protected void Application_Start() { }

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern int ____MessageBox(int h, string m, string c, int type); // Compliant

        public int MyPPProperty { get; set; } // Noncompliant {{Rename property 'MyPPProperty' to match camel case naming rules, consider using 'MyPpProperty'.}}

        public void Should_define_convention_that_returns_metadata_module_type() { } // Compliant

        public void Should_Define_Convention_That_Returns_Metadata_Module_Type() { } // Compliant

        public void Should_return_JSON_serialized_querystring { } // Compliant

        public void IsLocal_should_return_true_if_userHostAddr_is_localhost_IPV4 { } // Compliant

        public void 你好 { }

        public void Łódź() { }
    }

    [System.Runtime.InteropServices.ComImport()]
    public class MMMM
    {
        public void MMMMMethod() { } // Compliant
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
}
