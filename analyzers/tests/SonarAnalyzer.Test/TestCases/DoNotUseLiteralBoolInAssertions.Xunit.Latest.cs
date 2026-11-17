using System.Runtime.CompilerServices;
using Xunit;

namespace Tests.Diagnostics
{
    public union BoolOrString(bool, string);

    [Union]
    public struct CustomUnion
    {
        public CustomUnion(bool value) => Value = value;
        public CustomUnion(string value) => Value = value;
        public object Value { get; }
    }

    class Foo
    {
        public void Union(BoolOrString value, bool b)
        {
            Assert.Equal(true, value);          // Compliant, the literal is converted to the union
            Assert.Equal(true, b);              // Noncompliant
            Assert.Equal(value, false);         // Compliant
            Assert.Equal(b, false);             // Noncompliant
            Assert.NotEqual(true, value);       // Compliant
            Assert.NotEqual(true, b);           // Noncompliant
            Assert.StrictEqual(true, value);    // Compliant
            Assert.StrictEqual(true, b);        // Noncompliant
        }

        public void CustomUnions(CustomUnion custom, bool b)
        {
            Assert.Equal(true, custom);         // Compliant, the literal is converted to the custom union
            Assert.Equal(true, b);              // Noncompliant
        }

        public void NullableUnion(BoolOrString? value, bool b)
        {
            Assert.Equal(true, value);          // Compliant, T is inferred as Nullable<BoolOrString>
            Assert.Equal(true, b);              // Noncompliant
            Assert.Equal(value, false);         // Compliant
            Assert.NotEqual(true, value);       // Compliant
            Assert.StrictEqual(true, value);    // Compliant
        }

        public void ObjectOverload(BoolOrString value, BoolOrString? nullableValue)
        {
            Assert.Same(true, value);           // Noncompliant, a boxed bool is never the same reference as a boxed union
            Assert.Same(true, nullableValue);   // FN NET-5153, a nullable union boxes to the union, so this always fails too
        }
    }
}
