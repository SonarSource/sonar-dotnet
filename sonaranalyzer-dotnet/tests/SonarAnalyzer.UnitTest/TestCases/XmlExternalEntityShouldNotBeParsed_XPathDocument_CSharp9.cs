using System.Xml;
using System.Xml.XPath;

XPathDocument xPathDocument1 = new("doc.xml"); // XPathDocument is secure by default starting with .net 4.5.2

XmlReader xmlReader = XmlReader.Create("uri", new() { ProhibitDtd = false, XmlResolver = new XmlUrlResolver() }); // Compliant - FN
XPathDocument xPathDocument2 = new(xmlReader); // Compliant - we already raise the warning for the reader

XmlReader xmlReaderWithIgnore = XmlReader.Create("uri", new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
XPathDocument xPathDocument3 = new(xmlReaderWithIgnore);
