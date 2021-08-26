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
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG.Roslyn;
using IFlowCaptureReferenceOperation = Microsoft.CodeAnalysis.FlowAnalysis.IFlowCaptureReferenceOperation;

namespace SonarAnalyzer.UnitTest.CFG.Roslyn
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
            var cfg = Compile(code);
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
            nestedRegionA.CaptureIds.Single().GetHashCode().Should().Be(captureA.GetHashCode()).And.Should().NotBe(captureB.GetHashCode());
            nestedRegionA.CaptureIds.Single().Equals((object)captureA).Should().BeTrue();
            nestedRegionA.CaptureIds.Single().Equals((object)captureB).Should().BeFalse();

            CaptureId FindCapture(ControlFlowRegion region, string expectedName)
            {
                var flowCapture = (IFlowCaptureReferenceOperation)cfg.Blocks[region.FirstBlockOrdinal].BranchValue.Children.Single();
                flowCapture.Syntax.ToString().Should().Be(expectedName);
                return new CaptureId(flowCapture.Id);
            }
        }

        private static ControlFlowGraph Compile(string snippet)
        {
            var (tree, semanticModel) = TestHelper.Compile(snippet);
            var method = tree.GetRoot().DescendantNodes().First(x => x.RawKind == (int)SyntaxKind.MethodDeclaration);
            return ControlFlowGraph.Create(method, semanticModel);
        }
    }
}
