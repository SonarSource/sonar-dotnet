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

namespace SonarAnalyzer.Test.CFG.Roslyn
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
            value = 0;
        }

        int LocalMethod() => 42;
    }
}";
            var root = TestCompiler.CompileCfgCS(code).Root;

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
    }
}
