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
    /// In .NET Framework 4, the constructor is unsafe by default.
    /// </summary>
    class Test
    {
        XmlDocument doc = new XmlDocument(); // Noncompliant

        protected static void XmlDocument_MakeUnsafeWithProperty(XmlUrlResolver xmlUrlResolver)
        {
            var doc = new XmlDocument(); // Noncompliant
            doc.XmlResolver = xmlUrlResolver; // Noncompliant, shown twice
        }

        protected static void XmlDocument_2()
        {
            new XmlDocument(); // Noncompliant
        }

        protected void XmlDocument_SanitizeInsideIf(bool foo)
        {
            var doc = new XmlDocument(); // Noncompliant
            if (foo)
            {
                doc.XmlResolver = null; // conditionally set; not enough
            }
        }

        protected void XmlDocument_InsideIf_SanitizeAfterIf(bool foo)
        {
            var doc = new XmlDocument();
            if (foo)
            {
                doc.XmlResolver = null;
            }
            doc.XmlResolver = null; // this is ok
        }

    }
}
