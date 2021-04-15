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
    /// <summary>
    /// In .NET Framework 4.5.2+, the XmlDocument (and derivates) constructors are safe.
    /// An unsafe XML Resolver must be set in order to make the XmlDocument unsafe.
    /// </summary>
    class NoncompliantTests
    {
        XmlDocument doc = new XmlDocument() { XmlResolver = new XmlPreloadedResolver() }; // Noncompliant {{Disable access to external entities in XML parsing.}}

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

    class CompliantTests_After_Net_4_5_2
    {
        protected void XmlDocument_WithSecureResolver(XmlSecureResolver xmlSecureResolver)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = xmlSecureResolver;
        }

        protected void XmlDocument_WithNullResolver(XmlSecureResolver xmlSecureResolver)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
        }

        protected void XmlDataDocumentTest()
        {
            XmlDataDocument doc = new XmlDataDocument();
        }

        protected void ConfigXmlDocumentTest()
        {
            ConfigXmlDocument doc = new ConfigXmlDocument();
        }

        protected void XmlFileInfoDocumentTest()
        {
            XmlFileInfoDocument doc = new XmlFileInfoDocument();
        }

        protected void XmlTransformableDocumentTest()
        {
            XmlTransformableDocument doc = new XmlTransformableDocument();
        }
    }

    /// <summary>
    ///  These are not testing the APIs per se, but other test combinations
    /// </summary>
    class VariousUnsafeCombinations
    {
        public void InsideTryCatch()
        {
            ConfigXmlDocument doc;
            try
            {
                doc = new ConfigXmlDocument();
                doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
            }
            catch
            {
            }
        }

        public void InsideLoop()
        {
            ConfigXmlDocument doc;
            while (true)
            {
                doc = new ConfigXmlDocument();
                doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
            }
        }

        private void LocalFunction()
        {
            var doc = new ConfigXmlDocument();
            LocalFunction();

            void LocalFunction()
            {
                doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
            }
        }

        delegate XmlUrlResolver TestDelegate();
        private void LambdaFunction()
        {
            TestDelegate resolverFactory = () => new XmlUrlResolver();
            var doc = new XmlDocument();
            doc.XmlResolver = resolverFactory(); // Noncompliant
        }

        private void SetUnsafeResolverFromMethod()
        {
            var doc = new XmlDocument();
            doc.XmlResolver = GetUrlResolver(); // Noncompliant
        }

        private XmlUrlResolver GetUrlResolver() => new XmlUrlResolver();

        private void SetUnsafeResolverFromMethodWithTuple()
        {
            var doc = new XmlDocument();
            var tuple = GetUrlResolverInTuple();
            doc.XmlResolver = tuple.resolver; // Noncompliant
        }

        private (XmlUrlResolver resolver, int i) GetUrlResolverInTuple() => (new XmlUrlResolver(), 1);

        private void PropagateResolverValue(ConfigXmlDocument doc)
        {
            var res1 = new XmlUrlResolver();
            var res2 = res1;
            var res3 = res2;
            doc.XmlResolver = res3; // Noncompliant
        }

        public string XmlProperty
        {
            get
            {
                doc = new ConfigXmlDocument();
                doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
                return "";
            }
        }

        ConfigXmlDocument doc;
        // constructor
        public VariousUnsafeCombinations()
        {
            var doc = new ConfigXmlDocument();
            doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
        }

        // multiple constructor calls for same type
        public void MultipleConstructorCallsForSameType()
        {
            var doc = new ConfigXmlDocument();
            doc = new ConfigXmlDocument();
            doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
        }

        // multiple sets of the property, some sanitize some vulnerable, last sanitize
        public void MultipleSetsLastSanitize()
        {
            var doc = new ConfigXmlDocument();
            doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
            doc.XmlResolver = null;
        }

        // multiple sets of the property, some sanitize some vulnerable, last vulnerable
        public void MultipleSetsLastMakeUnsafe()
        {
            var doc = new ConfigXmlDocument();
            doc.XmlResolver = null;
            doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
        }

        public void MultipleVulnerableApisInSameMethod()
        {
            ConfigXmlDocument doc = new ConfigXmlDocument();
            XmlFileInfoDocument doc2 = new XmlFileInfoDocument();
            doc.XmlResolver = null;
            doc2.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
        }

        public void Caller()
        {
            ConfigXmlDocument doc = new ConfigXmlDocument();
            Callee(doc);
        }

        public void Callee(ConfigXmlDocument doc)
        {
            doc.XmlResolver = new XmlPreloadedResolver(); // Noncompliant
        }

        public void WithInitializeInsideIf()
        {
            XmlDocument doc;
            if ((doc = new XmlDocument()) != null)
            {
                doc.XmlResolver = null; // no DTD resolving
                doc.Load("");
            }
        }
    }
 }
