using System;
using System.Runtime.InteropServices;

// https://github.com/SonarSource/sonar-dotnet/issues/8156
namespace Repro_8156
{
    using System.Runtime.CompilerServices;

    class ZeroOverheadMemberAccess
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        extern static UserData CallConstructor(int x1, int x2, int x3, int x4);                 // Compliant: signature has to match target

        [UnsafeAccessorAttribute(UnsafeAccessorKind.Method, Name = "Method")]
        extern static void CallMethod(UserData userData, int x1, int x2, int x3, int x4);       // Compliant: signature has to match target

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "StaticMethod")]
        extern static void CallStaticMethod(UserData userData, int x1, int x2, int x3, int x4); // Compliant: signature has to match target
    }

    class UserData
    {
        UserData(int x1, int x2, int x3, int x4) { }         // Noncompliant

        void Method(int x1, int x2, int x3) { }              // Compliant
        static void StaticMethod(int x1, int x2, int x3) { } // Compliant
    }
}

public class MyWrongClass(int p1, int p2, int p3, int p4) { } // Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}

public class SubClass(int p1, int p2, int p3, int p4) : MyWrongClass(p1, p2, p3, p4) { } // Compliant: base class requires them

public class SubClass2() : MyWrongClass(1, 2, 3, 4) // Compliant
{
    public SubClass2(int p1, int p2, int p3, int p4, int p5) : this() { } // Noncompliant

    void Method()
    {
        var a = (int p1 = 1, int p2 = 2, int p3 = 3, int p4 = 4) => true; // Noncompliant {{Lambda has 4 parameters, which is greater than the 3 authorized.}}
    }
}

public struct MyWrongStruct(int p1, int p2, int p3, int p4) { } // Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}
