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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.ShimLayer.Common;

namespace SonarAnalyzer.Core.Test.ShimLayer.Common;

[TestClass]
public class AccessorFactoryTest
{
    [TestMethod]
    public void NullInstance_Throws()
    {
        var accessor = AccessorFactory.CreateProperty<Func<ClassDeclarationSyntax, ParameterListSyntax>>(typeof(ClassDeclarationSyntax), "ParameterList");
        FluentActions.Invoking(() => accessor(null)).Should().Throw<NullReferenceException>()
            .WithMessage("Object reference not set to an instance of an object. This ShimLayer accessor for ParameterList was called with 'null' sender.");
    }

    [TestMethod]
    public void ReturnType_CompileTimeType_Shimmed()
    {
        var accessor = AccessorFactory.CreateProperty<Func<ClassDeclarationSyntax, ParameterListSyntax>>(typeof(ClassDeclarationSyntax), "ParameterList");
        var declaration = CreateClassDeclaration();
        accessor(declaration).Should().Be(declaration.ParameterList);
    }

    [TestMethod]
    public void ReturnType_CompileTimeType_Fallback()
    {
        var accessor = AccessorFactory.CreateProperty<Func<ClassDeclarationSyntax, ParameterListSyntax>>(null, "ParameterList");
        accessor(CreateClassDeclaration()).Should().BeNull();
    }

    [TestMethod]
    public void ReturnType_ImmutableArrayOfIOperation_Shimmed()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IOperation, ImmutableArray<IOperation>>>(typeof(IInvocationOperation), "Arguments");
        accessor(CreateInvocationOperation()).Should().NotBeNull().And.HaveCount(1);
    }

    [TestMethod]
    public void ReturnType_ImmutableArrayOfIOperation_Fallback()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IOperation, ImmutableArray<IOperation>>>(null, "Arguments");
        accessor(CreateInvocationOperation()).Should().NotBeNull().And.BeEmpty();
    }

    [TestMethod]
    public void ReturnType_ImmutableArrayOfILocalSymbol_Shimmed()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IOperation, ImmutableArray<ILocalSymbol>>>(typeof(IForEachLoopOperation), "Locals");
        accessor(CreateForEachOperation()).Should().NotBeNull().And.HaveCount(1);
    }

    [TestMethod]
    public void ReturnType_ImmutableArrayOfILocalSymbol_Fallback()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IOperation, ImmutableArray<ILocalSymbol>>>(null, "Locals");
        accessor(CreateForEachOperation()).Should().NotBeNull().And.BeEmpty();
    }

    private static IInvocationOperation CreateInvocationOperation()
    {
        var compiler = new SnippetCompiler("""
            public class Sample
            {
                public void Method(int arg) => Method(42);
            }
            """);
        return (IInvocationOperation)compiler.Model.GetOperation(compiler.Nodes<InvocationExpressionSyntax>().Single());
    }

    private static IForEachLoopOperation CreateForEachOperation()
    {
        var compiler = new SnippetCompiler("""
            public class Sample
            {
                public void Method(int[] arg)
                {
                    foreach(var i in arg) { }
                }
            }
            """);
        return (IForEachLoopOperation)compiler.Model.GetOperation(compiler.Nodes<ForEachStatementSyntax>().Single());
    }

    private static ClassDeclarationSyntax CreateClassDeclaration() =>
        SyntaxFactory.ClassDeclaration("Sample")
            .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("First")).WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword))));
}
