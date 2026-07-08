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

using static SonarAnalyzer.Core.Common.AnalyzerConfiguration;

namespace SonarAnalyzer.Core.Test.Common;

[TestClass]
public class AnalyzerConfigurationTest
{
    [TestMethod]
    public void AlwaysEnabled_WhenNotInitialized_ReturnsTrue() =>
        AlwaysEnabled.IsEnabled("S101").Should().BeTrue();

    [TestMethod]
    public void AlwaysEnabled_AnyValue_ReturnsTrue()
    {
        AlwaysEnabled.IsEnabled(null).Should().BeTrue();
        AlwaysEnabled.IsEnabled(string.Empty).Should().BeTrue();
        AlwaysEnabled.IsEnabled("foo").Should().BeTrue();
    }

    [TestMethod]
    public void ForceSonarCfg_DisabledByDefault()
    {
        AlwaysEnabled.ForceSonarCfg.Should().BeFalse();
        AlwaysEnabledWithSonarCfg.ForceSonarCfg.Should().BeTrue();
    }

    [TestMethod]
    public void AlwaysEnabled_IgnoresInitialize()
    {
        var sut = AlwaysEnabled;

        sut.Initialize(null);
        sut.IsEnabled("S0000").Should().BeTrue();
    }
}
