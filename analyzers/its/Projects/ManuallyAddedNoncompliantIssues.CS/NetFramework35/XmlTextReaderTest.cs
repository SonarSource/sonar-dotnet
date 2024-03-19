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

        public static void Main(string[] args)
        {
            // dummy
        }

        public static XmlTextReader XmlTextReader_OnlyConstructor()
        {
            return new XmlTextReader(Url); // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
        }

        public static void XmlTextReader_SetProhibit()
        {
            var reader = new XmlTextReader(Url);
            reader.ProhibitDtd = true; // ok
        }

        public static void XmlTextReader_SetVulnerableProperty()
        {
            var reader = new XmlTextReader(Url); // Noncompliant in 3.5
            reader.ProhibitDtd = false; // Noncompliant
        }
    }
}
