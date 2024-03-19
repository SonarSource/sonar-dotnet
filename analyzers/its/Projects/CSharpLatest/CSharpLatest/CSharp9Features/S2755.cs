using System.Xml;
using System.Xml.Resolvers;
using System.Xml.XPath;

namespace CSharpLatest.CSharp9Features;

public class S2755
{
    XmlDocument doc = new () { XmlResolver = new XmlPreloadedResolver() };
    XmlTextReader reader = new ("resources/") { XmlResolver = new XmlUrlResolver() };
    XPathDocument xPathDocument = new ("doc.xml");
}
