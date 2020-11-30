using System.Xml;

namespace NetFramework35
{
    public class XmlReaderTest
    {
        public void XmlReader_EnableProhibitDtdAndSetResolver()
        {
            var settings = new XmlReaderSettings {ProhibitDtd = false, XmlResolver = new XmlUrlResolver()};

            using (XmlReader.Create("uri", settings)) { } // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
        }

        public void XmlReader_DisableProhibitDtd()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = false})) { } // Noncompliant
        }

        public void XmlReader_EnableProhibitDtd()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = true})) { } // Compliant
        }

        public void XmlReader_SafeByDefault()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings())) { }
        }
    }
}
