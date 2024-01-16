using System.Numerics;

namespace Tests.Diagnostics
{
    class MyClass : IEqualityOperators<MyClass, MyClass, MyClass>
    {
        public static MyClass operator ==(MyClass? left, MyClass? right) => new MyClass(); // Compliant 
        
        public static MyClass operator !=(MyClass? left, MyClass? right) => new MyClass();
    }
}
