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
    /// In .NET Framework 3.5, the constructor is unsafe by default.
    /// </summary>
    class Test
    {
        XmlDocument doc = new XmlDocument(); // Noncompliant

        protected static void XmlDocument_SetUnsafeProperty(XmlUrlResolver xmlUrlResolver)
        {
            var doc = new XmlDocument(); // Noncompliant
            doc.XmlResolver = xmlUrlResolver; // Noncompliant in all versions - and shown twice
        }

        protected static void XmlDocument_OnlyConstructor()
        {
            new XmlDocument(); // Noncompliant before 4.5.2
        }

        protected void XmlDocument_InsideIf_SanitizeInCondition(bool foo)
        {
            var doc = new XmlDocument(); // Noncompliant
            if (foo)
            {
                doc.XmlResolver = null; // conditionally set; not enough
            }
        }

        protected void XmlDocument_InsideIf_SanitizeOutsideCondition(bool foo)
        {
            var doc = new XmlDocument();
            if (foo)
            {
                doc.XmlResolver = null;
            }
            doc.XmlResolver = null; // this is ok
        }

        public void WithInitializeInsideIf(XmlUrlResolver xmlUrlResolver)
        {
            XmlDocument doc;
            if ((doc = new XmlDocument()) != null)
            {
                doc.XmlResolver = null; // no DTD resolving
                doc.Load("");
            }

            if ((doc = new XmlDocument()) != null) // Noncompliant
            {
                doc.XmlResolver = xmlUrlResolver; // Noncompliant
                doc.Load("");
            }

            if ((doc = new XmlDocument()) != null)
            {
                doc.XmlResolver = null; // no DTD resolving
                doc.Load("");
            }
        }
    }
}
