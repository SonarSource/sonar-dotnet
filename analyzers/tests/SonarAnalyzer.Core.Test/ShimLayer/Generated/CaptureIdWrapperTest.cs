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

using SonarAnalyzer.CFG.Roslyn;
using IFlowCaptureReferenceOperation = Microsoft.CodeAnalysis.FlowAnalysis.IFlowCaptureReferenceOperation;

namespace SonarAnalyzer.Core.Test.ShimLayer.Generated;

[TestClass]
public class CaptureIdWrapperTest
{
    [TestMethod]
    public void ValidateReflection()
    {
        var code = """
            public class Sample
            {
                public string Method(object a, object b) =>
                    a?.ToString() + b?.ToString();
            }
            """;
        var cfg = TestCompiler.CompileCfgCS(code);
        var outerLocalLifetimeRegion = cfg.Root.NestedRegions.Single();
        outerLocalLifetimeRegion.Kind.Should().Be(ControlFlowRegionKind.LocalLifetime);
        outerLocalLifetimeRegion.NestedRegions.Should().HaveCount(2).And.OnlyContain(x => x.Kind == ControlFlowRegionKind.LocalLifetime);
        var nestedRegionA = outerLocalLifetimeRegion.NestedRegions.First();
        var nestedRegionB = outerLocalLifetimeRegion.NestedRegions.Last();
        var captureA = FindCapture(nestedRegionA, "a");
        var captureB = FindCapture(nestedRegionB, "b");

        captureA.Equals(captureA).Should().BeTrue();
        captureA.Equals(captureB).Should().BeFalse();
        captureA.GetHashCode().Should().NotBe(captureB.GetHashCode());

        nestedRegionA.CaptureIds.Should().ContainSingle().Which.Should().Be(captureA);
        nestedRegionB.CaptureIds.Should().ContainSingle().Which.Should().Be(captureB);
        nestedRegionA.CaptureIds.Single().GetHashCode().Should().Be(captureA.GetHashCode());
        nestedRegionA.CaptureIds.Single().Equals(captureA).Should().BeTrue();

        CaptureIdWrapper FindCapture(ControlFlowRegion region, string expectedName)
        {
            var flowCapture = (IFlowCaptureReferenceOperation)cfg.Blocks[region.FirstBlockOrdinal].BranchValue.ChildOperations.Single();
            flowCapture.Syntax.ToString().Should().Be(expectedName);
            return CaptureIdWrapper.From(flowCapture.Id);
        }
    }
}
