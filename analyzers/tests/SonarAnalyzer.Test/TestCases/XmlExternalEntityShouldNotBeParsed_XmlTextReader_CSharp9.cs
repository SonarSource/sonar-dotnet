using System.Xml;

XmlTextReader reader = new("resources/") { XmlResolver = new XmlUrlResolver() }; // Noncompliant

void XmlTextReader_NewResolver_NotTopLevel()
{
    XmlTextReader reader = new("resources/") { XmlResolver = new XmlUrlResolver() }; // Noncompliant
}

void XmlTextReader_NewResolver(XmlNameTable table)
{
    XmlTextReader reader = new ("resources/", table);
    reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
}
