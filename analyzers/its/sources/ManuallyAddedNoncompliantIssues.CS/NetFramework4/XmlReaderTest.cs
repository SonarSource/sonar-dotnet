using System.Xml;

namespace NetFramework4
{
    public class XmlReaderTest
    {
        public void XmlReader_EnableProhibitDtdAndSetResolver()
        {
            var settings = new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse, XmlResolver = new XmlUrlResolver()};

            using (XmlReader.Create("uri", settings)) { } // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
        }

        public void XmlReader_EnableProhibitDtd()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse})) { } // Noncompliant
        }

        public void XmlReader_SafeByDefault()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings())) { }
        }

        public void XmlReader_SafeWhenDtdProcessingIsSetToIgnore()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore})) { }
        }
    }
}
