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

    class CompliantTests_After_Net_4_5_2
    {
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
    }

 }
