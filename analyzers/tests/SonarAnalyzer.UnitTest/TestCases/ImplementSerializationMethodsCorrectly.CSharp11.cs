using System;
using System.Runtime.Serialization;

namespace Net6Poc.ImplementSerializationMethodsCorrectly
{
    [Serializable]
    internal class TestCases
    {
        [OnSerializing, Generic<int>]
        public void OnSerializing(StreamingContext context) { } // Noncompliant
    }

    public class GenericAttribute<T> : Attribute { }

    interface IMyInterface
    {
        [OnSerializing]
        static virtual void OnSerializingStaticVirtual(StreamingContext context) { } // Compliant, because in an interface

        [OnSerializing]
        static abstract void OnSerializingStaticAbstract(StreamingContext context); // Compliant, because in an interface
    }
}
