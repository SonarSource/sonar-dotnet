using System.IO;
using System.Runtime.Serialization.Formatters.Soap;

namespace NetFramework48
{
    public class SoapFormatterTest
    {
        internal void SoapFormatterDeserialize(Stream stream)
        {
            new SoapFormatter().Deserialize(stream); // Noncompliant (S5773) {{Restrict types of objects allowed to be deserialized.}}

            new SoapFormatter {Binder = new SafeBinder()}.Deserialize(stream);
        }
    }
}
