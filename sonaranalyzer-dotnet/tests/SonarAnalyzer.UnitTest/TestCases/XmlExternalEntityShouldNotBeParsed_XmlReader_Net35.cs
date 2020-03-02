using System.Xml;

namespace SonarAnalyzer.UnitTest.TestCases
{
    public class XmlExternalEntityShouldNotBeParsed_XmlReader_Net35
    {
        public void XmlReader_EnableProhibitDtdAndSetResolver()
        {
            var settings = new XmlReaderSettings {ProhibitDtd = false, XmlResolver = new XmlUrlResolver()};

            using (XmlReader.Create("uri", settings)) { } // Noncompliant
        }

        public void XmlReader_DisableProhibitDtd()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = false})) { } // Noncompliant
        }

        public void XmlReader_EnableProhibitDtd()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = true})) { } // Compliant
        }

        public void XmlReader_DisableProhibitDtdAndSetXmlResolverToNull()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = false, XmlResolver = null})) { } // Compliant
        }

        public void XmlReader_SecureXmlResolver(XmlSecureResolver secureResolver)
        {
            using (XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = false, XmlResolver = secureResolver})) { } // Compliant
        }

        public void XmlReader_SafeByDefault()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings())) { }
        }
    }
}
