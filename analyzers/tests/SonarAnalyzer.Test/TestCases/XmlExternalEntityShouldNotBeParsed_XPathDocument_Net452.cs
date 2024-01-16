using System;
using System.Xml;
using System.Xml.XPath;

namespace SonarAnalyzer.Test.TestCases
{
    public class XPathDocumentCases
    {
        public void SafeByDefault()
        {
            var xPathDocument = new XPathDocument("doc.xml"); // XPathDocument is secure by default starting with .net 4.5.2
        }

        public void UnsafeXmlReader()
        {
            // Secondary@+2
            // Secondary@+1
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {ProhibitDtd = false, XmlResolver = new XmlUrlResolver()}); // Noncompliant
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

    public class VariousUsages
    {
        XPathDocument document = new XPathDocument("uri");

        public void InsideTryCatch()
        {
            try
            {
                var xPathDocument = new XPathDocument("uri");
            }
            catch
            {
            }
        }

        private void LocalFunction()
        {
            void LocalFunction()
            {
                var xPathDocument = new XPathDocument("uri");
            }
        }

        private void LambdaFunction()
        {
            Func<XPathDocument> documentFactory = () => new XPathDocument("uri");
        }

        private XPathDocument GetDocument() => new XPathDocument("uri");
    }
}
