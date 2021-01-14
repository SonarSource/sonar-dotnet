using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Tests.Diagnostics
{
    class AlwaysSafe
    {
        // System.Xml.XmlNodeReader is always safe
        protected void XmlNodeReader_1()
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = new XmlUrlResolver(); // Noncompliant
            XmlNodeReader reader = new XmlNodeReader(doc);  // safe even though the XmlDocument is not!
        }

        protected void ConfiguredWithSecureResolver()
        {
            XmlDataDocument doc = new XmlDataDocument();
            var resolver = new XmlUrlResolver();
            doc.XmlResolver = new XmlSecureResolver(resolver, ""); // XmlSecureResolver is ok
        }

        // System.Xml.Linq.XElement
        protected void XElementTest()
        {
            XElement xelement = XElement.Load(new MemoryStream()); // always safe
        }
    }

    /// <summary>
    /// There are APIs that become unsafe if an unsafe XmlDocument or XmlTextReader are passed to them.
    /// As we already track these types and raise on them, there's no need to add noise.
    /// </summary>
    class IgnoredToAvoidNoise
    {
        // System.Xml.Xsl.XslCompiledTransform
        protected void XslCompiledTransform(StringWriter stringWriter)
        {
            XmlTextReader reader = new XmlTextReader("resources/"); // safe in .NET version 4.5.2+
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
            // parsing the XML
            XslCompiledTransform transformer = new XslCompiledTransform();
            transformer.Load("resources/test.xsl");
            transformer.Transform(reader, new XsltArgumentList(), stringWriter); // we already raise on XmlTextReader
        }

        // System.Xml.XmlDictionaryReader
        protected void XmlDictionaryReader()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ProhibitDtd = false;                  // Secondary {{This value enables external entities in XML parsing.}}
            settings.DtdProcessing = DtdProcessing.Parse;  // Secondary {{This value enables external entities in XML parsing.}}
            settings.XmlResolver = new XmlUrlResolver();   // Secondary {{This value enables external entities in XML parsing.}}
            XmlReader reader = XmlReader.Create(new MemoryStream(), settings, "resources/"); // Noncompliant
            XDocument xdocument = XDocument.Load(reader); // we already raise on XmlReader
        }

        // System.Xml.Linq.XDocument
        protected void XDocumentTest()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ProhibitDtd = false;                  // Secondary {{This value enables external entities in XML parsing.}}
            settings.DtdProcessing = DtdProcessing.Parse;  // Secondary {{This value enables external entities in XML parsing.}}
            settings.XmlResolver = new XmlUrlResolver();   // Secondary {{This value enables external entities in XML parsing.}}
            XmlReader reader = XmlReader.Create(new MemoryStream(), settings, "resources/"); // Noncompliant
            XDocument xdocument = XDocument.Load(reader); // we already raise for the XmlReader config
        }
    }

}
