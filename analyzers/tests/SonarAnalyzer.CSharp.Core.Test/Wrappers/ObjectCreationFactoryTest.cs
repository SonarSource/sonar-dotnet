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

namespace SonarAnalyzer.CSharp.Core.Test.Wrappers;

[TestClass]
public class ObjectCreationFactoryTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void ObjectCreationSyntax()
    {
        var snippet = new SnippetCompiler("""
            public class A
            {
                public int X;
                public A(int y) { }
            }
            public class B
            {
                void Foo()
                {
                    var bar = new A(1) { X = 2 };
                }
            }
            """);
        var objectCreation = snippet.Tree.Single<ObjectCreationExpressionSyntax>();
        var wrapper = ObjectCreationFactory.Create(objectCreation);
        wrapper.Expression.Should().BeEquivalentTo(objectCreation);
        wrapper.Initializer.Should().BeEquivalentTo(objectCreation.Initializer);
        wrapper.ArgumentList.Should().BeEquivalentTo(objectCreation.ArgumentList);
        wrapper.InitializerExpressions.Should().BeEquivalentTo(objectCreation.Initializer.Expressions);
        wrapper.TypeAsString(snippet.Model).Should().Be("A");
        wrapper.TypeSymbol(snippet.Model).Name.Should().Be("A");
        wrapper.MethodSymbol(snippet.Model).Parameters.Length.Should().Be(1);
    }

    [TestMethod]
    public void ObjectCreationEmptyInitializerSyntax()
    {
        var snippet = new SnippetCompiler("""
            public class A
            {
                public int X;
                public A(int y) { }
            }
            public class B
            {
                void Foo()
                {
                    var bar = new A(1);
                }
            }
            """);
        var wrapper = ObjectCreationFactory.Create(snippet.Tree.Single<ObjectCreationExpressionSyntax>());
        wrapper.Initializer.Should().BeNull();
        wrapper.InitializerExpressions.Should().BeNull();
    }

    [TestMethod]
    public void ImplicitObjectCreationSyntax()
    {
        var snippet = new SnippetCompiler("""
            public class A
            {
                public int X;
                public A(int y) { }
            }
            public class B
            {
                void Foo()
                {
                    A bar =new(1) { X = 2 };
                }
            }
            """);
        var objectCreation = (ImplicitObjectCreationExpressionSyntaxWrapper)snippet.Tree.GetRoot(TestContext.CancellationToken).DescendantNodes()
            .First(x => x.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression));
        var wrapper = ObjectCreationFactory.Create(objectCreation);
        wrapper.Expression.Should().BeEquivalentTo(objectCreation.Node);
        wrapper.Initializer.Should().BeEquivalentTo(objectCreation.Initializer);
        wrapper.ArgumentList.Should().BeEquivalentTo(objectCreation.ArgumentList);
        wrapper.InitializerExpressions.Should().BeEquivalentTo(objectCreation.Initializer.Expressions);
        wrapper.TypeAsString(snippet.Model).Should().Be("A");
        wrapper.TypeSymbol(snippet.Model).Name.Should().Be("A");
        wrapper.MethodSymbol(snippet.Model).Parameters.Length.Should().Be(1);
    }

    [TestMethod]
    public void ImplicitObjectCreationEmptyInitializerSyntax()
    {
        var snippet = new SnippetCompiler("""
            public class A
            {
                public int X;
                public A(int y) { }
            }
            public class B
            {
                void Foo()
                {
                    A bar = new (1);
                }
            }
            """);
        var wrapper = ObjectCreationFactory.Create(snippet.Tree.Single<ImplicitObjectCreationExpressionSyntax>());
        wrapper.Initializer.Should().BeNull();
        wrapper.InitializerExpressions.Should().BeNull();
    }

    [TestMethod]
    public void GivenImplicitObjectCreationSyntaxWithMissingType_HasEmptyType()
    {
        var snippet = new SnippetCompiler("""
            public class B
            {
                void Foo()
                {
                    var bar = new();
                }
            }
            """,
            true,
            AnalyzerLanguage.CSharp);
        ObjectCreationFactory.Create((ImplicitObjectCreationExpressionSyntaxWrapper)snippet.Tree.GetRoot(TestContext.CancellationToken).DescendantNodes()
            .First(x => x.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression)))
            .TypeAsString(snippet.Model)
            .Should().BeEmpty();
    }

    [TestMethod]
    public void ImplicitObjectCreation_UserDefinedNullable_DoesNotCrash()
    {
        var snippet = new SnippetCompiler("""
            public class Repro_3596_AD0001<T> // https://sonarsource.atlassian.net/browse/NET-3596
            {
                private readonly Nullable field;
                public Repro_3596_AD0001() => field = new(this);

                private class Nullable(Repro_3596_AD0001<T> outer) { }
            }
            """);
        ObjectCreationFactory.Create((ImplicitObjectCreationExpressionSyntaxWrapper)snippet.Tree.GetRoot(TestContext.CancellationToken).DescendantNodes()
            .First(x => x.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression)))
            .TypeAsString(snippet.Model)
            .Should().Be("Nullable");
    }

    [TestMethod]
    public void GivenNull_ThrowsException() =>
        FluentActions.Invoking(() => ObjectCreationFactory.Create(null)).Should().Throw<ArgumentNullException>();

    [TestMethod]
    public void GivenNonConstructor_ThrowsException() =>
        FluentActions.Invoking(() => ObjectCreationFactory.Create(new SnippetCompiler("public class A{}").Tree.Single<ClassDeclarationSyntax>()))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Unexpected type: ClassDeclarationSyntax");
}
