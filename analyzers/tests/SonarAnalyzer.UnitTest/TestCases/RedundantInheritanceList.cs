using System;

namespace Tests.Diagnostics
{
    enum MyEnum : long
    {
    }
    enum MyEnum2
        : int //Noncompliant {{'int' should not be explicitly used as the underlying type.}}
//      ^^^^^
    {
    }
    enum MyEnum3
    {
    }

    class AA : Object //Noncompliant {{'Object' should not be explicitly extended.}}
    { }
    class AAA://Noncompliant
        Object
    { }

    class A
        : Object //Noncompliant
    { }
    class B :
        Object, //Noncompliant
//      ^^^^^^^
        IBase
    { }

    class BB
       : Object, //Noncompliant
         IBase
    { }

    interface IBase { }
    interface IA : IBase { }
    interface IB : IA
        , IBase //Noncompliant {{'IA' implements 'IBase' so 'IBase' can be removed from the inheritance list.}}
    { }

    interface IPrint1
    {
        void Print();
    }
    class Base : IPrint1
    {
        public void Print() { }
    }
    class A1 : Base
        , IPrint1 //Noncompliant
    { }
    class A2 : Base, IPrint1
    {
        public new void Print() { }
    }
    class A3 : Base, IPrint1
    {
        void IPrint1.Print() { }
    }

    interface IB1
    {
        void Method();
    }

    interface IB2 : IB1
    {
    }

    class C1 : IB2
        , IB1 // Noncompliant
    {
        public void Method() { }
    }

    public interface IM
    {
        void Print();
    }

    public interface IM2 : IM // not redundant
    {
    }

    public class Base2 : IM
    {
        public void Print()
        {
            Console.WriteLine("base");
        }
    }

    public class Derived1 : Base2, IM // not redundant
    {
        public void Print()
        {
            Console.WriteLine("derived");
        }
    }

    public class Derived2 : IM2
    {
        public void Print()
        {
            Console.WriteLine("derived");
        }
    }

    public class Derived2B : IM, // Noncompliant
        IM2
    {
        public void Print()
        {
            Console.WriteLine("derived");
        }
    }

    public class Derived3 : IM2
        , IM // Noncompliant
    {
        void IM.Print()
        {
            Console.WriteLine("derived");
        }
    }

    public class Derived4 : Base2, IM
    {
        void IM.Print()
        {
            Console.WriteLine("derived");
        }
    }
    interface IMyInt
    {
        void M();
    }

    class X1 : IMyInt
    {
        public void M()
        {
            throw new NotImplementedException();
        }
    }

    class X2 : X1
    {
        public new void M()
        {
            throw new NotImplementedException();
        }
    }

    class X3 : X2, IMyInt // Compliant
    {
    }

    struct RedunantInterfaceImpl : IA, IBase { } // Noncompliant {{'IA' implements 'IBase' so 'IBase' can be removed from the inheritance list.}}
    //                               ^^^^^^^

    // Reproducer for FP: https://github.com/SonarSource/sonar-dotnet/issues/6823
    class Foo { }

    interface IBar
    {
        int Test();
    }

    class Bar : Foo, IBar { } // Error [CS0535]
    // Noncompliant@-1 {{'Foo' implements 'IBar' so 'IBar' can be removed from the inheritance list.}} FP
}
