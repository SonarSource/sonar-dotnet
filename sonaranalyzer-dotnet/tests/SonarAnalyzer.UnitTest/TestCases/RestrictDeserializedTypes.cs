using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tests.Diagnostics
{
    internal class Serializer
    {
        internal void Deserialize()
        {
            new BinaryFormatter().Deserialize(new MemoryStream()); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }
    }
}
