/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.Xml;

namespace Test
{
    public static class XmlTextReaderTest
    {
        private const string Url = "resources/";

        public static XmlTextReader XmlTextReader_OnlyConstructor()
        {
            var x = new XmlTextReader(Url); // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
            return x;
        }

        public static void XmlTextReader_SetSanitizeWithNull()
        {
            var reader = new XmlTextReader("resources/");
            reader.XmlResolver = null;
        }

        public static XmlTextReader XmlTextReader_SetVulnerableProperty(XmlUrlResolver parameter)
        {
            var reader = new XmlTextReader(Url); // Noncompliant
            reader.XmlResolver = parameter; // Noncompliant
            return reader;
        }

        public static void XmlTextReader_SetProhibit()
        {
            var reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Prohibit; // ok
        }

        public static void XmlTextReader_SetIgnore()
        {
            var reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Ignore; // ok
        }

    }
}
