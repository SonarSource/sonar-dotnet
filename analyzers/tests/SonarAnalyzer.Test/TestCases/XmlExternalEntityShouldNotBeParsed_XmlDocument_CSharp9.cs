using System.Xml;
using System.Xml.Resolvers;

XmlDocument xmlDocument = new(); // Compliant - default constructor is safe in .Net 5

XmlDocument doc1 = new() { XmlResolver = new XmlPreloadedResolver() }; // Noncompliant

XmlDocument doc2 = new();
doc2.XmlResolver = new XmlPreloadedResolver(); // Noncompliant

void XmlDocumentTest(XmlUrlResolver xmlUrlResolver)
{
    XmlDocument doc = new() // Noncompliant
    {
        XmlResolver = xmlUrlResolver
    };
}
