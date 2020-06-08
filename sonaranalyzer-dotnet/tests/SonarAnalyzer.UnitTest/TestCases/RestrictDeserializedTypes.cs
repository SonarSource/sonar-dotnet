using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Web.UI;

namespace Tests.Diagnostics
{
    internal class Serializer
    {
        internal void Deserialize()
        {
            new BinaryFormatter().Deserialize(new MemoryStream()); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
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
    }
}
