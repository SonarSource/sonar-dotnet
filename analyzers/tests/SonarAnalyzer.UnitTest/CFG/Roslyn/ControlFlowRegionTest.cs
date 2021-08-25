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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.UnitTest.CFG.Roslyn
{
    [TestClass]
    public class ControlFlowRegionTest
    {
        [TestMethod]
        public void ValidateReflection()
        {
            const string code = @"
public class Sample
{
    public void Method()
    {
        var value = LocalMethod();
        try
        {
           throw new System.Exception();
        }
        catch(System.InvalidOperationException)
        {
        }
        finally
        {
            return value;
        }

        int LocalMethod() => 42;
    }
}";
            var root = Compile(code).Root;

            root.Should().NotBeNull();
            root.Kind.Should().Be(ControlFlowRegionKind.Root);
            root.EnclosingRegion.Should().BeNull();
            root.ExceptionType.Should().BeNull();
            root.FirstBlockOrdinal.Should().Be(0);
            root.LastBlockOrdinal.Should().Be(5);
            root.NestedRegions.Should().HaveCount(1);
            root.Locals.Should().BeEmpty();
            root.LocalFunctions.Should().BeEmpty();
            root.CaptureIds.Should().BeEmpty();

            var localLifetime = root.NestedRegions.Single();
            localLifetime.Kind.Should().Be(ControlFlowRegionKind.LocalLifetime);
            localLifetime.EnclosingRegion.Should().Be(root);
            localLifetime.ExceptionType.Should().BeNull();
            localLifetime.FirstBlockOrdinal.Should().Be(1);
            localLifetime.LastBlockOrdinal.Should().Be(4);
            localLifetime.NestedRegions.Should().HaveCount(1);
            localLifetime.Locals.Should().HaveCount(1).And.Contain(x => x.Name == "value");
            localLifetime.LocalFunctions.Should().HaveCount(1).And.Contain(x => x.Name == "LocalMethod");
            localLifetime.CaptureIds.Should().BeEmpty();

            var tryFinallyRegion = localLifetime.NestedRegions.Single();
            tryFinallyRegion.Kind.Should().Be(ControlFlowRegionKind.TryAndFinally);

            var tryRegion = tryFinallyRegion.NestedRegions.First();
            tryRegion.Kind.Should().Be(ControlFlowRegionKind.Try);

            var tryCatchRegion = tryRegion.NestedRegions.First();
            tryCatchRegion.Kind.Should().Be(ControlFlowRegionKind.TryAndCatch);

            var catchRegion = tryCatchRegion.NestedRegions.Last();
            catchRegion.Kind.Should().Be(ControlFlowRegionKind.Catch);
            catchRegion.ExceptionType.Should().NotBeNull();
            catchRegion.ExceptionType.Name.Should().Be("InvalidOperationException");
        }

        private ControlFlowGraph Compile(string snippet)
        {
            var (tree, semanticModel) = TestHelper.Compile(snippet);
            var method = tree.GetRoot().DescendantNodes().First(x => x.RawKind == (int)SyntaxKind.MethodDeclaration);
            return ControlFlowGraph.Create(method, semanticModel);
        }
    }
}
