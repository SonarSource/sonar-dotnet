/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CFG.Extensions;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.Core.Test.Extensions;

[TestClass]
public class BasicBlockExtensionsTest
{
    [TestMethod]
    public void IsEnclosedIn_ReturnsTrueForLocalLifeTime()
    {
        const string code = @"
public class Sample
{
    public void Method()
    {
        var t = true || true;
    }
}";
        var cfg = TestCompiler.CompileCfgCS(code);
        var localLifetimeRegion = cfg.Root.NestedRegions.Single();
        var block = cfg.Blocks[localLifetimeRegion.FirstBlockOrdinal];

        block.IsEnclosedIn(ControlFlowRegionKind.LocalLifetime).Should().BeTrue();
    }

    [TestMethod]
    public void IsEnclosedIn_IgnoresLocalLifeTimeForOtherKinds()
    {
        const string code = @"
public class Sample
{
    public void Method()
    {
        try
        {
            DoSomething();
            var t = true || true;   // This causes LocalLivetimeRegion to be generated
        }
        catch
        {
        }
    }

    public void DoSomething() { }
}";
        var cfg = TestCompiler.CompileCfgCS(code);
        var block = cfg.Blocks[2];

        block.EnclosingRegion.Kind.Should().Be(ControlFlowRegionKind.LocalLifetime);
        block.EnclosingRegion.EnclosingRegion.Kind.Should().Be(ControlFlowRegionKind.Try);
        block.EnclosingRegion.EnclosingRegion.EnclosingRegion.Kind.Should().Be(ControlFlowRegionKind.TryAndCatch);
        block.IsEnclosedIn(ControlFlowRegionKind.Try).Should().BeTrue();
        block.IsEnclosedIn(ControlFlowRegionKind.Catch).Should().BeFalse();
        block.IsEnclosedIn(ControlFlowRegionKind.TryAndCatch).Should().BeFalse();   // Because it's enclosed in Try
        block.IsEnclosedIn(ControlFlowRegionKind.LocalLifetime).Should().BeTrue();  // When asking for it, it should return
    }
}
