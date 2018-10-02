/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.Common
{
    public class DefaultAnalyzerConfiguration : IAnalyzerConfiguration
    {
        public string ProjectOutputPath { get; private set; }

        public IReadOnlyCollection<string> EnabledRules { get; private set; }

        public bool IsEnabled(string ruleKey) =>
            EnabledRules.Contains(ruleKey);

        public void Read(AnalyzerOptions options)
        {
            var projectOutputAdditionalFile = options.AdditionalFiles
                .FirstOrDefault(UtilityAnalyzerBase.IsProjectOutput);

            var sonarLintAdditionalFile = options.AdditionalFiles
                .FirstOrDefault(f => ParameterLoader.ConfigurationFilePathMatchesExpected(f.Path));

            if (sonarLintAdditionalFile == null ||
                projectOutputAdditionalFile == null)
            {
                return;
            }

            var xml = XDocument.Load(sonarLintAdditionalFile.Path);

            EnabledRules = xml.Descendants("Rule")
                .Select(r => r.Element("Key")?.Value)
                .WhereNotNull()
                .ToHashSet();

            ProjectOutputPath = File.ReadAllLines(projectOutputAdditionalFile.Path)
                .FirstOrDefault(l => !string.IsNullOrEmpty(l));
        }
    }
}
