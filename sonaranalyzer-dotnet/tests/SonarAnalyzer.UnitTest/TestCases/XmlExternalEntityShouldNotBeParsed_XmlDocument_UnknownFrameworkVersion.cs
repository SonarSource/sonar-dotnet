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
    class Test
    {
        protected static void XmlDocument_1(XmlUrlResolver xmlUrlResolver)
        {
            var doc = new XmlDocument();
            doc.XmlResolver = xmlUrlResolver; // Noncompliant in all versions - and shown twice
        }

        protected static void XmlDocument_2()
        {
            new XmlDocument();
        }
    }
}
