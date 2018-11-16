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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Common
{
    public class AnalyzerConfiguration
    {
        private static readonly Lazy<HotspotConfiguration> hotspotConfiguration =
            new Lazy<HotspotConfiguration>(() => new HotspotConfiguration());

        private static readonly Lazy<IAnalyzerConfiguration> alwaysEnabledConfiguration =
            new Lazy<IAnalyzerConfiguration>(() => new AlwaysEnabledConfiguration());

        public static IAnalyzerConfiguration Hotspot =>
            hotspotConfiguration.Value;

        public static IAnalyzerConfiguration AlwaysEnabled =>
            alwaysEnabledConfiguration.Value;

        private class AlwaysEnabledConfiguration : IAnalyzerConfiguration
        {
            public void Initialize(AnalyzerOptions options) { }

            public bool IsEnabled(string ruleKey) => true;
        }

        private class HotspotConfiguration : IAnalyzerConfiguration
        {
            private HashSet<string> enabledRules;

            // This class is a singleton and there could be many rules that check if they are
            // enabled simultaneously and since the XML loading is an IO operation (e.g. slow)
            // we lock until it completes to prevent rules from wrongly deciding that they are
            // disabled while the XML is loaded.
            private static readonly object @lock = new object();

            public bool IsEnabled(string ruleKey) =>
                enabledRules.Contains(ruleKey);

            public void Initialize(AnalyzerOptions options)
            {
                if (enabledRules != null)
                {
                    return;
                }

                lock (@lock)
                {
                    if (enabledRules != null)
                    {
                        return;
                    }

                    enabledRules = new HashSet<string>();

                    var sonarLintAdditionalFile = options.AdditionalFiles
                        .FirstOrDefault(f => ParameterLoader.ConfigurationFilePathMatchesExpected(f.Path));
                    if (sonarLintAdditionalFile != null)
                    {
                        var sonarLintXml = XDocument.Load(sonarLintAdditionalFile.Path);

                        var rulesInXml = sonarLintXml.Descendants("Rule")
                            .Select(r => r.Element("Key")?.Value)
                            .WhereNotNull();

                        enabledRules.UnionWith(rulesInXml);
                    }
                }
            }
        }
    }
}
