using System.Xml;
using System.Xml.XPath;

namespace NetFramework4
{
    public class XPathDocumentTest
    {
        public void UnsafeByDefault()
        {
            new XPathDocument("doc.xml"); // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
        }

        public void UnsafeXmlReader()
        {
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse}); // Noncompliant
            new XPathDocument(xmlReader);
        }

        public void SafeXmlReader()
        {
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore});
            new XPathDocument(xmlReader);
        }

        public void XmlReaderAsParameter(XmlReader xmlReader)
        {
            new XPathDocument(xmlReader);
        }
    }
}
