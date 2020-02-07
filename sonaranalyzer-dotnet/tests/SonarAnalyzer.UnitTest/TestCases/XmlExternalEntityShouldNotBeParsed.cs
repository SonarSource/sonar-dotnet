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
    public class Program
    {
        public static void Main(string[] args)
        {

        }
    }

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

        // System.Xml.XmlTextReader
        protected void XmlTextReader_1(XmlUrlResolver parameter)
        {
            XmlTextReader reader = new XmlTextReader("resources/"); // this is safe in .NET 4.5.2+
            // this is not used, but the rule is not advanced enough to figure it out
            XmlUrlResolver res = new XmlUrlResolver();
            // we don't really know it's null or not
            reader.XmlResolver = parameter; // Noncompliant
        }

        protected void XmlTextReader_2(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        protected void XmlTextReader_3(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            XmlTextReader reader = new XmlTextReader(xmlFragment, fragType, context);
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        protected void XmlTextReader_4(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table); // Noncompliant
            reader.DtdProcessing = DtdProcessing.Parse;
        }

        // System.Xml.XmlDocument
        protected void XmlDocumentTest(XmlUrlResolver xmlUrlResolver)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = xmlUrlResolver; // Noncompliant
        }

        // System.Xml.XmlDataDocument
        protected void XmlDataDocumentTest()
        {
            XmlDataDocument doc = new XmlDataDocument();
            doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
        }

        // System.Configuration.ConfigXmlDocument
        protected void ConfigXmlDocumentTest()
        {
            ConfigXmlDocument doc = new ConfigXmlDocument();
            doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
        }

        // Microsoft.Web.XmlTransform.XmlFileInfoDocument
        protected void XmlFileInfoDocumentTest()
        {
            XmlFileInfoDocument doc = new XmlFileInfoDocument();
            doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
        }

        // Microsoft.Web.XmlTransform.XmlTransformableDocument
        protected void XmlTransformableDocumentTest()
        {
            XmlTransformableDocument doc = new XmlTransformableDocument();
            doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
        }
    }

    class NoncompliantTests_Before_Net_4_5_2
    {
        // System.Xml.XPath.XPathNavigator
        protected void XPathNavigator_1()
        {
            XPathDocument doc = new XPathDocument(new MemoryStream(Encoding.ASCII.GetBytes("")));
            XPathNavigator nav = doc.CreateNavigator(); // Noncompliant
        }

        protected void XPathNavigator_2()
        {
            XPathDocument doc = new XPathDocument(""); // Noncompliant
            XPathNavigator nav = doc.CreateNavigator();
        }

        // System.Xml.XmlReader
        protected void XmlReader_WithMemoryStream()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // Noncompliant
        }

        protected void XmlReader_3()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = null;
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // Noncompliant
        }

        // System.Xml.XmlTextReader
        protected void XmlTextReader_1()
        {
            XmlTextReader reader = new XmlTextReader("resources/"); // Noncompliant
        }

        // System.Xml.XmlDocument
        protected void XmlDocument_1()
        {
            XmlDocument doc = new XmlDocument(); // Noncompliant
        }

        // System.Xml.XmlDataDocument
        protected void XmlDataDocumentTest()
        {
            XmlDataDocument doc = new XmlDataDocument(); // Noncompliant
        }

        // System.Configuration.ConfigXmlDocument
        protected void ConfigXmlDocumentTest()
        {
            ConfigXmlDocument doc = new ConfigXmlDocument(); // Noncompliant
        }

        // Microsoft.Web.XmlTransform.XmlFileInfoDocument
        protected void XmlFileInfoDocumentTest()
        {
            XmlFileInfoDocument doc = new XmlFileInfoDocument(); // Noncompliant
        }

        // Microsoft.Web.XmlTransform.XmlTransformableDocument
        protected void XmlTransformableDocumentTest()
        {
            XmlTransformableDocument doc = new XmlTransformableDocument(); // Noncompliant
        }
    }


    /// <summary>
    ///  These are not testing the APIs per se, but other test combinations
    /// </summary>
    class VariousUnsafeCombinations
    {

        protected void MoreInstructions()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;   // unsafe!
            XmlUrlResolver res = new XmlUrlResolver();
            res.ResolveUri(new Uri(Environment.CurrentDirectory), "resources/xxetestuser.xml");
            settings.XmlResolver = res;
            XmlReader.Create(new StringReader(""), settings); // Noncompliant
        }


        public void InsideTryCatch()
        {

        }

        public void InsideLoop()
        {

        }

        private void PrivateMethod()
        {

        }

        private void LocalFunction()
        {
            LocalFunction();

            string LocalFunction()
            {
                return "";
            }
        }

        public string XmlProperty
        {
            get
            {
                return "";
            }
        }

        // constructor
        public VariousUnsafeCombinations()
        {

        }

        // multiple constructor calls for same type
        public void MultipleConstructorCallsForSameType()
        {

        }

        // multiple sets of the property, some sanitize some vulnerable, last sanitize
        public void MultipleSetsLastSanitize()
        {

        }

        // multiple sets of the property, some sanitize some vulnerable, last vulnerable
        public void MultipleSetsLastMakeUnsafe()
        {

        }

        public void MultipleVulnerableApisInSameMethod()
        {

        }

        public void Caller()
        {

        }

        public void Callee()
        {

        }
    }

    class CompliantTests_After_Net_4_5_2
    {
        // System.Xml.XPath.XPathNavigator
        protected void XPathNavigator_1()
        {
            XPathDocument doc = new XPathDocument(new MemoryStream(Encoding.ASCII.GetBytes("")));
            XPathNavigator nav = doc.CreateNavigator(); // Noncompliant
        }

        protected void XPathNavigator_2()
        {
            XPathDocument doc = new XPathDocument(""); // Noncompliant
            XPathNavigator nav = doc.CreateNavigator();
        }

        // System.Xml.XmlReader
        protected void XmlReader_WithMemoryStream()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse; // still ok in 4.5.2+
            XmlReader.Create(new MemoryStream(), settings, "resources/");
        }

        // System.Xml.XmlTextReader
        protected void XmlTextReader_2(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
        }

        protected void XmlTextReader_3(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            XmlTextReader reader = new XmlTextReader(xmlFragment, fragType, context);
            reader.XmlResolver = new XmlSecureResolver(null, "");
        }

        protected void XmlTextReader_4(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.DtdProcessing = DtdProcessing.Prohibit;
        }

        protected void XmlTextReader_5(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.DtdProcessing = DtdProcessing.Ignore;
        }

        // System.Xml.XmlDocument
        protected void XmlDocument_1(XmlSecureResolver xmlSecureResolver)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = xmlSecureResolver;
        }

        protected void XmlDocument_2(XmlSecureResolver xmlSecureResolver)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
        }

        // System.Xml.XmlDataDocument
        protected void XmlDataDocumentTest()
        {
            XmlDataDocument doc = new XmlDataDocument();
        }

        // System.Configuration.ConfigXmlDocument
        protected void ConfigXmlDocumentTest()
        {
            ConfigXmlDocument doc = new ConfigXmlDocument();
        }

        // Microsoft.Web.XmlTransform.XmlFileInfoDocument
        protected void XmlFileInfoDocumentTest()
        {
            XmlFileInfoDocument doc = new XmlFileInfoDocument();
        }

        // Microsoft.Web.XmlTransform.XmlTransformableDocument
        protected void XmlTransformableDocumentTest()
        {
            XmlTransformableDocument doc = new XmlTransformableDocument();
        }
    }

    class CompliantTests_Before_Net_4_5_2
    {
        // System.Xml.XPath.XPathNavigator
        protected void XPathNavigator_1()
        {
            XPathDocument doc = new XPathDocument(new MemoryStream(Encoding.ASCII.GetBytes("")));
            XPathNavigator nav = doc.CreateNavigator(); // ok
        }

        // System.Xml.XmlReader
        protected void XmlReader_1()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;
            settings.XmlResolver = new XmlUrlResolver();
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // ok
        }

        protected void XmlReader_2()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            settings.XmlResolver = new XmlUrlResolver();
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // ok
        }

        // System.Xml.XmlTextReader
        protected void XmlTextReader_1()
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            reader.XmlResolver = null;
        }

        // System.Xml.XmlDocument
        protected void XmlDocument_1()
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
        }

        // System.Xml.XmlDataDocument
        protected void XmlDataDocumentTest()
        {
            XmlDataDocument doc = new XmlDataDocument();
            doc.XmlResolver = null;
        }

        // System.Configuration.ConfigXmlDocument
        protected void ConfigXmlDocumentTest()
        {
            ConfigXmlDocument doc = new ConfigXmlDocument();
            doc.XmlResolver = null;
        }

        // Microsoft.Web.XmlTransform.XmlFileInfoDocument
        protected void XmlFileInfoDocumentTest()
        {
            XmlFileInfoDocument doc = new XmlFileInfoDocument();
            doc.XmlResolver = null;
        }

        // Microsoft.Web.XmlTransform.XmlTransformableDocument
        protected void XmlTransformableDocumentTest()
        {
            XmlTransformableDocument doc = new XmlTransformableDocument();
            doc.XmlResolver = null;
        }
    }

    class AlwaysSafe
    {
        // System.Xml.XmlNodeReader is always safe
        protected void XmlNodeReader_1()
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = new XmlUrlResolver();
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
