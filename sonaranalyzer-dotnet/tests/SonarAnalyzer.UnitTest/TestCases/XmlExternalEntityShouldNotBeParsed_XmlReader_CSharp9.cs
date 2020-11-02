using System.Xml;

XmlReader.Create("uri", new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse, XmlResolver = new XmlUrlResolver() }).Dispose(); // Noncompliant

var settings = new XmlReaderSettings()
{
    DtdProcessing = DtdProcessing.Parse,
    XmlResolver = new XmlUrlResolver()
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
