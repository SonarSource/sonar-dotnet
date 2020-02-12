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

using System.IO;
using System.Xml;

namespace Test
{
    class XmlReaderTest
    {
        private const string BaseUri = "resources/";

        protected static XmlReader XmlReader_1()
        {
            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse; // by default in 4.0+ , it is Prohibit
            return XmlReader.Create(new MemoryStream(), settings, BaseUri); // Noncompliant before and after 4.5.2
        }

        protected static void XmlReader_2()
        {
            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = new XmlUrlResolver(); // by default in 4.5.2+, it is null
            XmlReader.Create(new MemoryStream(), settings, BaseUri); // Noncompliant after 4.5.2
        }
        protected void XmlReader_3()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = null;
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // Noncompliant
        }

        protected void XmlReader_4()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;
            settings.XmlResolver = new XmlUrlResolver();
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // ok
        }
        protected void XmlReader_5()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            settings.XmlResolver = new XmlUrlResolver();
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // ok
        }

        protected static void XmlReader_6()
        {
            XmlReader.Create(new MemoryStream(), new XmlReaderSettings(), BaseUri); // ok in all versions
        }
    }
}
