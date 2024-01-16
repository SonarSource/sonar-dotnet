using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class A
    {
        void a() { } // Noncompliant {{All 'a' method overloads should be adjacent.}}
        //   ^

        void a(int a, char b) { }

        class B { }

        void a(string a) { } // Secondary
        //   ^

        interface C { }

        void a(int a) { } // Secondary {{Non-adjacent overload}}
        //   ^

        enum D { }

        void a(double a) { } // Secondary {{Non-adjacent overload}}
        //   ^

        int myField;

        void a(char a) { } // Secondary {{Non-adjacent overload}}
        //   ^

        int MyProperty
        {
            get;
        }

        void a(char a, int b) { } // Secondary {{Non-adjacent overload}}
        //   ^
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
        //   ^^^^^^^^^^^

        void DoSomething();

        void DoSomethingElse();

        void DoSomething(int a); // Secondary {{Non-adjacent overload}}
        //   ^^^^^^^^^^^
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

        public void SeparateFromSameInterfaceD() { }

        // Compliant - we dont not raise issues for explicit interface implementation as it is a corner case and it can make sense to group implementation by interface
        void D.DoSomething() { }

        public void DoSomethingElse() { }

        void E.DoSomething() { } // Compliant - explicit interface implementation

        public void SeparateFromSameInterfaceE() { }

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

    class K
    {
        public void Lorem() { }

        public void DoSomething() { }

        private void DoSomething(int i) { }    // Compliant interleaving with different accesibility

        protected void DoSomething(bool b) { }

        public void DoSomething(string s) { }

        private void Lorem(int i) { }       // Compliant, different accessibility

    }

    public class L
    {
        public void MethodA() { }

        public void MethodB() { }

        public static void MethodA(int i) { } // Compliant as one is static the other is not
    }

    public abstract class M
    {
        public abstract void MethodA();

        public void MethodB() { }

        public void MethodA(int i) { } // Compliant as one is abstract and the other is not
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

        public void DoSomething() { }
        protected abstract void DoSomething(bool b);     //Compliant interleaving with abstract
        public void DoSomething(int i) { }

    }

    public class Inheritor : MustOverrideMethodsTogether
    {

        protected override void DoWork(bool b)
        {
        }

        public static void DoWork(string s) //Compliant interleaving with static
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

        protected override void DoSomething(bool b)
        {
        }

    }

    public class InterfaceImplementationTogether : IEqualityComparer<int>, IEqualityComparer<string>
    {
        public bool Equals(int x, int y) { return true; }

        public int GetHashCode(int obj) { return 0; }

        public bool Equals(string x, string y) { return true; }

        public int GetHashCode(string obj) { return 0; }
    }

    public interface ITest<TItem>
    {
        void Aaa(TItem x);
        event EventHandler<TItem> Eee;
        TItem this[TItem index] { get; set; }
        TItem Ppp { get; set; }
        void Zzz(TItem x);
    }

    public class InterfaceImplementationTogetherWithPropertiesAndEvents : ITest<int>, ITest<string>
    {
        public event EventHandler<int> Eee;
        public void Aaa(int x) { }
        public int this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        int ITest<int>.Ppp { get; set; }
        public void Zzz(int x) { }

        event EventHandler<string> ITest<string>.Eee
        {
            add { }
            remove { }
        }
        public void Aaa(string x) { }
        public string this[string index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Ppp { get; set; }
        public void Zzz(string x) { }
    }

    public class ImplicitInterfaceWithEventHandler : ITest<int>
    {
        public void Zzz(string x) { } // Noncompliant

        // Implicit interface implementation groupped together
        public void Aaa(int x) { }
        int ITest<int>.Ppp { get; set; }
        public int this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public event EventHandler<int> Eee; // C# events are not recognized as parts of interface group, grouping is splitted here
        public void Zzz(int x) { } // Secondary
    }

    public interface ICancel
    {
        void Cancel();
        void Cancel(bool b);
        void Renew();
    }

    public class InterfaceImplementationMethodsTogether : ICancel
    {
        public void Cancel() { } // Noncompliant, it should be adjecent inside same interface implementation group
        public void Renew() { }
        public void Cancel(bool b) { } // Secondary
    }

    public interface ISecond
    {
        void A();
        void Cancel();
        void B();
    }

    public class ImplementsBoth : ICancel, ISecond
    {
        public void Cancel(bool b) { }
        public void Renew() { }

        public void A() { }
        public void Cancel() { } // Implements ICancel and ISecond
        public void B() { }
    }

    public class ImplementsBothFalseNegative : ICancel, ISecond
    {
        public void Cancel(bool b) { } // Noncompliant
        public void Renew() { }

        public void A() { }
        public void B() { }

        public void Something() { }

        public void Cancel() { } // Secondary, implements ICancel and ISecond
    }

    public interface IInterfaceSample
    {
        void Grouping();
        void DoSomething();
    }

    public class Interface_InterfaceMembersTogether : IInterfaceSample
    {
        private void DoSomething(int nonInterfaceOverload) { } // Compliant. DoSomething is kept with the other interface implementations.
        public void Grouping() { }
        public void DoSomething() { }
    }

    public class Interface_OverloadsTogether : IInterfaceSample
    {
        public void Grouping() { } // Compliant. Interface members are not kept together, but overloads are.
        private void DoSomething(int nonInterfaceOverload) { }
        public void DoSomething() { }
    }

    public class Interface_OverloadsAndInterfacesMixed : IInterfaceSample
    {
        public void Grouping() { } // FN. Neither the interface members nor the overoads are kept together.
        private void DoSomething(int nonInterfaceOverload) { }
        private void Grouping(int nonInterfaceOverload) { }
        public void DoSomething() { }
    }

    public class Interface_ExplicitImpl : IInterfaceSample
    {
        public void Grouping() { }
        private void DoSomething(int nonInterfaceOverload) { }
        private void GroupEnd() { }
        void IInterfaceSample.DoSomething() { } // Compliant. Explicit interface members can form their own group.
    }

    public interface IOverloadInterfaceSample
    {
        void DoSomething();
        void DoSomething(int overload);
    }

    public class InterfaceOverload_InterfacemembersTogether : IOverloadInterfaceSample
    {
        public void DoSomething(string nonInterfaceOverload) { } // Compliant. Interface members are kept separate
        public void EndGrouping() { }
        public void DoSomething() { }
        public void DoSomething(int overload) { }
    }

    public class InterfaceOverload_InterfacemembersSplit : IOverloadInterfaceSample
    {
        public void DoSomething() { }             // Noncompliant
        public void EndGrouping() { }
        public void DoSomething(int overload) { } // Secondary
    }

    public class InterfaceOverload_ExplicitAndImplicit : IOverloadInterfaceSample
    {
        public void DoSomething() { } // Compliant. Implicit and explicit interface implementations form their own group.
        public void EndGrouping() { }
        void IOverloadInterfaceSample.DoSomething(int overload) { }
    }

    public class InterfaceOverload_Explicit : IOverloadInterfaceSample
    {
        void IOverloadInterfaceSample.DoSomething() { } // FN. Explicit overload implementations should be kept together.
        public void EndGrouping() { }
        void IOverloadInterfaceSample.DoSomething(int overload) { }
    }
}
