using System;
using System.Collections.Generic;

namespace MyLibrary
{
    class A
    {
        public void Method_01(int count) { }

        public void Method_02(int count) { }

        public void Method_03(int count) { }

        protected void Method_04(int count) { }

        public void Method_05(int count, string foo, params object[] args) { }

        public void Method_06<T>(T count) { }

        public virtual void Method_07(int count) { }

        public virtual void Method_08(int count) { }

        public void Method_09(int count) { }

        // No method 10

        public void Method_11<T>(int count) { }

        public void Method_12<T, V>(T obj, V obj2) { }

        public void Method_13<T, V>(T obj, IEnumerable<V> obj2) { }

        public void Method_14(int count) { }

        public void Method_15(out int count) { count = 2; }

        public void Method_16(ref int count) { }

        public virtual void Method_17(int count) { }


        public int Property_01 { get; set; }

        public int Property_02 { get; set; }

        public int Property_03 { get; private set; }

        public int Property_04 { get; }

        public int Property_05 { get; set; }

        public int Property_06 { get; set; }

        public virtual int Property_07 { get; set; }

        public virtual int Property_08 { get; set; }

        public virtual int Property_09 { get; set; }

        public int Property_10 { get; set; }
    }

    class B : A
    {
    }

    class C : B
    {
        private void Method_01(int count) { } // Noncompliant {{This member hides 'MyLibrary.A.Method_01(int)'. Make it non-private or seal the class.}}
//                   ^^^^^^^^^

        protected void Method_02(int count) { } // Noncompliant

        void Method_03(int count) { } // Noncompliant

        private void Method_04(int count) { } // Noncompliant

        private void Method_05(int something, string something2, params object[] someArgs) { } // Noncompliant

        private void Method_06<T>(T count) { } // Noncompliant

        // Error@+1 [CS0621]
        private virtual void Method_07(int count) { } // Noncompliant

        // Error@+1 [CS0621, CS0507]
        private override void Method_08(int count) { } // Noncompliant

        private new void Method_09(int count) { }

        private void Method_10(int count) { }

        private void Method_11<V>(int count) { } // Noncompliant

        private void Method_12<V, T>(T obj, V obj2) { } // Noncompliant

        private void Method_13<V, T>(T obj, IEnumerable<T> obj2) { } // Noncompliant

        private void Method_14(int count) { } // Noncompliant

        private void Method_15(int count) { }

        private void Method_16(out int count) { count = 2; }

        // Error@+1 [CS0621,CS0507]
        private override void Method_17(int count) { } // Noncompliant


        private int Property_01 { get; set; } // Noncompliant

        public int Property_02 { private get; set; } // Noncompliant

        private int Property_03 { get; set; } // Noncompliant

        public int Property_04 { get; private set; }

        public int Property_05 { get; } // Noncompliant

        private int Property_06 { get; } // Noncompliant

        int i;

        // Note this cannot be auto-property, as it is a compiler error.
        public override int Property_07 { get { return i; } }

        // Error@+1 [CS0507] - cannot change modifier
        public override int Property_08 { get { return i; } private set { i = value; } } // Noncompliant

        public override int Property_09 { get; } // Error [CS8080]

        private string Property_10 { get; set; } // Noncompliant, return type is irrelevant for method resolution
    }

    class Foo
    {
        public void Method_01(int count) { }
    }

    sealed class Bar : Foo
    {
        private void Method_01(int count) { }
    }
}

namespace OtherNamespace
{
    public class Class1
    {
        internal void SomeMethod(string s) { }
    }

    public class Class2 : Class1
    {
        private void SomeMethod(string s) { } // Noncompliant
    }
}

namespace SomeNamespace
{
    public class Class3 : OtherNamespace.Class1
    {
        private void SomeMethod(string s) { }
    }
}

namespace FalsePositiveOnIndexers
{
    public class BaseClass
    {
        public int this[int index]
        {
            get { return index; }
            set { }
        }
    }

    public class DescendantClass : BaseClass
    {
        public int this[string name] // Compliant, parameters are of different types
        {
            get { return name.Length; }
        }
    }
}

namespace DefaultInterfaceMembers
{
    public interface IFoo
    {
        public void SomeMethod(int count) { }
    }

    public interface IBar : IFoo
    {
        private void SomeMethod(int count) { }
    }

    public class Consumer
    {
        public Consumer(IBar bar)
        {
            bar.SomeMethod(1); // Compliant, the method from IFoo is called
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8666
namespace Repro_8666
{
    public class BaseClass
    {
        protected virtual void DoSomething(IEnumerable<int> numbers) { }
    }

    public class DerivedClass : BaseClass
    {
        private void DoSomething(IEnumerable<string> numbers) { }   // Noncompliant - FP: the method in the derived class has different arguments
    }
}

namespace Events
{
    public class Class1
    {
        internal event EventHandler SomeEvent;
        internal event EventHandler SomeEvent2;
    }

    public class Class2 : Class1
    {
        private event EventHandler SomeEvent // Noncompliant
        //                         ^^^^^^^^^
        {
            add { }
            remove { }
        }

        private event EventHandler SomeEvent2; // Compliant FN: EventFieldDeclaration not supported yet.
    }

    public class Class3 : Class1
    {
        private new event EventHandler SomeEvent // Compliant: new keyword used to hide base class event
        {
            add { }
            remove { }
        }

        public new event EventHandler SomeEvent2; // Compliant: new keyword used to hide base class event
    }

    public class Class4
    {
        public virtual event EventHandler SomeEvent;
        public virtual event EventHandler SomeEvent2;
    }

    public class Class5 : Class4
    {
        public override event EventHandler SomeEvent // Compliant: override base class event
        {
            add { }
            remove { }
        }

        public override event EventHandler SomeEvent2; // Compliant: override base class event
    }
}
