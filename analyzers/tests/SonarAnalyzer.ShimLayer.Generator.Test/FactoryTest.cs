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

namespace SonarAnalyzer.ShimLayer.Generator.Test;

[TestClass]
public class FactoryTest
{
    [TestMethod]
    public void CreateAllFiles() =>
        Factory.CreateAllFiles().ToArray().Should().HaveCountGreaterThan(350)
            .And.Contain(x => x.Name == "SyntaxKind.g.cs", $"it's from {nameof(PartialEnumStrategy)}")
            .And.Contain(x => x.Name == "SarifVersion.g.cs", $"it's from {nameof(NewEnumStrategy)}")
            .And.Contain(x => x.Name == "SyntaxNode.g.cs", $"it's from {nameof(SyntaxNodeExtendStrategy)} as the base type")
            .And.Contain(x => x.Name == "RecordDeclarationSyntax.g.cs", $"it's from {nameof(SyntaxNodeWrapStrategy)} as inherited type")
            .And.Contain(x => x.Name == "IOperation.g.cs", $"it's from {nameof(IOperationStrategy)} as base interface")
            .And.Contain(x => x.Name == "IInvocationOperation.g.cs", $"it's from {nameof(IOperationStrategy)} as inherited interface")
            .And.AllSatisfy(x => x.Content.Should().Contain("namespace SonarAnalyzer.ShimLayer;"));
}
