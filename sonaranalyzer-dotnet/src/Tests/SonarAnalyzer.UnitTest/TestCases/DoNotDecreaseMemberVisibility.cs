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


        public int Property_01 { get; set; }

        public int Property_02 { get; set; }

        public int Property_03 { get; private set; }

        public int Property_04 { get; }

        public int Property_05 { get; set; }

        public int Property_06 { get; set; }
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

        private virtual void Method_07(int count) { } // Noncompliant

        private override void Method_08(int count) { } // Noncompliant

        private new void Method_09(int count) { } // Noncompliant

        private void Method_10(int count) { }

        private void Method_11<V>(int count) { } // Noncompliant

        private void Method_12<V, T>(T obj, V obj2) { } // Noncompliant

        private void Method_13<V, T>(T obj, IEnumerable<T> obj2) { } // Noncompliant


        private int Property_01 { get; set; } // Noncompliant

        public int Property_02 { private get; set; } // Noncompliant

        private int Property_03 { get; set; } // Noncompliant

        public int Property_04 { get; private set; }

        public int Property_05 { get; } // Noncompliant

        private int Property_06 { } // Noncompliant
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