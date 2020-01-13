/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System;
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

        internal static bool ShouldAnalyzeGeneratedCode(this AnalyzerOptions options, string language)
            => ReadBooleanProperty(GetSettings(options),
                language == LanguageNames.CSharp
                    ? AnalyzeGeneratedCodeCSharp
                    : AnalyzeGeneratedCodeVisualBasic);

        internal static IEnumerable<XElement> GetSettings(AnalyzerOptions options)
        {
            var sonarLintAdditionalFile = options.AdditionalFiles
                .FirstOrDefault(f => ParameterLoader.IsSonarLintXml(f.Path));

            if (sonarLintAdditionalFile == null)
            {
                return Enumerable.Empty<XElement>();
            }

            try
            {
                var xml = XDocument.Load(sonarLintAdditionalFile.Path);
                return xml.Descendants("Setting");
            }
            catch (Exception exception)
            {
                // ignoring exception as we cannot log it
                return Enumerable.Empty<XElement>();
            }
        }

        internal static bool ReadBooleanProperty(IEnumerable<XElement> settings, string propertyName, bool defaultValue = false)
        {
            if (!settings.Any())
            {
                return defaultValue;
            }
            var propertyStringValue = GetPropertyStringValue(propertyName);
            if (propertyStringValue != null &&
                bool.TryParse(propertyStringValue, out var propertyValue))
            {
                return propertyValue;
            }
            return defaultValue;

            string GetPropertyStringValue(string propName) =>
                settings
                    .FirstOrDefault(s => s.Element("Key")?.Value == propName)
                    ?.Element("Value").Value;
        }
    }
}
