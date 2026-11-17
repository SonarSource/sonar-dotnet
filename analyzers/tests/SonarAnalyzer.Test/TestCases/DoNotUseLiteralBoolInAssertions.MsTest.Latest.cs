using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

    [Union]
    public class CustomUnionWithInterface : IUnion
    {
        public CustomUnionWithInterface(bool value) => Value = value;
        public CustomUnionWithInterface(string value) => Value = value;
        public object Value { get; }
    }

    class Foo
    {
        public void Union(BoolOrString value, bool b)
        {
            Assert.AreEqual(true, value);     // Compliant NET-4908, the literal is converted to the union
            Assert.AreEqual(true, b);         // Noncompliant
            Assert.AreEqual(value, false);    // Compliant
            Assert.AreEqual(b, false);        // Noncompliant
            Assert.AreNotEqual(true, value);  // Compliant
            Assert.AreNotEqual(true, b);      // Noncompliant
        }

        public void CustomUnions(CustomUnion custom, CustomUnionWithInterface customWithInterface, bool b)
        {
            Assert.AreEqual(true, custom);              // Compliant, the literal is converted to the custom union
            Assert.AreEqual(true, customWithInterface); // Compliant
            Assert.AreEqual(true, b);                   // Noncompliant
        }

        public void NullableUnion(BoolOrString? value, CustomUnion? custom, bool b)
        {
            Assert.AreEqual(true, value);       // Compliant, T is inferred as Nullable<BoolOrString>
            Assert.AreEqual(true, b);           // Noncompliant
            Assert.AreEqual(value, false);      // Compliant
            Assert.AreNotEqual(true, value);    // Compliant
            Assert.AreEqual(true, custom);      // Compliant
        }
    }
}
