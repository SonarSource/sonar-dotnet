using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Test interface inheritance
namespace Test_01
{
    public interface A
    {
        static abstract void Method();
    }

    public class B : A
    {
        public static void Method() { }

        public void Test_01(B foo) // Noncompliant {{Consider using more general type 'Test_01.A' instead of 'Test_01.B'.}}
//                            ^^^
        {
            foo.Method();
        }
    }
}

// Test interface inheritance with in-between interface
namespace Test_02
{
    public interface A
    {
        static abstract void Method();;
    }

    public interface B : A
    {
    }

    public class C : B
    {
        public static void Method() { }

        public void Test_02(C foo) // Noncompliant {{Consider using more general type 'Test_02.A' instead of 'Test_02.C'.}}
        {
            foo.Method();
        }
    }
}

// Test interface inheritance with hierarchy
namespace Test_03
{
    public interface A_Base
    {
        static abstract void Method();
    }

    public interface A_Derived : A_Base
    {
    }

    public interface B_Base
    {
        void OtherMethod();
    }

    public interface B_Derived : B_Base
    {
    }

    public class C : B_Derived, A_Derived
    {
        public static void Method() { }
        public void OtherMethod() { }

        public void Test_03(C foo) // Noncompliant {{Consider using more general type 'Test_03.A_Base' instead of 'Test_03.C'.}}
        {
            foo.Method();
        }
    }
}

