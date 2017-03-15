/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Utilities;
using System.Text;

namespace SonarAnalyzer.Runner
{
    public class Configuration
    {
        private readonly ImmutableArray<DiagnosticAnalyzer> analyzers;
        private readonly AnalyzerLanguage language;

        public string SonarLintAdditionalPath { get; private set; }
        public string ProtoFolderAdditionalPath { get; private set; }
        public Encoding Encoding { get; private set; }

        public bool IgnoreHeaderComments { get; }
        public IImmutableList<string> Files { get; }
        public IImmutableSet<string> AnalyzerIds { get; }

        public Configuration(string sonarLintFilePath, string protoFolderFilePath, AnalyzerLanguage language)
        {
            if (!ParameterLoader.ConfigurationFilePathMatchesExpected(sonarLintFilePath))
            {
                throw new ArgumentException(
                    $"Input configuration doesn't match expected file name: '{ParameterLoader.ParameterConfigurationFileName}'",
                    nameof(sonarLintFilePath));
            }

            this.language = language;
            ProtoFolderAdditionalPath = protoFolderFilePath;

            SonarLintAdditionalPath = sonarLintFilePath;
            analyzers = ImmutableArray.Create(GetAnalyzers(language).ToArray());

            var xml = XDocument.Load(sonarLintFilePath);
            var settings = ParseSettings(xml);
            IgnoreHeaderComments = "true".Equals(settings[$"sonar.{language}.ignoreHeaderComments"], StringComparison.OrdinalIgnoreCase);

            Files = xml.Descendants("File").Select(e => e.Value).ToImmutableList();

            AnalyzerIds = xml.Descendants("Rule").Select(e => e.Elements("Key").Single().Value).ToImmutableHashSet();

            if (settings.ContainsKey("sonar.sourceEncoding"))
            {
                try
                {
                    var encodingName = settings["sonar.sourceEncoding"];
                    Encoding = Encoding.GetEncoding(encodingName);
                }
                catch (ArgumentException)
                {
                    Program.Write($"Could not get encoding '{settings["sonar.sourceEncoding"]}'");
                }
            }
        }

        private static ImmutableDictionary<string, string> ParseSettings(XContainer xml)
        {
            return xml
                .Descendants("Setting")
                .Select(e =>
                {
                    var keyElement = e.Element("Key");
                    var valueElement = e.Element("Value");
                    if (valueElement != null && keyElement != null)
                    {
                        return new
                        {
                            Key = keyElement.Value,
                            Value = valueElement.Value
                        };
                    }
                    return null;
                })
                .Where(e => e != null)
                .ToImmutableDictionary(e => e.Key, e => e.Value);
        }

        public ImmutableArray<DiagnosticAnalyzer> GetAnalyzers()
        {
            var builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();

            foreach (var analyzer in analyzers
                .Where(analyzer => AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id)))
            {
                builder.Add(analyzer);
            }

            return builder.ToImmutable();
        }

        public ImmutableArray<DiagnosticAnalyzer> GetUtilityAnalyzers()
        {
            var builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();

            var utilityAnalyzerTypes = RuleFinder.GetUtilityAnalyzerTypes(language)
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var analyzer in utilityAnalyzerTypes
                    .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)))
            {
                builder.Add(analyzer);
            }

            return builder.ToImmutable();
        }

        #region Discover analyzers

        public static IEnumerable<DiagnosticAnalyzer> GetAnalyzers(AnalyzerLanguage language)
        {
            return
                new RuleFinder().GetAnalyzerTypes(language)
                    .Select(type => (DiagnosticAnalyzer) Activator.CreateInstance(type));
        }

        #endregion
    }
}
