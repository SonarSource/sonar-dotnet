/*
 * SonarQube, open source software quality management tool.
 * Copyright (C) 2008-2013 SonarSource
 * mailto:contact AT sonarsource DOT com
 *
 * SonarQube is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * SonarQube is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
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
            return new XmlDocument(); // Noncompliant (S2755) {{Disable access to external entities in XML parsing.}}
        }

        public static void XmlDocument_ConstructorWithVulnerableProperty(XmlUrlResolver xmlUrlResolver)
        {
            var doc = new XmlDocument(); // Noncompliant
            doc.XmlResolver = xmlUrlResolver; // Noncompliant in all versions - and shown twice
        }

        public static void XmlDocument_SetSafeResolver(XmlSecureResolver xmlSecureResolver)
        {
            var doc = new XmlDocument();
            doc.XmlResolver = xmlSecureResolver;
        }

        public static void XmlDocument_SetNull()
        {
            var doc = new XmlDocument();
            doc.XmlResolver = null;
        }
    }
}
