using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Resolvers;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.Web.XmlTransform;

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
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = new XmlUrlResolver();
            XmlReader reader = XmlReader.Create(new MemoryStream(), settings, "resources/"); // Noncompliant
            XDocument xdocument = XDocument.Load(reader); // we already raise on XmlReader
        }

        // System.Xml.Linq.XDocument
        protected void XDocumentTest()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = new XmlUrlResolver();
            XmlReader reader = XmlReader.Create(new MemoryStream(), settings, "resources/");
            XDocument xdocument = XDocument.Load(reader); // we already raise for the XmlReader config
        }
    }

}
