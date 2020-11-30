using System.Xml;
using System.Xml.XPath;

namespace NetFramework35
{
    public class XPathDocumentTest
    {
        public void UnsafeByDefault()
        {
            new XPathDocument("doc.xml"); // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
        }

        public void UnsafeXmlReader()
        {
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = false}); // Noncompliant
            new XPathDocument(xmlReader);
        }

        public void SafeXmlReader()
        {
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = true});
            new XPathDocument(xmlReader);
        }

        public void XmlReaderAsParameter(XmlReader xmlReader)
        {
            new XPathDocument(xmlReader);
        }
    }
}
