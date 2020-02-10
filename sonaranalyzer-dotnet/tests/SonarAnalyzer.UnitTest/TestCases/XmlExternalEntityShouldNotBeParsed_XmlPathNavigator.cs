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
    class NoncompliantTests_Before_Net_4_5_2
    {
        // System.Xml.XPath.XPathNavigator
        protected void XPathNavigator_1()
        {
            XPathDocument doc = new XPathDocument(new MemoryStream(Encoding.ASCII.GetBytes("")));
            XPathNavigator nav = doc.CreateNavigator(); // FN
        }

        protected void XPathNavigator_2()
        {
            XPathDocument doc = new XPathDocument(""); // FN
            XPathNavigator nav = doc.CreateNavigator();
        }
     }

    class CompliantTests_After_Net_4_5_2
    {
        // System.Xml.XPath.XPathNavigator
        protected void XPathNavigator_1()
        {
            XPathDocument doc = new XPathDocument(new MemoryStream(Encoding.ASCII.GetBytes("")));
            XPathNavigator nav = doc.CreateNavigator(); // FN
        }

        protected void XPathNavigator_2()
        {
            XPathDocument doc = new XPathDocument(""); // FN
            XPathNavigator nav = doc.CreateNavigator();
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
    }
}
