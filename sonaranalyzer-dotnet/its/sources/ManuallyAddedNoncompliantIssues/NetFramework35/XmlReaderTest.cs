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

namespace NetFramework4
{
    public class XmlReaderTest
    {
        private const string BaseUri = "resources/";
        public static void Main(string[] args)
        {
            // dummy
        }

        protected static XmlReader XmlReader_1()
        {
            var settings = new XmlReaderSettings();
            settings.ProhibitDtd = false;
            return XmlReader.Create(new MemoryStream(), settings, BaseUri); // Noncompliant in 3.5
        }

        protected static void XmlReader_2()
        {
            var settings = new XmlReaderSettings();
            XmlReader.Create(new MemoryStream(), new XmlReaderSettings(), BaseUri); // ok
        }

        protected static XmlReader XmlReader_3()
        {
            var settings = new XmlReaderSettings();
            settings.ProhibitDtd = true;
            return XmlReader.Create(new MemoryStream(), settings, BaseUri); // ok
        }

    }
}
