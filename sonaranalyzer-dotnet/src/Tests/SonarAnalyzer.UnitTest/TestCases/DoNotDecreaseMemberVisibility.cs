using System;

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
    }

    class B : A
    {
    }

    class C : B
    {
        private void Method_01(int count) { } // Noncompliant {{Make this member non-private or the class sealed.}}
//                   ^^^^^^^^^

        protected void Method_02(int count) { }

        void Method_03(int count) { } // Noncompliant

        private void Method_04(int count) { }

        private void Method_05(int something, string something2, params object[] someArgs) { } // Noncompliant

        private void Method_06<T>(T count) { }

        private virtual void Method_07(int count) { } // Noncompliant

        private override void Method_08(int count) { } // Noncompliant

        private new void Method_09(int count) { } // Noncompliant

        private void Method_10(int count) { }
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