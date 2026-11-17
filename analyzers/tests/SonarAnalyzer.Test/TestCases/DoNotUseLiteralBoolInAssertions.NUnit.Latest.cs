using System.Runtime.CompilerServices;
using NUnit.Framework.Legacy;

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
        public void ObjectOverload(BoolOrString value, CustomUnion custom, bool? nullable, BoolOrString? nullableValue, CustomUnion? nullableCustom)
        {
            ClassicAssert.AreEqual(true, value);            // Noncompliant, a boxed bool is compared with a boxed union, always fails
            ClassicAssert.AreEqual(true, custom);           // Noncompliant
            ClassicAssert.AreEqual(true, nullable);         // Compliant, boxing unwraps the nullable
            ClassicAssert.AreEqual(true, nullableValue);    // FN NET-5153, a nullable union boxes to the union, so this always fails too
            ClassicAssert.AreEqual(true, nullableCustom);   // FN NET-5153
        }
    }
}
