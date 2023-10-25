using System.Runtime.CompilerServices;

class ZeroOverheadMemberAccess
{
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    extern static UserData CallConstructor(int x1, int x2, int x3, int x4);                 // Compliant: signature has to match target

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Method")]
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
