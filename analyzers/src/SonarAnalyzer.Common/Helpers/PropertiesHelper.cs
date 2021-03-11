/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    internal static class PropertiesHelper
    {
        internal static IEnumerable<XElement> GetSettings(AnalyzerOptions options)
        {
            var sonarLintAdditionalFile = options.AdditionalFiles.FirstOrDefault(f => ParameterLoader.IsSonarLintXml(f.Path));
            if (sonarLintAdditionalFile == null)
            {
                return Enumerable.Empty<XElement>();
            }

            try
            {
                var xml = XDocument.Load(sonarLintAdditionalFile.Path);
                return xml.Descendants("Setting");
            }
            catch
            {
                // ignoring exception as we cannot log it
                return Enumerable.Empty<XElement>();
            }
        }

        internal static bool ReadAnalyzeGeneratedCodeProperty(IEnumerable<XElement> settings, string language) =>
            ReadBooleanProperty(settings, language, "analyzeGeneratedCode");

        internal static bool ReadIgnoreHeaderCommentsProperty(IEnumerable<XElement> settings, string language) =>
            ReadBooleanProperty(settings, language, "ignoreHeaderComments");

        private static bool ReadBooleanProperty(IEnumerable<XElement> settings, string language, string propertySuffix, bool defaultValue = false)
        {
            var propertyLanguage = language == LanguageNames.CSharp ? "cs" : "vbnet";
            var propertyName = $"sonar.{propertyLanguage}.{propertySuffix}";
            return settings.Any()
                && GetPropertyStringValue(propertyName) is { } propertyStringValue
                && bool.TryParse(propertyStringValue, out var propertyValue)
                ? propertyValue
                : defaultValue;

            string GetPropertyStringValue(string propName) =>
                settings.FirstOrDefault(s => s.Element("Key")?.Value == propName)?.Element("Value").Value;
        }
    }
}
