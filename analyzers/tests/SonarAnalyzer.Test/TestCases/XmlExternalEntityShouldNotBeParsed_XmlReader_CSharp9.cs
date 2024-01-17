using System.IO;
using System.Xml;
using System.Xml.Linq;

// Secondary@+2
// Secondary@+1
XmlReader.Create("uri", new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse, XmlResolver = new XmlUrlResolver() }).Dispose(); // Noncompliant

var settings = new XmlReaderSettings()
{
    DtdProcessing = DtdProcessing.Parse, // Secondary
    XmlResolver = new XmlUrlResolver() // Secondary
};

XmlReader.Create("uri", settings).Dispose(); // Noncompliant

XmlReaderSettings safeSettings = new();
XmlReaderSettings unsafeSettings = new();

safeSettings.DtdProcessing = DtdProcessing.Parse;
unsafeSettings.DtdProcessing = DtdProcessing.Parse;

safeSettings.XmlResolver = null;
unsafeSettings.XmlResolver = new XmlUrlResolver();

XmlReader.Create("uri", safeSettings).Dispose(); // Compliant
XmlReader.Create("uri", unsafeSettings).Dispose(); // Compliant - FN

class Foo
{
    void Bar()
    {
        XmlReaderSettings settings = new ();
        settings.ProhibitDtd = false;                  // Secondary {{This value enables external entities in XML parsing.}}
        settings.DtdProcessing = DtdProcessing.Parse;  // Secondary {{This value enables external entities in XML parsing.}}
        settings.XmlResolver = new XmlUrlResolver();   // Secondary {{This value enables external entities in XML parsing.}}
        XmlReader reader = XmlReader.Create(new MemoryStream(), settings, "resources/"); // Noncompliant
        XDocument xdocument = XDocument.Load(reader); // we already raise on XmlReader
    }
}
