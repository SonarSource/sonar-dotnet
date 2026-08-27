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

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.ShimLayer.Common;

namespace SonarAnalyzer.Core.Test.ShimLayer.Common;

[TestClass]
public class AccessorFactoryTest
{
    private delegate bool TryGetValueAccessorDelegate<T, TKey, TValue>(T instance, TKey key, out TValue value);

    [TestMethod]
    public void NullInstance_Throws()
    {
        var accessor = AccessorFactory.CreateProperty<Func<ClassDeclarationSyntax, ParameterListSyntax>>(typeof(ClassDeclarationSyntax), nameof(ClassDeclarationSyntax.ParameterList));
        FluentActions.Invoking(() => accessor(null)).Should().Throw<NullReferenceException>()
            .WithMessage("Object reference not set to an instance of an object. This ShimLayer accessor for ParameterList was called with 'null' sender.");
    }

    [TestMethod]
    public void ReturnType_CompileTimeType_Shimmed()
    {
        var accessor = AccessorFactory.CreateProperty<Func<ClassDeclarationSyntax, ParameterListSyntax>>(typeof(ClassDeclarationSyntax), nameof(ClassDeclarationSyntax.ParameterList));
        var declaration = CreateClassDeclaration();
        accessor(declaration).Should().Be(declaration.ParameterList);
    }

    [TestMethod]
    public void ReturnType_CompileTimeType_Fallback()
    {
        var accessor = AccessorFactory.CreateProperty<Func<ClassDeclarationSyntax, ParameterListSyntax>>(null, nameof(ClassDeclarationSyntax.ParameterList));
        accessor(CreateClassDeclaration()).Should().BeNull();
    }

    [TestMethod]
    public void ReturnType_ImmutableArrayOfIOperation_Shimmed()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IOperation, ImmutableArray<IOperation>>>(typeof(IBlockOperation), nameof(IBlockOperation.Operations));
        var block = CreateForEachOperation().Parent;
        accessor(block).Should().NotBeNull().And.HaveCount(1);
    }

    [TestMethod]
    public void ReturnType_ImmutableArrayOfIOperation_Fallback()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IOperation, ImmutableArray<IOperation>>>(null, nameof(IBlockOperation.Operations));
        var block = CreateForEachOperation().Parent;
        accessor(block).Should().NotBeNull().And.BeEmpty();
    }

    [TestMethod]
    public void ReturnType_ImmutableArrayOfIArgumentOperationWrapper_Shimmed()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IOperation, ImmutableArray<IArgumentOperationWrapper>>>(typeof(IInvocationOperation), nameof(IInvocationOperation.Arguments));
        accessor(CreateInvocationOperation()).Should().NotBeNull().And.HaveCount(1);
    }

    [TestMethod]
    public void ReturnType_ImmutableArrayOfIArgumentOperationWrapper_Fallback()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IOperation, ImmutableArray<IArgumentOperationWrapper>>>(null, nameof(IInvocationOperation.Arguments));
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

    [TestMethod]
    public void ReturnType_ImmutableArrayOfNullableAnnotation_Shimmed()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IMethodSymbol, ImmutableArray<SonarAnalyzer.ShimLayer.NullableAnnotation>>>(
            typeof(IMethodSymbol),
            nameof(IMethodSymbol.TypeArgumentNullableAnnotations));
        accessor(CreateMethodSymbol()).Should().NotBeNull().And.HaveCount(1);
    }

    [TestMethod]
    public void ReturnType_ImmutableArrayOfNullableAnnotation_Fallback()
    {
        var accessor = AccessorFactory.CreateProperty<Func<IMethodSymbol, ImmutableArray<SonarAnalyzer.ShimLayer.NullableAnnotation>>>(
            null,
            nameof(IMethodSymbol.TypeArgumentNullableAnnotations));
        accessor(CreateMethodSymbol()).Should().NotBeNull().And.HaveCount(0);
    }

    [TestMethod]
    public void ReturnType_SeparatedSyntaxListOfWrappedType_Shimmed()
    {
        var accessor = AccessorFactory.CreateProperty<Func<CollectionExpressionSyntax, SeparatedSyntaxListWrapper<CollectionElementSyntaxWrapper>>>(typeof(CollectionExpressionSyntax), nameof(CollectionExpressionSyntax.Elements));
        var collectionExpression = CreateCollectionExpression();
        var result = accessor(collectionExpression);

        result.Should().NotBeNull().And.HaveCount(2);
        result[0].WrappedInstance.Should().Be(collectionExpression.Elements[0]);
        result[1].WrappedInstance.Should().Be(collectionExpression.Elements[1]);

        result.SeparatorCount.Should().Be(1);
        result.Separator(0).IsKind(SyntaxKind.CommaToken);
    }

    [TestMethod]
    public void ReturnType_SeparatedSyntaxListOfWrappedType_Fallback()
    {
        var accessor = AccessorFactory.CreateProperty<Func<CollectionExpressionSyntax, SeparatedSyntaxListWrapper<CollectionElementSyntaxWrapper>>>(null, nameof(CollectionExpressionSyntax.Elements));
        accessor(CreateCollectionExpression()).Should().NotBeNull().And.BeEmpty();
    }

    [TestMethod]
    public void ReturnType_SeparatedSyntaxListOfCompiletimeType_Shimmed()
    {
        var accessor = AccessorFactory.CreateProperty<Func<TupleExpressionSyntax, SeparatedSyntaxList<ArgumentSyntax>>>(typeof(TupleExpressionSyntax), nameof(TupleExpressionSyntax.Arguments));
        accessor(CreateTupleExpressionSyntax()).Should().NotBeNull().And.HaveCount(1);
    }

    [TestMethod]
    public void ReturnType_SeparatedSyntaxListOfCompiletimeType_Fallback()
    {
        var accessor = AccessorFactory.CreateProperty<Func<TupleExpressionSyntax, SeparatedSyntaxList<ArgumentSyntax>>>(null, nameof(TupleExpressionSyntax.Arguments));
        accessor(CreateTupleExpressionSyntax()).Should().NotBeNull().And.BeEmpty();
    }

    [TestMethod]
    public void CreateMethod_NullInstance_Throws()
    {
        var accessor = AccessorFactory.CreateMethod<Func<ClassDeclarationSyntax, ParameterSyntax[], ClassDeclarationSyntax>>(typeof(ClassDeclarationSyntax), "AddParameterListParameters");
        FluentActions.Invoking(() => accessor(null, [])).Should().Throw<NullReferenceException>()
            .WithMessage("Object reference not set to an instance of an object. This ShimLayer accessor for AddParameterListParameters was called with 'null' sender.");
    }

    [TestMethod]
    public void CreateMethod_WithArrayOfCompiletimeType_Shimmed()
    {
        var accessor = AccessorFactory.CreateMethod<Func<ClassDeclarationSyntax, ParameterSyntax[], ClassDeclarationSyntax>>(typeof(ClassDeclarationSyntax), "AddParameterListParameters");
        var result = accessor(CreateClassDeclaration(), [CreateParameter()]);
        result.Should().NotBeNull();
        result.ParameterList.Parameters.Should().HaveCount(2);
    }

    [TestMethod]
    public void CreateMethod_WithArrayOfCompiletimeType_Fallback()
    {
        var accessor = AccessorFactory.CreateMethod<Func<ClassDeclarationSyntax, ParameterSyntax[], ClassDeclarationSyntax>>(null, "AddParameterListParameters");
        var result = accessor(CreateClassDeclaration(), [CreateParameter()]);
        result.Should().BeNull();
    }

    [TestMethod]
    public void CreateMethod_WithArrayOfWrappedType_Shimmed()
    {
        var accessor = AccessorFactory.CreateMethod<Func<TypeSyntax, TupleElementSyntaxWrapper[], TypeSyntax>>(typeof(TupleTypeSyntax), nameof(TupleTypeSyntax.AddElements));
        accessor(CreateTupleTypeSyntax(), [TupleElementSyntaxWrapper.From(SyntaxFactory.TupleElement(CreateTypeSyntax()))]).Should().BeOfType<TupleTypeSyntax>().Which.Elements.Should().HaveCount(2);
    }

    [TestMethod]
    public void CreateMethod_WithArrayOfWrappedType_Fallback()
    {
        var accessor = AccessorFactory.CreateMethod<Func<TypeSyntax, TupleElementSyntaxWrapper[], TypeSyntax>>(null, nameof(TupleTypeSyntax.AddElements));
        accessor(CreateTupleTypeSyntax(), [TupleElementSyntaxWrapper.From(SyntaxFactory.TupleElement(CreateTypeSyntax()))]).Should().BeNull();
    }

    [TestMethod]
    public void CreateMethod_FromFallbackWrappedTypeName()
    {
        var accessor = AccessorFactory.CreateMethod<Func<MemberDeclarationSyntax, SyntaxList<UsingDirectiveSyntax>, MemberDeclarationSyntax>>(typeof(BaseNamespaceDeclarationSyntax), "WithUsings");
        var syntax = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("NS"));
        accessor(syntax, []).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateMethod_WithOutParameter_Shimmed()
    {
        var accessor = AccessorFactory.CreateMethod<TryGetValueAccessorDelegate<TestAnalyzerConfigOptions, string, string>>(typeof(TestAnalyzerConfigOptions), nameof(AnalyzerConfigOptions.TryGetValue));
        accessor(new TestAnalyzerConfigOptions(), "AnyKey", out var value).Should().BeTrue();
        value.Should().Be("ExistingValue");
    }

    [TestMethod]
    public void CreateMethod_WithOutParameter_Fallback()
    {
        var accessor = AccessorFactory.CreateMethod<TryGetValueAccessorDelegate<TestAnalyzerConfigOptions, string, string>>(null, nameof(AnalyzerConfigOptions.TryGetValue));
        accessor(new TestAnalyzerConfigOptions(), "AnyKey", out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void CreateMethod_Void_Shimmed()
    {
        var accessor = AccessorFactory.CreateMethod<Action<CollectionExpressionSyntax, CSharpSyntaxVisitor>>(typeof(CollectionExpressionSyntax), nameof(CollectionExpressionSyntax.Accept));
        var visitor = new TestVisitor();
        accessor(CreateCollectionExpression(), visitor);
        visitor.Visited.Should().BeTrue();
    }

    [TestMethod]
    public void CreateMethod_Void_Fallback()
    {
        var accessor = AccessorFactory.CreateMethod<Action<CollectionExpressionSyntax, CSharpSyntaxVisitor>>(null, nameof(CollectionExpressionSyntax.Accept));
        var visitor = new TestVisitor();
        accessor(CreateCollectionExpression(), visitor);
        visitor.Visited.Should().BeFalse();
    }

    [TestMethod]
    public void CreateMethod_WithEnumReturnType_Shimmed()
    {
        var accessor = AccessorFactory.CreateMethod<Func<SemanticModel, int, SonarAnalyzer.ShimLayer.NullableContext>>(typeof(SemanticModel), nameof(SemanticModel.GetNullableContext));
        accessor(CreateInvocationOperation().SemanticModel, 0).Should().Be(SonarAnalyzer.ShimLayer.NullableContext.ContextInherited);
    }

    [TestMethod]
    public void CreateMethod_WithEnumReturnType_Fallback()
    {
        var accessor = AccessorFactory.CreateMethod<Func<SemanticModel, int, SonarAnalyzer.ShimLayer.NullableContext>>(null, nameof(SemanticModel.GetNullableContext));
        accessor(CreateInvocationOperation().SemanticModel, 0).Should().Be(SonarAnalyzer.ShimLayer.NullableContext.Disabled);
    }

    [TestMethod]
    public void CreateStaticProperty_Shimmed()
    {
        var accessor = AccessorFactory.CreateStaticProperty<Func<StringComparer>>(typeof(AnalyzerConfigOptions), nameof(AnalyzerConfigOptions.KeyComparer));
        accessor().Should().NotBeNull();
    }

    [TestMethod]
    public void CreateStaticProperty_Fallback()
    {
        var accessor = AccessorFactory.CreateStaticProperty<Func<StringComparer>>(null, nameof(AnalyzerConfigOptions.KeyComparer));
        accessor().Should().BeNull();
    }

    [TestMethod]
    public void Create_MethodWithEnumParameter_Shimmed()
    {
        var accessor = AccessorFactory
            .CreateMethod<Func<Compilation, SyntaxTree, SonarAnalyzer.ShimLayer.SemanticModelOptions, SemanticModel>>(typeof(Compilation), nameof(Compilation.GetSemanticModel));
        var model = CreateInvocationOperation().SemanticModel;
        accessor(model.Compilation, model.SyntaxTree, SonarAnalyzer.ShimLayer.SemanticModelOptions.IgnoreAccessibility).Should().NotBeNull();
    }

    [TestMethod]
    public void Create_MethodWithEnumParameter_Fallback()
    {
        var accessor = AccessorFactory
            .CreateMethod<Func<Compilation, SyntaxTree, SonarAnalyzer.ShimLayer.SemanticModelOptions, SemanticModel>>(null, nameof(Compilation.GetSemanticModel));
        var model = CreateInvocationOperation().SemanticModel;
        accessor(model.Compilation, model.SyntaxTree, SonarAnalyzer.ShimLayer.SemanticModelOptions.IgnoreAccessibility).Should().BeNull();
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

    private static IMethodSymbol CreateMethodSymbol() =>
        new SnippetCompiler("""
            public class Sample
            {
                public void Method<T>() { }
            }
            """).MethodSymbol("Sample.Method");

    private static ClassDeclarationSyntax CreateClassDeclaration() =>
        SyntaxFactory.ClassDeclaration("Sample")
            .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("First")).WithType(CreateTypeSyntax()));

    private static CollectionExpressionSyntax CreateCollectionExpression() =>
        SyntaxFactory.CollectionExpression()
            .AddElements(SyntaxFactory.ExpressionElement(SyntaxFactory.IdentifierName("first")), SyntaxFactory.ExpressionElement(SyntaxFactory.IdentifierName("second")));

    private static TupleExpressionSyntax CreateTupleExpressionSyntax() =>
        SyntaxFactory.TupleExpression().AddArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("first")));

    private static TupleTypeSyntax CreateTupleTypeSyntax() =>
        SyntaxFactory.TupleType().AddElements(SyntaxFactory.TupleElement(CreateTypeSyntax()));

    private static ParameterSyntax CreateParameter() =>
        SyntaxFactory.Parameter(SyntaxFactory.Identifier("Name")).WithType(CreateTypeSyntax());

    private static TypeSyntax CreateTypeSyntax() =>
        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));

    private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string value)
        {
            value = "ExistingValue";
            return true;
        }
    }

    private sealed class TestVisitor : CSharpSyntaxVisitor
    {
        public bool Visited { get; private set; }

        public override void VisitCollectionExpression(CollectionExpressionSyntax node) =>
            Visited = true;
    }
}
