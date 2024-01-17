using System.Xml;
using System.Xml.Resolvers;

XmlDocument doc = new();
(doc.XmlResolver, var x) = (new XmlPreloadedResolver(), 0); // Noncompliant

XmlDocument doc2 = new();
(doc2.XmlResolver, var x2) = (null, 0); // Compliant

XmlDocument doc3 = new();
(doc3.XmlResolver, var x3) = ((new XmlPreloadedResolver()), 0); // Noncompliant

public record struct RecordStruct
{
    public void SetValueAfterObjectInitialization()
    {
        XmlDocument doc1 = new() { XmlResolver = new XmlPreloadedResolver() }; // Compliant, property is set below
        (doc1.XmlResolver, var x1) = (null, 0);

        XmlDocument doc2 = new() { XmlResolver = new XmlPreloadedResolver() }; // Noncompliant  
        (doc2.XmlResolver, var x2) = (new XmlPreloadedResolver(), 0); // Noncompliant
    }
}
