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
            return new XmlTextReader(Url); // this is safe in .NET 4.5.2+ by default
        }

        public static XmlTextReader XmlTextReader_SetUnsafeResolver(XmlUrlResolver parameter)
        {
            var reader = new XmlTextReader(Url); // this is safe in .NET 4.5.2+ by default
            reader.XmlResolver = parameter; // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
            return reader;
        }

        public static void XmlTextReader_SetProhibit()
        {
            var reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Prohibit; // ok
        }

        public static void XmlTextReader_SetIgnoreProcessing()
        {
            var reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Ignore; // ok
        }

    }
}
