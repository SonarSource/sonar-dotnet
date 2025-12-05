using Extensions;
using System.Numerics;

record MyType
{
    public static MyType operator ==(MyType x, MyType y) // Error [CS0111]
    {
        return null;
    }

    public static MyType operator !=(MyType x, MyType y) => null; // Error [CS0111]
}

class MyClass : IEqualityOperators<MyClass, MyClass, MyClass>
{
    public static MyClass operator ==(MyClass? left, MyClass? right) => new MyClass(); // Compliant 
    
    public static MyClass operator !=(MyClass? left, MyClass? right) => new MyClass();
}

namespace Extensions
{
    class Sample { }

    static class Extensions
    {
        extension(Sample)
        {
            public static bool operator ==(Sample left, Sample right) => true;  // FN NET-2709
            public static bool operator !=(Sample left, Sample right) => true;
        }
    }
}

class CustomCompoundAssignment
{
    public int Value;

    public void operator +=(int x) => Value += x;
    public static bool operator ==(CustomCompoundAssignment left, CustomCompoundAssignment right) => true;  // Noncompliant
    public static bool operator !=(CustomCompoundAssignment left, CustomCompoundAssignment right) => true;
}
