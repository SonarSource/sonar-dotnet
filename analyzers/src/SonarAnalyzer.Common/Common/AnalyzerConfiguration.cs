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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Common
{
    public class AnalyzerConfiguration
    {
        /// <summary>
        /// Hotspot rules are not configurable (from ruleset) to prevent them from appearing in SonarLint.
        /// They are enabled by default and we check if SonarLint.xml contains the rule key on CompilationStart
        /// to determine whether to run the analysis or not.
        /// SonarLint.xml is added by both SonarScanner for .NET and by SonarLint, however there are differences:
        /// - SonarLint only uses it to pass parameters to rules
        /// - SonarScanner uses it to pass parameters and to enable security hotspots (which should only run in batch mode)
        /// </summary>
        public static IAnalyzerConfiguration Hotspot => new HotspotConfiguration(new RuleLoader());

        public static IAnalyzerConfiguration AlwaysEnabled => new AlwaysEnabledConfiguration(false);

        public static IAnalyzerConfiguration AlwaysEnabledWithSonarCfg => new AlwaysEnabledConfiguration(true);

        private class AlwaysEnabledConfiguration : IAnalyzerConfiguration
        {
            public bool ForceSonarCfg { get; }

            public AlwaysEnabledConfiguration(bool forceSonarCfg) =>
                ForceSonarCfg = forceSonarCfg;

            public void Initialize(AnalyzerOptions options)
            {
                // Ignore options because we always return true for IsEnabled
            }

            public bool IsEnabled(string ruleKey) => true;
        }

        /// <summary>
        /// Singleton to hold the configuration for hotspot rules.
        /// </summary>
        internal /* for tests */ class HotspotConfiguration : IAnalyzerConfiguration
        {
            private readonly IRuleLoader ruleLoader;

            // Hotspot configuration is cached at the assembly level and the MsBuild process
            // can reuse the already loaded assembly when multiple projects are analyzed one after the other.
            // Due to this we have to check the current configuration path to see if a reload is needed.
            private string loadedSonarLintXmlPath;
            private bool isInitialized;
            private ISet<string> enabledRules = new HashSet<string>();

            /// <summary>
            /// There could be many rules that check if they are enabled simultaneously and since
            /// the XML loading is an IO operation (e.g. slow) we lock until it completes to prevent
            /// rules from wrongly deciding that they are disabled while the XML is loaded.
            /// </summary>
            private static readonly object IsInitializedGate = new object();

            public bool ForceSonarCfg => false;

            public HotspotConfiguration(IRuleLoader ruleLoader) => this.ruleLoader = ruleLoader;

            public bool IsEnabled(string ruleKey) =>
                // Initialize can be called multiple times, and the `enabledRules` can change between initializations,
                // so here we have a race condition when a second initialization happens.
                // We would need to lock here as well, or even better, make IsEnabled and Initialize atomic.
                // https://github.com/SonarSource/sonar-dotnet/issues/4139
                isInitialized
                    ? enabledRules.Contains(ruleKey)
                    : throw new InvalidOperationException("Call Initialize() before calling IsEnabled().");

            public void Initialize(AnalyzerOptions options)
            {
                var currentSonarLintXmlFile = GetSonarLintXmlFile(options);
                if (isInitialized && loadedSonarLintXmlPath == currentSonarLintXmlFile?.Path)
                {
                    return;
                }

                lock (IsInitializedGate)
                {
                    if (isInitialized && loadedSonarLintXmlPath == currentSonarLintXmlFile?.Path)
                    {
                        return;
                    }

                    loadedSonarLintXmlPath = currentSonarLintXmlFile?.Path;

                    if (loadedSonarLintXmlPath == null)
                    {
                        isInitialized = true;
                        return;
                    }

                    // we assume the returned set is not null
                    var sonarLintXml = currentSonarLintXmlFile.GetText().ToString();
                    enabledRules = ruleLoader.GetEnabledRules(sonarLintXml);
                    isInitialized = true;
                }
            }

            private static AdditionalText GetSonarLintXmlFile(AnalyzerOptions options) =>
                options.AdditionalFiles.FirstOrDefault(f => ParameterLoader.IsSonarLintXml(f.Path));
        }
    }
}
