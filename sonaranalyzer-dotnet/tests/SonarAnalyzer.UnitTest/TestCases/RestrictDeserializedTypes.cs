using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Web.UI;

namespace Tests.Diagnostics
{
    internal class Serializer
    {
        internal void BinaryFormatterDeserialize(MemoryStream memoryStream)
        {
            new BinaryFormatter().Deserialize(memoryStream); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }

        internal void NetDataContractSerializerDeserialize()
        {
            new NetDataContractSerializer().Deserialize(new MemoryStream()); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }

        internal void SoapFormatterDeserialize()
        {
            new SoapFormatter().Deserialize(new MemoryStream()); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }

        internal void ObjectStateFormatterDeserialize()
        {
            new ObjectStateFormatter().Deserialize(new MemoryStream()); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }

        internal void BinderAsVariable(Stream stream)
        {
            var safeBinder = new SafeBinderStatementWithReturnNull();
            var unsafeBinder = new UnsafeBinder();

            var formatter1 = new BinaryFormatter();
            formatter1.Binder = safeBinder;
            formatter1.Deserialize(stream); // Compliant: safe binder was used

            var formatter2 = new BinaryFormatter();
            formatter2.Binder = unsafeBinder;
            formatter2.Deserialize(stream); // Noncompliant: unsafe binder used
        }

        internal void BinderCases(MemoryStream memoryStream)
        {
            var formatter = new BinaryFormatter();

            formatter.Binder = new SafeBinderStatementWithReturnNull();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new SafeBinderExpressionWithNull();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new SafeBinderWithThrowStatement();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new SafeBinderWithThrowExpression();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new UnsafeBinder();
            formatter.Deserialize(memoryStream); // Noncompliant: the used binder does not validate the deserialized types

            formatter.Binder = new UnsafeBinderExpressionBody();
            formatter.Deserialize(memoryStream); // Noncompliant: the used binder does not validate the deserialized types
        }
    }
}
