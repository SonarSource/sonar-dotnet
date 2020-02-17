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
            var doc = new XmlDocument(); // Noncompliant
            doc.XmlResolver = xmlUrlResolver; // Noncompliant in all versions - and shown twice
        }

        protected static void XmlDocument_2()
        {
            new XmlDocument(); // Noncompliant before 4.5.2
        }

        protected void XmlDocument_InsideIf(bool foo)
        {
            var doc = new XmlDocument(); // Noncompliant
            if (foo)
            {
                doc.XmlResolver = null; // conditionally set; not enough
            }
        }

        protected void XmlDocument_InsideIf2(bool foo)
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
