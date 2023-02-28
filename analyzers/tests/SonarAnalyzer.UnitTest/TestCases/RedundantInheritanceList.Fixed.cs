using System;

namespace Tests.Diagnostics
{
    enum MyEnum : long
    {
    }
    enum MyEnum2
    {
    }
    enum MyEnum3
    {
    }

    class AA  //Fixed
    { }
    class AAA
    { }

    class A
    { }
    class B :
        IBase
    { }

    class BB
       : IBase
    { }

    interface IBase { }
    interface IA : IBase { }
    interface IB : IA
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

    public class Derived2B : IM2
    {
        public void Print()
        {
            Console.WriteLine("derived");
        }
    }

    public class Derived3 : IM2
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

    struct RedunantInterfaceImpl : IA { } // Fixed

    // Reproducer for FP: https://github.com/SonarSource/sonar-dotnet/issues/6823
    class Foo { }

    interface IBar
    {
        int Test();
    }

    class Bar : Foo { } // Error [CS0535]
    // Fixed
}
