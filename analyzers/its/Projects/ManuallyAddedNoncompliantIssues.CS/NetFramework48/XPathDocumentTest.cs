﻿/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.Xml;
using System.Xml.XPath;

namespace NetFramework48
{
    public class XPathDocumentTest
    {
        public void UnsafeByDefault()
        {
            _ = new XPathDocument("doc.xml"); // Compliant - safe by default starting with .Net 4.5.2
        }

        public void UnsafeXmlReader()
        {
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse, XmlResolver = new XmlUrlResolver()}); // Noncompliant (S2755)
            _ = new XPathDocument(xmlReader);
        }

        public void SafeXmlReader()
        {
            var xmlReader = XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore});
            _ = new XPathDocument(xmlReader);
        }

        public void XmlReaderAsParameter(XmlReader xmlReader)
        {
            _ = new XPathDocument(xmlReader);
        }
    }
}
