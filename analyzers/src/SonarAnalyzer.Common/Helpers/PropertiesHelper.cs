/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Xml.Linq;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers
{
    internal static class PropertiesHelper
    {
        public static XElement[] ParseXmlSettings(SourceText sourceText)
        {
            try
            {
                return XDocument.Parse(sourceText.ToString()).Descendants("Setting").ToArray();
            }
            catch
            {
                return Array.Empty<XElement>();    // Can not log the exception, so ignore it
            }
        }

        public static bool ReadAnalyzeGeneratedCodeProperty(IEnumerable<XElement> settings, string language) =>
            ReadBooleanProperty(settings, $"sonar.{(language == LanguageNames.CSharp ? "cs" : "vbnet")}.analyzeGeneratedCode");

        public static bool ReadIgnoreHeaderCommentsProperty(IEnumerable<XElement> settings, string language) =>
            ReadBooleanProperty(settings, $"sonar.{(language == LanguageNames.CSharp ? "cs" : "vbnet")}.ignoreHeaderComments");

        public static string[] ReadSourceFileInclusionsProperty(IEnumerable<XElement> settings) =>
            ReadCommaSeparatedArrayProperty(settings, "sonar.inclusions");

        public static string[] ReadSourceFileExclusionsProperty(IEnumerable<XElement> settings) =>
            ReadCommaSeparatedArrayProperty(settings, "sonar.exclusions");

        public static string ReadProjectRootProperty(IEnumerable<XElement> settings) =>
            GetPropertyStringValue(settings, "sonar.projectRoot");

        private static bool ReadBooleanProperty(IEnumerable<XElement> settings, string propertyName, bool defaultValue = false) =>
            settings.Any()
            && GetPropertyStringValue(settings, propertyName) is { } propertyStringValue
            && bool.TryParse(propertyStringValue, out var propertyValue)
                ? propertyValue
                : defaultValue;

        public static string[] ReadCommaSeparatedArrayProperty(IEnumerable<XElement> settings, string propertyName) =>
            settings.Any()
            && GetPropertyStringValue(settings, propertyName) is { Length: > 0 } propertyStringValue
            && propertyStringValue.Split(',') is { } propertyValue
                ? propertyValue
                : Array.Empty<string>();

        private static string GetPropertyStringValue(IEnumerable<XElement> settings, string propName) =>
            settings.FirstOrDefault(s => s.Element("Key")?.Value == propName)?.Element("Value").Value;
    }
}
