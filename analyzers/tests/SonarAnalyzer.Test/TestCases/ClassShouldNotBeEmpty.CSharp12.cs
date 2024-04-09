namespace Compliant
{
    class ChildClass() : BaseClass(42) { } // Compliant
    
    class ChildClassWithParameters(int value) : BaseClass(value) { } // Compliant

    class BaseClass(int value) { }
}

namespace Noncompliant
{
    class ChildClass() : BaseClass() { } // Noncompliant

    class BaseClass()
    {
        public int Value { get; init; }
    }
}
