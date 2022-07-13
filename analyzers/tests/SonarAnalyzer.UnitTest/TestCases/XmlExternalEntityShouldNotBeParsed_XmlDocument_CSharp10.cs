using System.Xml;
using System.Xml.Resolvers;

XmlDocument doc = new();
(doc.XmlResolver, var x) = (new XmlPreloadedResolver(), 0); // Noncompliant

XmlDocument doc2 = new();
(doc2.XmlResolver, var x2) = (null, 0); // Compliant

XmlDocument doc3 = new();
(doc3.XmlResolver, var x3) = ((new XmlPreloadedResolver()), 0); // Noncompliant
