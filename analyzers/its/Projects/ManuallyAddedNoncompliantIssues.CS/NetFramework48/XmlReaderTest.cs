﻿/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.Xml;

namespace NetFramework48
{
    public class XmlReaderTest
    {
        public void XmlReader_EnableProhibitDtdAndSetResolver()
        {
            var settings = new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse, XmlResolver = new XmlUrlResolver()};

            using (XmlReader.Create("uri", settings)) { } // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
        }

        public void XmlReader_EnableProhibitDtd() =>
            XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse}).Dispose(); // Compliant - XmlResolver is null by default

        public void XmlReader_SafeByDefault()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings())) { }
        }

        public void XmlReader_SafeWhenDtdProcessingIsSetToIgnore()
        {
            using (XmlReader.Create("uri", new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore, XmlResolver = new XmlUrlResolver()})) { }
        }

        public void XmlReader_SecureResolver(XmlSecureResolver secureResolver)
        {
            var settings = new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse, XmlResolver = secureResolver};

            XmlReader.Create("uri", settings).Dispose(); // Compliant - XmlSecureResolver is used
        }
    }
}
