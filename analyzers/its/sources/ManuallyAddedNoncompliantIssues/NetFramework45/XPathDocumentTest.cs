using System.Xml;
using System.Xml.XPath;

namespace NetFramework45
{
    public class XPathDocumentTest
    {
        public void UnsafeByDefault()
        {
            _ = new XPathDocument("doc.xml"); // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
        }

        public void UnsafeXmlReader()
        {
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse}); // Noncompliant
            _ = new XPathDocument(xmlReader);
        }

        public void SafeXmlReader()
        {
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore});
            _ = new XPathDocument(xmlReader);
        }

        public void XmlReaderAsParameter(XmlReader xmlReader)
        {
            _ = new XPathDocument(xmlReader);
        }
    }
}
