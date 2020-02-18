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
    /// Unknown framework behaves like .NET Framework 4.5.2+
    /// </summary>
    class Test
    {
        protected static void XmlDocument_MakeUnsafe(XmlUrlResolver xmlUrlResolver)
        {
            var doc = new XmlDocument();
            doc.XmlResolver = xmlUrlResolver; // Noncompliant in all versions
        }

        protected static void XmlDocument_OnlyConstructor()
        {
            new XmlDocument();
        }
    }
}
