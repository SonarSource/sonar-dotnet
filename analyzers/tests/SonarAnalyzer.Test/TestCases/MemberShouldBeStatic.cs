using System;

namespace Tests.Diagnostics
{
    public interface IInterface1
    {
        int InterfaceMethod1();
        int InterfaceProperty1 { get; set; }
    }

    public class Class1
    {
        private int instanceMember = 0;
        private static int staticMember = 0;
        protected IInterface1 instanceInterface = null;

        // properties
        public int Property1 { get; }
        public int Property2 { get; set; }
        public int Property3 { get { return instanceMember; } }
        public int Property4 { get { return this.instanceMember; } }
        public int Property5 { get { return staticMember; } } // Noncompliant {{Make 'Property5' a static property.}}
        public int Property6 { get { return Class1.staticMember; } } // Noncompliant
        public int Property7 { get { return new Class1().instanceMember; } } // Noncompliant
        public int Property8 { get { return 0; } } // Noncompliant
        public int Property9 { get { return 0; } set { instanceMember = value; } }
        public int Property10 { get { return 0; } set { this.instanceMember = value; } }
        public int Property11 => 0; // Noncompliant
        public int Property12 => instanceMember;
        public int Property13 => this.instanceMember;
        public int Property14 => staticMember; // Noncompliant
        public int Property15 => Class1.staticMember; // Noncompliant
        public int Property16 => new Tests.Diagnostics.Class1().instanceMember; // Noncompliant
        public int Property17 => instanceInterface.InterfaceProperty1;
        public Class1 Property18 => this;
        public static int StaticProperty1 => 0;
        public static int StaticProperty2 => staticMember;
        public virtual int VirtualProperty { get; set; }
        public int Property20 // Noncompliant
        {
            get => 1;
        }

        public int Property21
        {
            get => instanceMember;
            set => instanceMember = value;
        }

        public int Property22
        {
            get => instanceMember;
        }

        public int Property23
        {
            set => instanceMember = value;
        }

        // indexers are always instance
        public int this[string index] { get { return 0; } } // Compliant!

        // methods
        public int Method1() { return 0; } // Noncompliant
        public int Method2() { return instanceMember; }
        public int Method3() { return this.instanceMember; }
        public int Method4() { return staticMember; } // Noncompliant
        public int Method5() { return Class1.staticMember; } // Noncompliant {{Make 'Method5' a static method.}}
        public int Method6() { return new Class1().instanceMember; } // Noncompliant
        public int Method7(Class1 arg) { return arg.instanceMember; } // Noncompliant
        public int Method8(Class1 arg) { return arg.instanceInterface.InterfaceProperty1; } // Noncompliant
        public int Method8_0(Class1 arg) { return (arg).instanceInterface.InterfaceProperty1; } // Noncompliant
        public int Method8_1(Class1 arg) { return (int)arg?.instanceInterface?.InterfaceProperty1; } // Noncompliant
        public int Method8_2(Class1 arg) { return (int)((arg))?.instanceInterface?.InterfaceProperty1; } // Noncompliant
        // Error@+1 [CS0030]
        public int Method8_3(Class1 arg) { return ((int)arg)?.instanceInterface?.InterfaceProperty1; } // Noncompliant
        public void Method9() { (Property2 + 1).ToString(); }

        public int Method10() => 0; // Noncompliant
        public int Method11() => instanceMember;
        public int Method12() => this.instanceMember;
        public int Method13() => staticMember; // Noncompliant
        public int Method13_1() => (staticMember); // Noncompliant
        public int Method14() => Class1.staticMember; // Noncompliant
        public int Method15() => new Class1().instanceMember; // Noncompliant
        public int Method15_1() => (new Class1()).instanceMember; // Noncompliant
        public int Method16() => 0; // Noncompliant
        public int Method17(Class1 arg) => arg.instanceMember; // Noncompliant
        public int Method18(Class1 arg) => arg.instanceInterface.InterfaceProperty1; // Noncompliant
        public int Method19() { return instanceInterface.InterfaceProperty1.GetHashCode(); }
        public int Method19_1() { return ((((((instanceInterface))).InterfaceProperty1))).GetHashCode(); }
        public int Method19_2() { return (((((((((this))).instanceInterface))).InterfaceProperty1))).GetHashCode(); }
        public Class1 Method21() => this;
        public string Method22() => nameof(instanceMember); // Noncompliant

        public static int StaticMethod1() { return 0; }
        public static int StaticMethod2() { return staticMember; }
        public static int StaticMethod3() => 0;
        public static int StaticMethod4() => staticMember;

        public bool Method30() { return 0 > instanceMember; }
        public bool Method31() { var a = instanceMember; return a > 0; }
        public bool Method32() { if (instanceMember - 5 > 10) return true; else return false; }
        public int Method33() { return Math.Abs(instanceMember); }

        public virtual int VirtualMethod() { return 0; }

        protected void GenericMethod<T>(T arg) { /*do nothing*/ }
        protected void Method34() { GenericMethod<int>(5); }
    }

    public abstract class Class2
    {
        public abstract int AbstractProperty { get; set; }
        public abstract int AbstractMethod();
    }

    public class Class3 : Class1
    {
        public override int VirtualProperty { get { return 0; } set { /*do nothing*/ } }
        public override int VirtualMethod() { return 0; }
        public new int Method1() { return 0; }
        public new int Property1 { get { return 0; } }
        public bool Method44(Class1 test) { return test.Property1 == instanceInterface.InterfaceProperty1; }
    }

    public class Class4 : IInterface1
    {
        public int InterfaceProperty1 { get { return 0; } set { } } // if Class4 adds an explicit implementation of this property an issue will be raised here
        public int InterfaceMethod1() => 0;
    }

    // Adding exception for SuppressMessage https://github.com/SonarSource/sonar-dotnet/issues/631
    public class Class5
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Foo", "Bar", Justification = "baz")]
        public int Property1 { get { return 0; } } // Noncompliant
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Foo", "Bar", Justification = "baz")]
        public int Property2 => 0; // Noncompliant
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Foo", "Bar", Justification = "baz")]
        public int Property3 { } // Error [CS0548]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Foo", "Bar", Justification = "baz")]
        public int Method1() { return 0; } // Noncompliant
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Foo", "Bar", Justification = "baz")]
        public int Method2() => 0; // Noncompliant
    }

    public class Class6
    {
        [System.ObsoleteAttribute]
        public int Property1 { get { return 0; } } // Compliant, any attribute disables this rule
        [System.ObsoleteAttribute]
        public int Property2 => 0; // Compliant, any attribute disables this rule
        [System.ObsoleteAttribute]
        public int Method1() { return 0; } // Compliant, any attribute disables this rule
        [System.ObsoleteAttribute]
        public int Method2() => 0; // Compliant, any attribute disables this rule
    }

    // The following test cases are linked to FP when using ASP controllers.
    // See https://github.com/SonarSource/sonar-dotnet/issues/733
    public class WebClass1 : System.Web.Mvc.Controller
    {
        public int Foo() => 0;

        protected int FooFoo() => 0; // Noncompliant
    }

    public class DerivedWebClass1 : WebClass1
    {
        public int Bar() => 1;

        protected int BarBar() => 1; // Noncompliant
    }

    public class WebClass2 : System.Web.Http.ApiController
    {
        public int Foo() => 0;

        protected int FooFoo() => 0; // Noncompliant
    }

    public class DerivedWebClass2 : WebClass2
    {
        public int Bar() => 1;

        protected int BarBar() => 1; // Noncompliant
    }

    public class WebClass3 : Microsoft.AspNetCore.Mvc.Controller
    {
        public int Foo() => 0;

        protected int FooFoo() => 0; // Noncompliant
    }

    public class DerivedWebClass3 : WebClass3
    {
        public int Bar() => 1;

        protected int BarBar() => 1; // Noncompliant
    }

    public class Foo
    {
        protected void Application_AuthenticateRequest(Object sender, EventArgs e) { }
        protected void Application_BeginRequest(object sender, EventArgs e) { }
        protected void Application_End() { }
        protected void Application_EndRequest(Object sender, EventArgs e) { }
        protected void Application_Error(object sender, EventArgs e) { }
        protected void Application_Init(object sender, EventArgs e) { }
        protected void Application_Start() { }
        protected void Session_End(object sender, EventArgs e) { }
        protected void Session_Start(object sender, EventArgs e) { }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3204
    public class Repro_3204<TFirst, TSecond>
    {
        public int BuildSomething() // Compliant, we don't raise inside generic class, so it doesn't have to be invoked like Repro_3204<SomethingA, SomethingB>.BuildSomething() - too difficult to read
        {
            return 42;
        }

        public int WithGenericArguments<TFirst, TSecond>() // Compliant
        {
            return 42;
        }

        private int PrivateMethod() => 42;      // Noncompliant, private member is easy to invoke => should be private
        protected int ProtectedMethod() => 42;  // Noncompliant

        internal int InternalMethod() => 42;                // Compliant
        protected internal int ProtectedInternal() => 42;   // Compliant as internal invocation would be hard

        public class PublicNestedInGenericType
        {
            public int BuildSomething() => 42;  // Compliant
        }

        private class PrivateNestedInGenericType
        {
            public int BuildSomething() => 42;  // Noncompliant
        }

        public static class PublicStaticNestedInGenericType
        {
            public static int BuildSomething() => 42;  // Compliant
        }

        private static class PrivateStaticNestedInGenericType
        {
            public static int BuildSomething() => 42;  // Compliant
        }

        public class Nongeneric
        {
            private class Generic<TFirst, TSecond>
            {
                public int BuildSomething() => 42;      // Compliant

                private class NestedNongeneric
                {
                    public int BuildSomething() => 42;  // Noncompliant, it can be accessed as NestedNongeneric.BuildSomething() from Generic<TFirst, TSecond> scope
                }
            }
        }

        public class GenericOuter<TFirst, TSecond>
        {
            private class NongenericInner
            {
                public int BuildSomething() => 42;  // Noncompliant
            }
        }
    }

    public class Repro_3204_OK
    {
        public int BuildSomething<TFirst, TSecond>() // Noncompliant, this generic method should be static
        {
            return 42;
        }
    }

    public class NameColon
    {
        string local = "hey";

        public decimal UsingParameter(string parameter) // Noncompliant
        {
            throw new ArgumentNullException(paramName: parameter);
        }

        public decimal UsingLocal() // Compliant
        {
            throw new ArgumentNullException(paramName: local);
        }
    }

    public class PropertyInitialization
    {
        public int Prop { get; set; }

        public PropertyInitialization Create() // FN - because of the property initialization
        {
            return new PropertyInitialization { Prop = 42 };
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8025
    partial class PartialClass
    {
        public void WriteEverything()
        {
            Console.WriteLine("Something");

            WriteMore();
        }

        partial void WriteMore();
    }

    partial class PartialClass
    {
        partial void WriteMore() // Compliant
        {
            Console.WriteLine("More");
        }
    }
}
