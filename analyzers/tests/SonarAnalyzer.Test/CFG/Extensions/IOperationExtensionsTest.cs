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

namespace SonarAnalyzer.CFG.Extensions.Test;

[TestClass]
public class IOperationExtensionsTest
{
    [TestMethod]
    public void DescendantsAndSelf_Null_ReturnsNull() =>
        IOperationExtensions.DescendantsAndSelf(null).Should().BeEmpty();

    [TestMethod]
    public void OperationWrapperSonarPropertyShortcuts()
    {
        var operation = Operation<VariableDeclarationSyntax>("""
            public class Sample
            {
                public void Method()
                {
                    var value = 42;
                }
            }
            """).ToVariableDeclaration();
        operation.Parent().Should().NotBeNull();
        operation.Parent().Kind.Should().Be(OperationKind.VariableDeclarationGroup);
        operation.Children().Should().HaveCount(1);
        operation.Children().Single().Kind.Should().Be(OperationKind.VariableDeclarator);
        operation.Language().Should().Be("C#");
        operation.IsImplicit().Should().Be(false);
        operation.SemanticModel().Should().Be(operation.SemanticModel());
    }

    [TestMethod]
    public void AsForEachLoop_ForEachLoop_ConvertsToWrapper() =>
        Operation<MethodDeclarationSyntax>("""
            public class Sample
            {
                public void Method(int[] items)
                {
                    foreach (var item in items)
                    {
                        // Do something with item
                    }
                }
            }
            """).Descendants().OfType<ILoopOperation>().Single().AsForEachLoop().Should().NotBeNull();

    [TestMethod]
    [DataRow("for (var i = 0; i < 9; i++) { }")]
    [DataRow("while (true) { }")]
    [DataRow("do { } while (true);")]
    public void AsForEachLoop_OtherLoop_ConvertsToWrapper(string loop) =>
        Operation<MethodDeclarationSyntax>($$"""
            public class Sample
            {
                public void Method()
                {
                    {{loop}}
                }
            }
            """).Descendants().OfType<ILoopOperation>().Single().AsForEachLoop().Should().BeNull();

    [TestMethod]
    public void ExtensionsMethodsUsedByArchitecture()
    {
        IOperation operation = null;
        // These extension methods are used by sonar-architecture. Do not remove them.
        Assert.ThrowsException<NullReferenceException>(() => operation.AsForEachLoop());
        Assert.ThrowsException<NullReferenceException>(() => operation.AsVariableDeclarator());
        operation.ToArrayCreation();
        operation.ToCatchClause();
        operation.ToConversion();
        operation.ToInvocation();
        operation.ToIsType();
        operation.ToLocalFunction();
        operation.ToMemberReference();
        operation.ToObjectCreation();
        operation.ToPattern();
        operation.ToVariableDeclaration();
        operation.ToVariableDeclarator();
    }

    private static IOperation Operation<T>(string code) where T : SyntaxNode
    {
        var (tree, model) = TestCompiler.CompileCS(code);
        return model.GetOperation(tree.Single<T>());
    }
}
