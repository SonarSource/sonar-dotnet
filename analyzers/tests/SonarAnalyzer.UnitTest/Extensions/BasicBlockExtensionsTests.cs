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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class BaciBlockExtensionsTests
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
            var cfg = TestHelper.CompileCfg(code);
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
            var cfg = TestHelper.CompileCfg(code);
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
}
