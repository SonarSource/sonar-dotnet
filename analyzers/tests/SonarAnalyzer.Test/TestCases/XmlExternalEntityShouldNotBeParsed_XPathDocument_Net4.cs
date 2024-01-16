using System.Xml;
using System.Xml.XPath;

namespace SonarAnalyzer.Test.TestCases
{
    public class XPathDocumentCases
    {
        public void UnsafeByDefault()
        {
            var xPathDocument = new XPathDocument("doc.xml"); // Noncompliant - XPathDocument is unsafe by default before .net 4.5.2
        }

        public void UnsafeXmlReader()
        {
            // Secondary@+1
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = false});  // Noncompliant
            var xPathDocument = new XPathDocument(xmlReader); // Compliant - we already raise the warning for the reader
        }

        public void SafeXmlReader()
        {
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore});
            var xPathDocument = new XPathDocument(xmlReader);
        }

        public void XmlReaderAsParameter(XmlReader xmlReader)
        {
            var xPathDocument = new XPathDocument(xmlReader);
        }
    }
}
