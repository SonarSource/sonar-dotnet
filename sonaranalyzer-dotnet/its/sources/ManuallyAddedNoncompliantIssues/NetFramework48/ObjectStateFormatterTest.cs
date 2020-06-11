using System.IO;
using System.Web.UI;

namespace NetFramework48
{
    public class ObjectStateFormatterTest
    {
        internal void ObjectStateFormatterDeserialize(Stream stream) =>
            new ObjectStateFormatter().Deserialize(stream); // Noncompliant (S5773) {{Restrict types of objects allowed to be deserialized.}}
    }
}
