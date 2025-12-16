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
using Microsoft.CodeAnalysis.Operations;

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
        model[type].Should().BeOfType<SyntaxNodeWrapStrategy>();
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
        model[type].Should().BeOfType<SyntaxNodeWrapStrategy>().Which.BaseType.FullName.Should().Be(typeof(TypeDeclarationSyntax).FullName);
    }

    [TestMethod]
    public void Build_SyntaxNode_NoCommonBaseType()
    {
        var type = typeof(RecordDeclarationSyntax);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model[type].Should().BeOfType<SyntaxNodeWrapStrategy>().Which.BaseType.Should().BeNull();
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
    public void Build_IInvocationOperation()
    {
        using var typeLoader = new TypeLoader();
        var type = typeLoader.LoadLatest().Single(x => x.Type.Name == nameof(IInvocationOperation));
        var model = ModelBuilder.Build([type], []);
        model[type.Type].Should().BeOfType<IOperationStrategy>()
            .Which.Members.Select(x => x.Member.ToString()).Should().BeEquivalentTo([
                "Microsoft.CodeAnalysis.IMethodSymbol TargetMethod",
                "Microsoft.CodeAnalysis.ITypeSymbol ConstrainedToType",
                "Microsoft.CodeAnalysis.IOperation Instance",
                "System.Boolean IsVirtual",
                "System.Collections.Immutable.ImmutableArray`1[Microsoft.CodeAnalysis.Operations.IArgumentOperation] Arguments",
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
        model[type].Should().BeOfType<SyntaxNodeWrapStrategy>()
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
        model[latest.Type].Should().BeOfType<SyntaxNodeExtendStrategy>()
            .Which.Members.Select(x => x.ToString()).Should().BeEquivalentTo([
                "System.Boolean IsIncrementallyIdenticalTo(Microsoft.CodeAnalysis.SyntaxNode)",
                "System.Boolean ContainsDirective(System.Int32)",
                "TNode FirstAncestorOrSelf[TNode,TArg](System.Func`3[TNode,TArg,System.Boolean], TArg, System.Boolean)"]);
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
        model[type].Should().BeOfType<SyntaxNodeWrapStrategy>().Which.Members.Should().BeEmpty();
    }
}
