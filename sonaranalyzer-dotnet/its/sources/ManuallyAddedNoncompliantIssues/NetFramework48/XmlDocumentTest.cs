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
    class XmlDocumentTest
    {
        public static void Main(string[] args)
        {
            // dummy
        }

        protected static void XmlDocument_1(XmlUrlResolver xmlUrlResolver)
        {
            var doc = new XmlDocument();
            doc.XmlResolver = xmlUrlResolver; // Noncompliant in all versions
        }

        protected static void XmlDocument_2()
        {
            new XmlDocument(); // Noncompliant before 4.5.2
        }

        // System.Xml.XmlDocument
        protected static void XmlDocument_3(XmlSecureResolver xmlSecureResolver)
        {
            var doc = new XmlDocument();
            doc.XmlResolver = xmlSecureResolver;
        }

        protected static void XmlDocument_1()
        {
            var doc = new XmlDocument();
            doc.XmlResolver = null;
        }
    }
}
