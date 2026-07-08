/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Common;

public class AnalyzerConfiguration
{
    public static IAnalyzerConfiguration AlwaysEnabled { get; } = new AlwaysEnabledConfiguration(false);

    public static IAnalyzerConfiguration AlwaysEnabledWithSonarCfg { get; } = new AlwaysEnabledConfiguration(true);

    private sealed class AlwaysEnabledConfiguration : IAnalyzerConfiguration
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
}
