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

        internal void BinderCases(MemoryStream memoryStream)
        {
            var formatter = new BinaryFormatter();

            formatter.Binder = new ValidBinderStatementWithReturnNull();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new ValidBinderExpressionWithNull();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new ValidBinderWithThrowStatement();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new ValidBinderWithThrowExpression();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new InvalidBinder();
            formatter.Deserialize(memoryStream); // Noncompliant: the used binder does not validate the deserialized types

            formatter.Binder = new InvalidBinderExpressionBody();
            formatter.Deserialize(memoryStream); // Noncompliant: the used binder does not validate the deserialized types
        }
    }
}
