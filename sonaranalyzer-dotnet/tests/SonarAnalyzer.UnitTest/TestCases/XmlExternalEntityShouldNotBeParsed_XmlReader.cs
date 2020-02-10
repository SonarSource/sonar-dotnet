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
    class NoncompliantTests_After_Net_4_5_2
    {
        // System.Xml.XmlReader
        protected void XmlReader_WithMemoryStream()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = new XmlUrlResolver();
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // Noncompliant
        }

        protected void XmlReader_WithTextReader()
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Parse,
                XmlResolver = new XmlUrlResolver()
            };
            XmlReader.Create(new StringReader(""), settings); // Noncompliant
        }

        protected void XmlReader_WithString()
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Parse,
                XmlResolver = new XmlUrlResolver()
            };
            XmlReader.Create("", settings); // Noncompliant
        }

        protected void XmlReader_WithXmlReader(XmlValidatingReader xmlReader)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Parse,
                XmlResolver = new XmlUrlResolver()
            };
            XmlReader.Create(xmlReader, settings); // Noncompliant
        }
    }

    class CompliantTests_After_Net_4_5_2
    {
        // System.Xml.XmlReader
        protected void XmlReader_WithMemoryStream()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse; // still ok in 4.5.2+
            XmlReader.Create(new MemoryStream(), settings, "resources/");
        }
    }
}
