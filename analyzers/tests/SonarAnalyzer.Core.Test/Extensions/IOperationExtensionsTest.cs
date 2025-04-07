/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.CFG.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Core.Test.Extensions;

[TestClass]
public class IOperationExtensionsTest
{
    [TestMethod]
    public void Null_ReturnsNull() =>
        IOperationExtensions.DescendantsAndSelf(null).Should().BeEmpty();

    [TestMethod]
    public void ValidateReflection()
    {
        var sut = CreateOperationWrapper(out var model);
        sut.Parent().Should().NotBeNull();
        sut.Parent().Kind.Should().Be(OperationKind.VariableDeclarationGroup);
        sut.Children().Should().HaveCount(1);
        sut.Children().Single().Kind.Should().Be(OperationKind.VariableDeclarator);
        sut.Language().Should().Be("C#");
        sut.IsImplicit().Should().Be(false);
        sut.SemanticModel().Should().Be(model);
    }

    private static IVariableDeclarationOperationWrapper CreateOperationWrapper(out SemanticModel model)
    {
        const string code = """
        public class Sample
        {
            public void Method()
            {
                var value = 42;
            }
        }
        """;
        var (tree, localModel) = TestCompiler.CompileCS(code);
        var declaration = tree.Single<VariableDeclarationSyntax>();
        model = localModel;
        return localModel.GetOperation(declaration).ToVariableDeclaration();
    }
}
