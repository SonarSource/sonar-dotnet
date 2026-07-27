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
    public static readonly AnalyzerConfiguration Default = new(false);
    public static readonly AnalyzerConfiguration WithSonarCfg = new(true);

    // Force the use of Sonar Cfg in rules that support both Roslyn and Sonar CFGs
    public bool ForceSonarCfg { get; }

    private AnalyzerConfiguration(bool forceSonarCfg) =>
        ForceSonarCfg = forceSonarCfg;
}
