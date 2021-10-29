using System.Xml;

public record struct S
{
    void XmlTextReader_NewResolver(XmlNameTable table)
    {
        XmlTextReader reader = new("resources/", table);
        (reader.XmlResolver, var x) = (new XmlUrlResolver(), 0); // FN
    }
}
