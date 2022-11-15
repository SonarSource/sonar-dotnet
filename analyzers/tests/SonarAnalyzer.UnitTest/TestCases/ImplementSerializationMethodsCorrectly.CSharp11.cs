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
        static virtual void OnSerializingStaticVirtual(StreamingContext context) { } // Noncompliant, interfaces might need special treatment based on https://github.com/SonarSource/sonar-dotnet/issues/6331

        [OnSerializing]
        static abstract void OnSerializingStaticAbstract(StreamingContext context); // Noncompliant, interfaces might need special treatment based on https://github.com/SonarSource/sonar-dotnet/issues/6331
    }
}
