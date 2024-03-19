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
    public static class XmlDocumentTest
    {
        public static void Main(string[] args)
        {
            // dummy
        }

        public static XmlDocument XmlDocument_OnlyConstructor()
        {
            return new XmlDocument();
        }

        public static void XmlDocument_SetUnsafeResolver(XmlUrlResolver xmlUrlResolver)
        {
            var doc = new XmlDocument();
            doc.XmlResolver = xmlUrlResolver; // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
        }

        public static void XmlDocument_SetSecureResolver(XmlSecureResolver xmlSecureResolver)
        {
            var doc = new XmlDocument();
            doc.XmlResolver = xmlSecureResolver;
        }

        public static void XmlDocument_SetNullResolver()
        {
            var doc = new XmlDocument();
            doc.XmlResolver = null;
        }
    }
}
