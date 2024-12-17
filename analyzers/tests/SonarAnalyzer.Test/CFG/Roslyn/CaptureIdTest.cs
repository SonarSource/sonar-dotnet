/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.CFG.Roslyn;
using StyleCop.Analyzers.Lightup;
using IFlowCaptureReferenceOperation = Microsoft.CodeAnalysis.FlowAnalysis.IFlowCaptureReferenceOperation;

namespace SonarAnalyzer.Test.CFG.Roslyn
{
    [TestClass]
    public class CaptureIdTest
    {
        [TestMethod]
        public void Null_ThrowsException()
        {
            Action a = () => new CaptureId(null).ToString();
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Equals_ReturnsFalse()
        {
            var capture = new CaptureId(new object());
            capture.Equals(42).Should().BeFalse();
            capture.Equals(null).Should().BeFalse();
        }

        [TestMethod]
        public void ValidateReflection()
        {
            const string code = @"
public class Sample
{
    public string Method(object a, object b) =>
        a?.ToString() + b?.ToString();
}";
            var cfg = TestCompiler.CompileCfgCS(code);
            var outerLocalLifetimeRegion = cfg.Root.NestedRegions.Single();
            outerLocalLifetimeRegion.Kind.Should().Be(ControlFlowRegionKind.LocalLifetime);
            outerLocalLifetimeRegion.NestedRegions.Should().HaveCount(2).And.OnlyContain(x => x.Kind == ControlFlowRegionKind.LocalLifetime);
            var nestedRegionA = outerLocalLifetimeRegion.NestedRegions.First();
            var nestedRegionB = outerLocalLifetimeRegion.NestedRegions.Last();
            var captureA = FindCapture(nestedRegionA, "a");
            var captureB = FindCapture(nestedRegionB, "b");
            // Assert
            nestedRegionA.CaptureIds.Should().HaveCount(1).And.Contain(captureA).And.NotContain(captureB);
            nestedRegionB.CaptureIds.Should().HaveCount(1).And.Contain(captureB).And.NotContain(captureA);
            nestedRegionA.CaptureIds.Single().GetHashCode().Should().Be(captureA.GetHashCode()).And.NotBe(captureB.GetHashCode());
            nestedRegionA.CaptureIds.Single().Equals((object)captureA).Should().BeTrue();
            nestedRegionA.CaptureIds.Single().Equals((object)captureB).Should().BeFalse();

            CaptureId FindCapture(ControlFlowRegion region, string expectedName)
            {
                var flowCapture = (IFlowCaptureReferenceOperation)cfg.Blocks[region.FirstBlockOrdinal].BranchValue.ChildOperations.Single();
                flowCapture.Syntax.ToString().Should().Be(expectedName);
                return new CaptureId(flowCapture.Id);
            }
        }
    }
}
