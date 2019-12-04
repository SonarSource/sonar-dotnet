using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class A
    {
        void a() { } // Noncompliant {{All 'a' method overloads should be adjacent.}}
//           ^

        void a(int a, char b) { }

        class B { }

        void a(string a) { } // Secondary
//           ^

        interface C { }

        void a(int a) { } // Secondary {{Non-adjacent overload}}
//           ^

        enum D { }

        void a(double a) { } // Secondary {{Non-adjacent overload}}
//           ^

        int myField;

        void a(char a) { } // Secondary {{Non-adjacent overload}}
//           ^

        int MyProperty
        {
            get;
        }

        void a(char a, int b) { } // Secondary {{Non-adjacent overload}}
//           ^
    }

    class B
    {
        B() { } // Noncompliant {{All 'B' method overloads should be adjacent.}}

        B(int a) { }

        B(char a) { }

        ~B() { }

        B(double a) { } // Secondary {{Non-adjacent overload}}
    }

    class C
    {
        public static C operator -(C c1) // Compliant - this one is unary -, while the other is binary -
        {
            return null;
        }

        void a() { }

        public static implicit operator B(C d) => null; // Compliant - this operator and the B() method are not related

        void B() { }

        public static C operator -(C c1, C c2)
        {
            return null;
        }
    }

    interface D
    {
        void DoSomething(double b);

        void DoSomething();

        void DoSomething(int a);
    }

    interface E
    {
        void DoSomething(double b); // Noncompliant {{All 'DoSomething' method overloads should be adjacent.}}
//           ^^^^^^^^^^^

        void DoSomething();

        void DoSomethingElse();

        void DoSomething(int a); // Secondary {{Non-adjacent overload}}
//           ^^^^^^^^^^^
    }

    struct F
    {
        F(char a) { }

        F(int a) { }

        void MyStructMethod() { } // Noncompliant {{All 'MyStructMethod' method overloads should be adjacent.}}

        void MyStructMethod(int a) { }

        void MyStructMethod2() { }

        void MyStructMethod(char a) { } // Secondary {{Non-adjacent overload}}
    }

    class G : D, E
    {
        public void DoSomething(double a) { } // Noncompliant

        // Compliant - we dont not raise issues for explicit interface implementation as it is a corner case and it can make sense to group implementation by interface
        void D.DoSomething() { }

        public void DoSomethingElse() { }

        void E.DoSomething() { } // Compliant - explicit interface implementation

        public void DoSomething(int a) { } // Secondary
    }

    class H
    {
        private void DoSomething(int a) { } // Compliant - not same accessibility as the other method

        void DoSomethingElse() { }

        public void DoSomething() { }
    }

    class I
    {
        private void DoSomething(int a) { } // Noncompliant

        void DoSomethingElse() { }

        private void DoSomething() { } // Secondary {{Non-adjacent overload}}
    }

    class J
    {
        void DoSomething<T>(T t) { } // Noncompliant

        void DoSomethingElse() { }

        void DoSomething(int a) { } // Secondary {{Non-adjacent overload}}
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2776
    public class StaticMethodsTogether
    {

        public StaticMethodsTogether() { }
        public StaticMethodsTogether(int i) { }

        public static void MethodA() // Compliant - static methods are grouped together, it's ok
        {
        }

        public static void MethodB()
        {
        }

        static StaticMethodsTogether() { }  //Compliant - static constructor can be grouped with static methods

        public void MethodA(int i)
        {
        }

        public static void MethodC()    // Noncompliant - When there're more shared methods, they still should be together
        {
        }

        public static void MethodD()
        {
        }

        public static int MethodD(int i)
        {
            return i;
        }

        public static void MethodD(bool b)
        {
        }

        public static void MethodC(bool b)    // Secondary
        {
        }

        public static void Separator()
        {
        }

        public static int MethodC(int i)    // Secondary
        {
            return i;
        }

    }

    public abstract class MustOverrideMethodsTogether
    {

        protected abstract void DoWork(bool b); //Compliant - abstract methods are grouped together, it's OK
        protected abstract void DoWork(int i);

        protected MustOverrideMethodsTogether() { }

        protected void DoWork() //Compliant - abstract methods are grouped together, it's OK
        {
            DoWork(true);
            DoWork(42);
        }

        protected abstract void OnEvent(bool b);    //Noncompliant
        protected abstract void OnProgress();
        protected abstract void OnEvent(int i);     //Secondary

    }

    public class Inheritor : MustOverrideMethodsTogether
    {

        protected override void DoWork(bool b)
        {
        }

        protected override void DoWork(int i)
        {
        }

        protected override void OnEvent(bool b) //Noncompliant
        {
        }

        protected override void OnProgress()
        {
        }

        protected override void OnEvent(int i)  //Secondary
        {
        }

    }

}
