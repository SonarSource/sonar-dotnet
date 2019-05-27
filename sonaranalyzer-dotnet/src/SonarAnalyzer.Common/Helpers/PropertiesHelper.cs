/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
        internal const string AnalyzeGeneratedCodeCSharp = "sonar.cs.analyzeGeneratedCode";
        internal const string AnalyzeGeneratedCodeVisualBasic = "sonar.vbnet.analyzeGeneratedCode";

        // TODO should make this a singleton class or make sure somehow to not read every time the setting

        public static bool ShouldAnalyzeGeneratedCode(this AnalyzerOptions options, string language)
        {
            return ReadProperty(GetSettings(options), GetProperty());

            string GetProperty() =>
                language == LanguageNames.CSharp
                    ? AnalyzeGeneratedCodeCSharp
                    : AnalyzeGeneratedCodeVisualBasic;
        }

        private static IEnumerable<XElement> GetSettings(AnalyzerOptions options)
        {
            var sonarLintAdditionalFile = options.AdditionalFiles
                .FirstOrDefault(f => ParameterLoader.IsSonarLintXml(f.Path));

            if (sonarLintAdditionalFile == null)
            {
                return new List<XElement>();
            }

            var xml = XDocument.Load(sonarLintAdditionalFile.Path);
            return xml.Descendants("Setting");
        }

        private static bool ReadProperty(IEnumerable<XElement> settings, string propertyName)
        {
            var propertyStringValue = GetPropertyStringValue(propertyName);
            if (propertyStringValue != null &&
                bool.TryParse(propertyStringValue, out var propertyValue))
            {
                return propertyValue;
            }
            return false;

            string GetPropertyStringValue(string propName) =>
                settings
                    .FirstOrDefault(s => s.Element("Key")?.Value == propName)
                    ?.Element("Value").Value;
        }
    }
}
