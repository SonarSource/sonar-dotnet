using System.Xml;

public record struct S
{
    void XmlTextReader_NewResolver(XmlNameTable table)
    {
        XmlTextReader reader = new("resources/", table);
        (reader.XmlResolver, var x) = (new XmlUrlResolver(), 0); // Noncompliant
    }

    public void SetValueAfterObjectInitialization(XmlNameTable table)
    {
        XmlTextReader reader1 = new("resources/", table) { XmlResolver = new XmlUrlResolver() }; // Compliant, property is set below
        (reader1.XmlResolver, var x1) = (null, 0);

        XmlTextReader reader2 = new("resources/", table) { XmlResolver = new XmlUrlResolver() }; // Noncompliant  
        (reader2.XmlResolver, var x2) = (new XmlUrlResolver(), 0); // Noncompliant
    }
}
