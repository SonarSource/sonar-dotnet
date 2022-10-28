using System.Numerics;

namespace Tests.Diagnostics
{
    class MyClass : IEqualityOperators<MyClass, MyClass, MyClass>
    {
        public static MyClass operator ==(MyClass? left, MyClass? right) => new MyClass(); // Noncompliant FP (implementing IEqualityOperators interface require == operator overload)
        
        public static MyClass operator !=(MyClass? left, MyClass? right) => new MyClass();
    }
}
