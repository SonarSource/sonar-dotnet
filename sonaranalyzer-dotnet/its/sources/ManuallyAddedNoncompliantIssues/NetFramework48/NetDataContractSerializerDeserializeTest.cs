using System.IO;
using System.Runtime.Serialization;

namespace NetFramework48
{
    public class NetDataContractSerializerDeserializeTest
    {
        internal void NetDataContractSerializerDeserialize(Stream stream)
        {
            new NetDataContractSerializer().Deserialize(stream); // Noncompliant (S5773) {{Restrict types of objects allowed to be deserialized.}}

            new NetDataContractSerializer {Binder = new SafeBinder()}.Deserialize(stream);
        }
    }
}
