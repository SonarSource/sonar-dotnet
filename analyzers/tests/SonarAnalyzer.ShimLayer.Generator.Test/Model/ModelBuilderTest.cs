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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.ShimLayer.Generator.Model.Test;

[TestClass]
public class ModelBuilderTest
{
    [TestMethod]
    public void Build_NestedTypes()
    {
        var type = typeof(IOperation.OperationList);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model[type].Should().BeOfType<SkipStrategy>();
    }

    [TestMethod]
    public void Build_GenericTypes()
    {
        var type = typeof(IEnumerable<int>);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model[type].Should().BeOfType<SkipStrategy>();
    }

    [TestMethod]
    public void Build_Delegates()
    {
        var type = typeof(SyntaxReceiverCreator);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model[type].Should().BeOfType<SkipStrategy>();
    }

    [TestMethod]
    public void Build_Enums_NoBaseline()
    {
        var type = typeof(NamespaceKind);
        var members = type.GetMembers();
        var model = ModelBuilder.Build([new TypeDescriptor(type, members)], []);
        model[type].Should().BeOfType<NewEnumStrategy>()
            .Which.Fields.Select(x => x.Name).Should().BeEquivalentTo([
                "Assembly",
                "Compilation",
                "Module"]);
        // Make sure we've sent other members that are not in the result
        members.Should().ContainSingle(x => x is FieldInfo && x.Name == "value__"); // IsSpecialName
        members.Should().ContainSingle(x => x is MethodInfo && x.Name == "HasFlag"); // IsSpecialName
    }

    [TestMethod]
    public void Build_Enums_WithBaseline()
    {
        var type = typeof(NamespaceKind);
        var assembly = type.GetMember("Assembly").Single();
        var compilation = type.GetMember("Compilation").Single();
        var module = type.GetMember("Module").Single();

        var model = ModelBuilder.Build([new(type, [assembly, compilation, module])], [new(type, [assembly])]);
        model[type].Should().BeOfType<PartialEnumStrategy>()
            .Which.Fields.Select(x => x.Name).Should().BeEquivalentTo([
                "Compilation",
                "Module"]);
    }

    [TestMethod]
    public void Build_SyntaxNode_Itself()
    {
        var type = typeof(SyntaxNode);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model[type].Should().BeOfType<SyntaxNodeStrategy>();
    }

    [TestMethod]
    public void Build_SyntaxNode_RecordDeclaration()
    {
        var type = typeof(RecordDeclarationSyntax);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], [
            new(typeof(TypeDeclarationSyntax), []),
            new(typeof(BaseTypeDeclarationSyntax), []),
            new(typeof(MemberDeclarationSyntax), []),
            new(typeof(CSharpSyntaxNode), []),
            new(typeof(SyntaxNode), [])]);
        model[type].Should().BeOfType<SyntaxNodeStrategy>().Which.BaseType.FullName.Should().Be(typeof(TypeDeclarationSyntax).FullName);
    }

    [TestMethod]
    public void Build_SyntaxNode_NoCommonBaseType()
    {
        var type = typeof(RecordDeclarationSyntax);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model[type].Should().BeOfType<SyntaxNodeStrategy>().Which.BaseType.Should().BeNull();
    }

    [TestMethod]
    public void Build_IOperation()
    {
        using var typeLoader = new TypeLoader();
        var type = typeLoader.LoadLatest().Single(x => x.Type.Name == nameof(IOperation));
        var model = ModelBuilder.Build([type], []);
        model[type.Type].Should().BeOfType<IOperationStrategy>()
            .Which.Members.Select(x => x.Member.ToString()).Should().BeEquivalentTo([
                "System.Void Accept(Microsoft.CodeAnalysis.Operations.OperationVisitor)",
                "TResult Accept[TArgument,TResult](Microsoft.CodeAnalysis.Operations.OperationVisitor`2[TArgument,TResult], TArgument)",
                "Microsoft.CodeAnalysis.IOperation Parent",
                "Microsoft.CodeAnalysis.OperationKind Kind",
                "Microsoft.CodeAnalysis.SyntaxNode Syntax",
                "Microsoft.CodeAnalysis.ITypeSymbol Type",
                "Microsoft.CodeAnalysis.Optional`1[System.Object] ConstantValue",
                "System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.IOperation] Children",
                "Microsoft.CodeAnalysis.IOperation+OperationList ChildOperations",
                "System.String Language",
                "System.Boolean IsImplicit",
                "Microsoft.CodeAnalysis.SemanticModel SemanticModel"]);
    }

    [TestMethod]
    public void Build_StaticClass()
    {
        var type = typeof(GeneratorExtensions);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model[type].Should().BeOfType<SkipStrategy>();  // ToDo: This will change later, likely to StaticClassStrategy
    }

    [TestMethod]
    public void Build_NoChangeStrategy()
    {
        var type = typeof(NamespaceKind);
        var members = type.GetMembers();

        var model = ModelBuilder.Build(
            [new(type, members)],
            [new(type, members.OrderByDescending(x => x.ToString()).ToArray())]);
        model[type].Should().BeOfType<NoChangeStrategy>();
    }

    [TestMethod]
    public void Build_NoChangeStrategy_DifferentMembers()
    {
        var type = typeof(SyntaxToken);
        var members = type.GetMembers();

        var model = ModelBuilder.Build(
            [new(type, members)],
            [new(type, [])]);           // Fallback for types that do have a baseline (can be used), but do not have a dedicated strategy
        model[type].Should().BeOfType<NoChangeStrategy>();
    }

    [TestMethod]
    public void CreateMembers_NoBaseline()
    {
        var type = typeof(SyntaxNode);
        var parent = type.GetMember("Parent").Single();
        var ancestors = type.GetMember("Ancestors").Single();
        var model = ModelBuilder.Build([new TypeDescriptor(type, [parent, ancestors])], []);
        model[type].Should().BeOfType<SyntaxNodeStrategy>()
            .Which.Members.Should().BeEquivalentTo([
                new MemberDescriptor(parent, false),
                new MemberDescriptor(ancestors, false)]);
    }

    [TestMethod]
    public void CreateMembers_WithBaseline()
    {
        using var typeLoader = new TypeLoader();
        var baseline = typeLoader.LoadBaseline().Single(x => x.Type.Name == nameof(SyntaxNode));
        var latest = typeLoader.LoadLatest().Single(x => x.Type.Name == nameof(SyntaxNode));
        var model = ModelBuilder.Build([latest], [baseline]);
        model[latest.Type].Should().BeOfType<SyntaxNodeStrategy>()
            .Which.Members.Select(x => (x.Member.ToString(), x.IsPassthrough)).Should().BeEquivalentTo([
                ("System.String ToFullString()", true),
                ("System.Void WriteTo(System.IO.TextWriter)", true),
                ("Microsoft.CodeAnalysis.Text.SourceText GetText(System.Text.Encoding, Microsoft.CodeAnalysis.Text.SourceHashAlgorithm)", true),
                ("System.Boolean IsEquivalentTo(Microsoft.CodeAnalysis.SyntaxNode)", true),
                ("System.Boolean IsIncrementallyIdenticalTo(Microsoft.CodeAnalysis.SyntaxNode)", false),
                ("System.Boolean IsPartOfStructuredTrivia()", true),
                ("System.Boolean ContainsDirective(System.Int32)", false),
                ("System.Boolean Contains(Microsoft.CodeAnalysis.SyntaxNode)", true),
                ("Microsoft.CodeAnalysis.Location GetLocation()", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.Diagnostic] GetDiagnostics()", true),
                ("Microsoft.CodeAnalysis.SyntaxReference GetReference()", true),
                ("Microsoft.CodeAnalysis.ChildSyntaxList ChildNodesAndTokens()", true),
                ("Microsoft.CodeAnalysis.SyntaxNodeOrToken ChildThatContainsPosition(System.Int32)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNode] ChildNodes()", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNode] Ancestors(System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNode] AncestorsAndSelf(System.Boolean)", true),
                ("TNode FirstAncestorOrSelf[TNode](System.Func`2[TNode,System.Boolean], System.Boolean)", true),
                ("TNode FirstAncestorOrSelf[TNode,TArg](System.Func`3[TNode,TArg,System.Boolean], TArg, System.Boolean)", false),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNode] DescendantNodes(System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNode] DescendantNodes(Microsoft.CodeAnalysis.Text.TextSpan, System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNode] DescendantNodesAndSelf(System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNode] DescendantNodesAndSelf(Microsoft.CodeAnalysis.Text.TextSpan, System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNodeOrToken] DescendantNodesAndTokens(System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNodeOrToken] DescendantNodesAndTokens(Microsoft.CodeAnalysis.Text.TextSpan, System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNodeOrToken] DescendantNodesAndTokensAndSelf(System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNodeOrToken] DescendantNodesAndTokensAndSelf(Microsoft.CodeAnalysis.Text.TextSpan, System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("Microsoft.CodeAnalysis.SyntaxNode FindNode(Microsoft.CodeAnalysis.Text.TextSpan, System.Boolean, System.Boolean)", true),
                ("Microsoft.CodeAnalysis.SyntaxToken FindToken(System.Int32, System.Boolean)", true),
                ("Microsoft.CodeAnalysis.SyntaxToken GetFirstToken(System.Boolean, System.Boolean, System.Boolean, System.Boolean)", true),
                ("Microsoft.CodeAnalysis.SyntaxToken GetLastToken(System.Boolean, System.Boolean, System.Boolean, System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxToken] ChildTokens()", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxToken] DescendantTokens(System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxToken] DescendantTokens(Microsoft.CodeAnalysis.Text.TextSpan, System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("Microsoft.CodeAnalysis.SyntaxTriviaList GetLeadingTrivia()", true),
                ("Microsoft.CodeAnalysis.SyntaxTriviaList GetTrailingTrivia()", true),
                ("Microsoft.CodeAnalysis.SyntaxTrivia FindTrivia(System.Int32, System.Boolean)", true),
                ("Microsoft.CodeAnalysis.SyntaxTrivia FindTrivia(System.Int32, System.Func`2[Microsoft.CodeAnalysis.SyntaxTrivia,System.Boolean])", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxTrivia] DescendantTrivia(System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxTrivia] DescendantTrivia(Microsoft.CodeAnalysis.Text.TextSpan, System.Func`2[Microsoft.CodeAnalysis.SyntaxNode,System.Boolean], System.Boolean)", true),
                ("System.Boolean HasAnnotations(System.String)", true),
                ("System.Boolean HasAnnotations(System.Collections.Generic.IEnumerable`1[System.String])", true),
                ("System.Boolean HasAnnotation(Microsoft.CodeAnalysis.SyntaxAnnotation)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxAnnotation] GetAnnotations(System.String)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxAnnotation] GetAnnotations(System.Collections.Generic.IEnumerable`1[System.String])", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNodeOrToken] GetAnnotatedNodesAndTokens(System.String)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNodeOrToken] GetAnnotatedNodesAndTokens(System.String[])", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNodeOrToken] GetAnnotatedNodesAndTokens(Microsoft.CodeAnalysis.SyntaxAnnotation)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNode] GetAnnotatedNodes(Microsoft.CodeAnalysis.SyntaxAnnotation)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxNode] GetAnnotatedNodes(System.String)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxToken] GetAnnotatedTokens(Microsoft.CodeAnalysis.SyntaxAnnotation)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxToken] GetAnnotatedTokens(System.String)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxTrivia] GetAnnotatedTrivia(System.String)", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxTrivia] GetAnnotatedTrivia(System.String[])", true),
                ("System.Collections.Generic.IEnumerable`1[Microsoft.CodeAnalysis.SyntaxTrivia] GetAnnotatedTrivia(Microsoft.CodeAnalysis.SyntaxAnnotation)", true),
                ("T CopyAnnotationsTo[T](T)", true),
                ("System.Boolean IsEquivalentTo(Microsoft.CodeAnalysis.SyntaxNode, System.Boolean)", true),
                ("System.Void SerializeTo(System.IO.Stream, System.Threading.CancellationToken)", true),
                ("System.Int32 RawKind", true),
                ("System.String Language", true),
                ("Microsoft.CodeAnalysis.SyntaxTree SyntaxTree", true),
                ("Microsoft.CodeAnalysis.Text.TextSpan FullSpan", true),
                ("Microsoft.CodeAnalysis.Text.TextSpan Span", true),
                ("System.Int32 SpanStart", true),
                ("System.Boolean IsMissing", true),
                ("System.Boolean IsStructuredTrivia", true),
                ("System.Boolean HasStructuredTrivia", true),
                ("System.Boolean ContainsSkippedText", true),
                ("System.Boolean ContainsDiagnostics", true),
                ("System.Boolean ContainsDirectives", true),
                ("System.Boolean HasLeadingTrivia", true),
                ("System.Boolean HasTrailingTrivia", true),
                ("Microsoft.CodeAnalysis.SyntaxNode Parent", true),
                ("Microsoft.CodeAnalysis.SyntaxTrivia ParentTrivia", true),
                ("System.Boolean ContainsAnnotations", true),]);
    }

    [TestMethod]
    public void CreateMembers_SkippedMembers()
    {
        var type = typeof(SyntaxNode);
        var membersToSkip = new[]
        {
            type.GetMember("get_SyntaxTree").First(),
            type.GetMember("GetType").First(),
            type.GetMember("Equals").First(),
            type.GetMember("GetHashCode").First(),
            type.GetMember("ToString").First()
        };
        var model = ModelBuilder.Build([new(type, membersToSkip)], []);
        model[type].Should().BeOfType<SyntaxNodeStrategy>().Which.Members.Should().BeEmpty();
    }
}
