// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Analyzer.Utilities;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Generator]
    internal sealed class SyntaxLightupGenerator : IIncrementalGenerator
    {
        private enum NodeKind
        {
            Predefined,
            Abstract,
            Concrete,
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var referencedAssemblies = context.CompilationProvider.SelectMany(
                static (compilation, cancellationToken) =>
                {
                    return compilation.SourceModule.ReferencedAssemblySymbols
                        .Where(reference => reference.Name is "Microsoft.CodeAnalysis" or "Microsoft.CodeAnalysis.CSharp");
                });

            var existingTypesInReferences = referencedAssemblies.SelectMany(
                static (reference, cancellationToken) =>
                {
                    var microsoftNamespace = reference.GlobalNamespace.GetNamespaceMembers().SingleOrDefault(symbol => symbol.Name == nameof(Microsoft));
                    var microsoftCodeAnalysisNamespace = microsoftNamespace?.GetNamespaceMembers().SingleOrDefault(symbol => symbol.Name == nameof(Microsoft.CodeAnalysis));
                    var microsoftCodeAnalysisCSharpNamespace = microsoftCodeAnalysisNamespace?.GetNamespaceMembers().SingleOrDefault(symbol => symbol.Name == nameof(Microsoft.CodeAnalysis.CSharp));
                    var microsoftCodeAnalysisCSharpSyntaxNamespace = microsoftCodeAnalysisCSharpNamespace?.GetNamespaceMembers().SingleOrDefault(symbol => symbol.Name == nameof(Microsoft.CodeAnalysis.CSharp.Syntax));

                    var existingTypesBuilder = ImmutableArray.CreateBuilder<ExistingTypeData>();
                    AddPublicTypesFromNamespace(microsoftNamespace, existingTypesBuilder);
                    AddPublicTypesFromNamespace(microsoftCodeAnalysisNamespace, existingTypesBuilder);
                    AddPublicTypesFromNamespace(microsoftCodeAnalysisCSharpNamespace, existingTypesBuilder);
                    AddPublicTypesFromNamespace(microsoftCodeAnalysisCSharpSyntaxNamespace, existingTypesBuilder);

                    return existingTypesBuilder.ToImmutable();

                    static void AddPublicTypesFromNamespace(INamespaceSymbol? namespaceSymbol, ImmutableArray<ExistingTypeData>.Builder existingTypesBuilder)
                    {
                        if (namespaceSymbol is null)
                        {
                            return;
                        }

                        foreach (var type in namespaceSymbol.GetTypeMembers())
                        {
                            if (type is not { DeclaredAccessibility: Accessibility.Public })
                            {
                                continue;
                            }

                            existingTypesBuilder.Add(ExistingTypeData.FromNamedType(type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                        }
                    }
                });

            var compilationData = existingTypesInReferences.Collect().Select(
                static (existingTypes, cancellationToken) =>
                {
                    return new CompilationData(
                        ExistingTypesWrapper: new EquatableValue<ImmutableDictionary<string, ExistingTypeData>>(
                            existingTypes.ToImmutableDictionary(type => type.TypeName),
                            ImmutableDictionaryEqualityComparer<string, ExistingTypeData>.Default));
                });

            var syntaxFiles = context.AdditionalTextsProvider.Where(static x => Path.GetFileName(x.Path) == "Syntax.xml");
            context.RegisterSourceOutput(
                syntaxFiles.Combine(compilationData),
                (context, value) => this.Execute(in context, value.Right, value.Left));
        }

        private void Execute(in SourceProductionContext context, CompilationData compilationData, AdditionalText syntaxFile)
        {
            var syntaxText = syntaxFile.GetText(context.CancellationToken);
            if (syntaxText is null)
            {
                throw new InvalidOperationException("Failed to read Syntax.xml");
            }

            var syntaxData = new SyntaxData(compilationData, XDocument.Parse(syntaxText.ToString()));
            this.GenerateSyntaxWrapperHelper(in context, syntaxData.Nodes);
        }

        private void GenerateSyntaxWrapperHelper(in SourceProductionContext context, ImmutableArray<NodeData> wrapperTypes)
        {
            // private static readonly ImmutableDictionary<Type, Type> WrappedTypes;
            var wrappedTypes = SyntaxFactory.FieldDeclaration(
                attributeLists: default,
                modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                declaration: SyntaxFactory.VariableDeclaration(
                    type: SyntaxFactory.GenericName(
                        identifier: SyntaxFactory.Identifier("ImmutableDictionary"),
                        typeArgumentList: SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                new[]
                                {
                                    SyntaxFactory.IdentifierName("Type"),
                                    SyntaxFactory.IdentifierName("Type"),
                                }))),
                    variables: SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("WrappedTypes")))));

            // var csharpCodeAnalysisAssembly = typeof(CSharpSyntaxNode).GetTypeInfo().Assembly;
            // var builder = ImmutableDictionary.CreateBuilder<Type, Type>();
            var staticCtorStatements = SyntaxFactory.List<StatementSyntax>()
                .Add(SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                    type: SyntaxFactory.IdentifierName("var"),
                    variables: SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(
                        identifier: SyntaxFactory.Identifier("csharpCodeAnalysisAssembly"),
                        argumentList: null,
                        initializer: SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        expression: SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName("CSharpSyntaxNode")),
                                        name: SyntaxFactory.IdentifierName("GetTypeInfo"))),
                                name: SyntaxFactory.IdentifierName("Assembly"))))))))
                .Add(SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                    type: SyntaxFactory.IdentifierName("var"),
                    variables: SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(
                        identifier: SyntaxFactory.Identifier("builder"),
                        argumentList: null,
                        initializer: SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    expression: SyntaxFactory.IdentifierName("ImmutableDictionary"),
                                    name: SyntaxFactory.GenericName(
                                        identifier: SyntaxFactory.Identifier("CreateBuilder"),
                                        typeArgumentList: SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                                new[]
                                                {
                                                    SyntaxFactory.IdentifierName("Type"),
                                                    SyntaxFactory.IdentifierName("Type"),
                                                })))))))))));

            foreach (var node in wrapperTypes.OrderBy(node => node.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (node.WrapperName is null || node.Name is
                    nameof(AnonymousFunctionExpressionSyntax)
                    or nameof(ClassDeclarationSyntax)
                    or nameof(LambdaExpressionSyntax)
                    or nameof(ParenthesizedLambdaExpressionSyntax)
                    or nameof(SimpleLambdaExpressionSyntax)
                    or nameof(StructDeclarationSyntax))
                {
                    continue;
                }

                if (node.Name == nameof(CommonForEachStatementSyntax))
                {
                    // Prior to C# 7, ForEachStatementSyntax was the base type for all foreach statements. If
                    // the CommonForEachStatementSyntax type isn't found at runtime, we fall back to using this type instead.
                    //
                    // var forEachStatementSyntaxType = csharpCodeAnalysisAssembly.GetType(CommonForEachStatementSyntaxWrapper.WrappedTypeName)
                    //     ?? csharpCodeAnalysisAssembly.GetType(CommonForEachStatementSyntaxWrapper.FallbackWrappedTypeName);
                    staticCtorStatements = staticCtorStatements.Add(
                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                            type: SyntaxFactory.IdentifierName("var"),
                            variables: SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(
                                identifier: SyntaxFactory.Identifier("forEachStatementSyntaxType"),
                                argumentList: null,
                                initializer: SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.CoalesceExpression,
                                        left: SyntaxFactory.InvocationExpression(
                                            expression: SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                expression: SyntaxFactory.IdentifierName("csharpCodeAnalysisAssembly"),
                                                name: SyntaxFactory.IdentifierName("GetType")),
                                            argumentList: SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    expression: SyntaxFactory.IdentifierName(node.WrapperName),
                                                    name: SyntaxFactory.IdentifierName("WrappedTypeName")))))),
                                        right: SyntaxFactory.InvocationExpression(
                                            expression: SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                expression: SyntaxFactory.IdentifierName("csharpCodeAnalysisAssembly"),
                                                name: SyntaxFactory.IdentifierName("GetType")),
                                            argumentList: SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    expression: SyntaxFactory.IdentifierName(node.WrapperName),
                                                    name: SyntaxFactory.IdentifierName("FallbackWrappedTypeName")))))))))))));

                    // builder.Add(typeof(CommonForEachStatementSyntaxWrapper), forEachStatementSyntaxType);
                    staticCtorStatements = staticCtorStatements.Add(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            expression: SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: SyntaxFactory.IdentifierName("builder"),
                                name: SyntaxFactory.IdentifierName("Add")),
                            argumentList: SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(
                                    new[]
                                    {
                                        SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(node.WrapperName))),
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("forEachStatementSyntaxType")),
                                    })))));

                    continue;
                }

                if (node.Name == nameof(BaseObjectCreationExpressionSyntax))
                {
                    // Prior to C# 9, ObjectCreationExpressionSyntax was the base type for all object creation
                    // statements. If the BaseObjectCreationExpressionSyntax type isn't found at runtime, we fall back
                    // to using this type instead.
                    //
                    // var objectCreationExpressionSyntaxType = csharpCodeAnalysisAssembly.GetType(BaseObjectCreationExpressionSyntaxWrapper.WrappedTypeName)
                    //     ?? csharpCodeAnalysisAssembly.GetType(BaseObjectCreationExpressionSyntaxWrapper.FallbackWrappedTypeName);
                    LocalDeclarationStatementSyntax localStatement =
                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                            type: SyntaxFactory.IdentifierName("var"),
                            variables: SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(
                                identifier: SyntaxFactory.Identifier("objectCreationExpressionSyntaxType"),
                                argumentList: null,
                                initializer: SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.CoalesceExpression,
                                        left: SyntaxFactory.InvocationExpression(
                                            expression: SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                expression: SyntaxFactory.IdentifierName("csharpCodeAnalysisAssembly"),
                                                name: SyntaxFactory.IdentifierName("GetType")),
                                            argumentList: SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    expression: SyntaxFactory.IdentifierName(node.WrapperName),
                                                    name: SyntaxFactory.IdentifierName("WrappedTypeName")))))),
                                        right: SyntaxFactory.InvocationExpression(
                                            expression: SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                expression: SyntaxFactory.IdentifierName("csharpCodeAnalysisAssembly"),
                                                name: SyntaxFactory.IdentifierName("GetType")),
                                            argumentList: SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    expression: SyntaxFactory.IdentifierName(node.WrapperName),
                                                    name: SyntaxFactory.IdentifierName("FallbackWrappedTypeName"))))))))))));

                    // This is the first line of the statements that initialize 'builder', so start it with a blank line
                    staticCtorStatements = staticCtorStatements.Add(localStatement.WithLeadingBlankLine());

                    // builder.Add(typeof(BaseObjectCreationExpressionSyntaxWrapper), objectCreationExpressionSyntaxType);
                    staticCtorStatements = staticCtorStatements.Add(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            expression: SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: SyntaxFactory.IdentifierName("builder"),
                                name: SyntaxFactory.IdentifierName("Add")),
                            argumentList: SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(
                                    new[]
                                    {
                                        SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(node.WrapperName))),
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("objectCreationExpressionSyntaxType")),
                                    })))));

                    continue;
                }

                if (node.Name == nameof(BaseNamespaceDeclarationSyntax))
                {
                    // Prior to C# 10, NamespaceDeclarationSyntax was the base type for all namespace declarations.
                    // If the BaseNamespaceDeclarationSyntax type isn't found at runtime, we fall back
                    // to using this type instead.
                    //
                    // var baseNamespaceDeclarationSyntaxType = csharpCodeAnalysisAssembly.GetType(BaseNamespaceDeclarationSyntaxWrapper.WrappedTypeName)
                    //     ?? csharpCodeAnalysisAssembly.GetType(BaseNamespaceDeclarationSyntaxWrapper.WrappedTypeName);
                    LocalDeclarationStatementSyntax localStatement =
                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                            type: SyntaxFactory.IdentifierName("var"),
                            variables: SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(
                                identifier: SyntaxFactory.Identifier("baseNamespaceDeclarationSyntaxType"),
                                argumentList: null,
                                initializer: SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.CoalesceExpression,
                                        left: SyntaxFactory.InvocationExpression(
                                            expression: SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                expression: SyntaxFactory.IdentifierName("csharpCodeAnalysisAssembly"),
                                                name: SyntaxFactory.IdentifierName("GetType")),
                                            argumentList: SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    expression: SyntaxFactory.IdentifierName(node.WrapperName),
                                                    name: SyntaxFactory.IdentifierName("WrappedTypeName")))))),
                                        right: SyntaxFactory.InvocationExpression(
                                            expression: SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                expression: SyntaxFactory.IdentifierName("csharpCodeAnalysisAssembly"),
                                                name: SyntaxFactory.IdentifierName("GetType")),
                                            argumentList: SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    expression: SyntaxFactory.IdentifierName(node.WrapperName),
                                                    name: SyntaxFactory.IdentifierName("FallbackWrappedTypeName"))))))))))));

                    // This is the first line of the statements that initialize 'builder', so start it with a blank line
                    staticCtorStatements = staticCtorStatements.Add(localStatement.WithLeadingBlankLine());

                    // builder.Add(typeof(BaseNamespaceDeclarationSyntaxWrapper), baseNamespaceDeclarationSyntaxType);
                    staticCtorStatements = staticCtorStatements.Add(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            expression: SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: SyntaxFactory.IdentifierName("builder"),
                                name: SyntaxFactory.IdentifierName("Add")),
                            argumentList: SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(
                                    new[]
                                    {
                                        SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(node.WrapperName))),
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("baseNamespaceDeclarationSyntaxType")),
                                    })))));

                    continue;
                }

                // builder.Add(typeof(ConstantPatternSyntaxWrapper), csharpCodeAnalysisAssembly.GetType(ConstantPatternSyntaxWrapper.WrappedTypeName));
                staticCtorStatements = staticCtorStatements.Add(SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        expression: SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: SyntaxFactory.IdentifierName("builder"),
                            name: SyntaxFactory.IdentifierName("Add")),
                        argumentList: SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                new[]
                                {
                                    SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(node.WrapperName))),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.InvocationExpression(
                                            expression: SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                expression: SyntaxFactory.IdentifierName("csharpCodeAnalysisAssembly"),
                                                name: SyntaxFactory.IdentifierName("GetType")),
                                            argumentList: SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    expression: SyntaxFactory.IdentifierName(node.WrapperName),
                                                    name: SyntaxFactory.IdentifierName("WrappedTypeName"))))))),
                                })))));
            }

            // WrappedTypes = builder.ToImmutable();
            staticCtorStatements = staticCtorStatements.Add(SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    left: SyntaxFactory.IdentifierName("WrappedTypes"),
                    right: SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: SyntaxFactory.IdentifierName("builder"),
                            name: SyntaxFactory.IdentifierName("ToImmutable"))))).WithLeadingBlankLine());

            var staticCtor = SyntaxFactory.ConstructorDeclaration(
                attributeLists: default,
                modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
                identifier: SyntaxFactory.Identifier("SyntaxWrapperHelper"),
                parameterList: SyntaxFactory.ParameterList(),
                initializer: null,
                body: SyntaxFactory.Block(staticCtorStatements),
                expressionBody: null).WithLeadingBlankLine();

            // /// <summary>
            // /// Gets the type that is wrapped by the given wrapper.
            // /// </summary>
            // /// <param name="wrapperType">Type of the wrapper for which the wrapped type should be retrieved.</param>
            // /// <returns>The wrapped type, or null if there is no info.</returns>
            // internal static Type GetWrappedType(Type wrapperType)
            // {
            //     if (WrappedTypes.TryGetValue(wrapperType, out Type wrappedType))
            //     {
            //         return wrappedType;
            //     }
            //
            //     return null;
            // }
            var getWrappedType = SyntaxFactory.MethodDeclaration(
                attributeLists: default,
                modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)), // Sonar: changed to PublicKeyword
                returnType: SyntaxFactory.IdentifierName("Type"),
                explicitInterfaceSpecifier: null,
                identifier: SyntaxFactory.Identifier("GetWrappedType"),
                typeParameterList: null,
                parameterList: SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: SyntaxFactory.IdentifierName("Type"),
                    identifier: SyntaxFactory.Identifier("wrapperType"),
                    @default: null))),
                constraintClauses: default,
                body: SyntaxFactory.Block(
                    SyntaxFactory.IfStatement(
                        condition: SyntaxFactory.InvocationExpression(
                            expression: SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: SyntaxFactory.IdentifierName("WrappedTypes"),
                                name: SyntaxFactory.IdentifierName("TryGetValue")),
                            argumentList: SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                                new[]
                                {
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("wrapperType")),
                                    SyntaxFactory.Argument(
                                        nameColon: null,
                                        refKindKeyword: SyntaxFactory.Token(SyntaxKind.OutKeyword),
                                        expression: SyntaxFactory.DeclarationExpression(
                                            type: SyntaxFactory.IdentifierName("Type"),
                                            designation: SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier("wrappedType")))),
                                }))),
                        statement: SyntaxFactory.Block(
                            SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("wrappedType")))),
                    SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))),
                expressionBody: null);

            getWrappedType = getWrappedType.WithLeadingTrivia(SyntaxFactory.TriviaList(
                SyntaxFactory.Trivia(SyntaxFactory.DocumentationComment(
                    SyntaxFactory.XmlText(" "),
                    SyntaxFactory.XmlSummaryElement(
                        SyntaxFactory.XmlText(XmlSyntaxFactory.XmlCarriageReturnLineFeedWithContinuation),
                        SyntaxFactory.XmlText(" Gets the type that is wrapped by the given wrapper."),
                        SyntaxFactory.XmlText(XmlSyntaxFactory.XmlCarriageReturnLineFeedWithContinuation),
                        SyntaxFactory.XmlText(" ")),
                    SyntaxFactory.XmlText(XmlSyntaxFactory.XmlCarriageReturnLineFeedWithContinuation),
                    SyntaxFactory.XmlText(" "),
                    SyntaxFactory.XmlParamElement(
                        "wrapperType",
                        SyntaxFactory.XmlText("Type of the wrapper for which the wrapped type should be retrieved.")),
                    SyntaxFactory.XmlText(XmlSyntaxFactory.XmlCarriageReturnLineFeedWithContinuation),
                    SyntaxFactory.XmlText(" "),
                    SyntaxFactory.XmlReturnsElement(
                        SyntaxFactory.XmlText("The wrapped type, or null if there is no info.")),
                    SyntaxFactory.XmlText(XmlSyntaxFactory.XmlCarriageReturnLineFeedWithContinuation).WithoutTrailingTrivia()))));

            var wrapperHelperClass = SyntaxFactory.ClassDeclaration(
                attributeLists: default,
                modifiers: SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)).Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)), // Sonar: changed to PublicKeyword
                identifier: SyntaxFactory.Identifier("SyntaxWrapperHelper"),
                typeParameterList: null,
                baseList: null,
                constraintClauses: default,
                members: SyntaxFactory.List<MemberDeclarationSyntax>()
                    .Add(wrappedTypes)
                    .Add(staticCtor)
                    .Add(getWrappedType));
            var wrapperNamespace = SyntaxFactory.NamespaceDeclaration(
                name: SyntaxFactory.ParseName("StyleCop.Analyzers.Lightup"),
                externs: default,
                usings: SyntaxFactory.List<UsingDirectiveSyntax>()
                    .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                    .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Immutable")))
                    .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Reflection")))
                    .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.CodeAnalysis")))
                    .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.CodeAnalysis.CSharp"))),
                members: SyntaxFactory.SingletonList<MemberDeclarationSyntax>(wrapperHelperClass));

            wrapperNamespace = wrapperNamespace
                .NormalizeWhitespace()
                .WithLeadingTrivia(
                    SyntaxFactory.Comment("// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved."),
                    SyntaxFactory.CarriageReturnLineFeed,
                    SyntaxFactory.Comment("// Licensed under the MIT License. See LICENSE in the project root for license information."),
                    SyntaxFactory.CarriageReturnLineFeed,
                    SyntaxFactory.CarriageReturnLineFeed)
                .WithTrailingTrivia(
                    SyntaxFactory.CarriageReturnLineFeed);

            context.AddSource("SyntaxWrapperHelper.g.cs", wrapperNamespace.GetText(Encoding.UTF8));
        }

        private sealed class SyntaxData
        {
            private readonly Dictionary<string, NodeData> nameToNode;

            public SyntaxData(CompilationData compilationData, XDocument document)
            {
                var nodesBuilder = ImmutableArray.CreateBuilder<NodeData>();
                if (document.XPathSelectElement("/Tree[@Root='SyntaxNode']") is { } tree)
                {
                    foreach (var element in tree.XPathSelectElements("PredefinedNode|AbstractNode|Node"))
                    {
                        nodesBuilder.Add(new NodeData(compilationData, element));
                    }
                }

                this.Nodes = nodesBuilder.ToImmutable();
                this.nameToNode = this.Nodes.ToDictionary(node => node.Name);
            }

            public ImmutableArray<NodeData> Nodes { get; }

            public NodeData? TryGetConcreteType(NodeData? node)
            {
                for (var current = node; current is not null; current = this.TryGetNode(current.BaseName))
                {
                    if (current.WrapperName is null)
                    {
                        // This is not a wrapper
                        return current;
                    }
                }

                return null;
            }

            public NodeData? TryGetConcreteBase(NodeData node)
            {
                return this.TryGetConcreteType(this.TryGetNode(node.BaseName));
            }

            public NodeData? TryGetNode(string name)
            {
                this.nameToNode.TryGetValue(name, out var node);
                return node;
            }
        }

        private sealed class NodeData
        {
            private static readonly ImmutableHashSet<string> ClassesThatShouldBeAlwaysGenerated = ImmutableHashSet.Create<string>( // Sonar
                nameof(AnonymousFunctionExpressionSyntax),
                nameof(LambdaExpressionSyntax),
                nameof(ParenthesizedLambdaExpressionSyntax),
                nameof(SimpleLambdaExpressionSyntax),
                // TypeDeclarationSyntaxWrapper causes compatibility problems with ParameterList because the property was first declared in RecordDeclarationSyntax (C#9) and later
                // moved upwards into TypeDeclarationSyntax (C#12). The forwarding of the accessor in RecordDeclarationSyntax  to the base wrapper does not work for SDK versions
                // .Net5 to .Net7 as the property is not found in TypeDeclarationSyntax.
                // Use the `ParameterList(this TypeDeclarationSyntax)` extension method for a unified access instead of adding TypeDeclarationSyntax here.
                nameof(ClassDeclarationSyntax),
                nameof(StructDeclarationSyntax));

            public NodeData(CompilationData compilationData, XElement element)
            {
                this.Kind = element.Name.LocalName switch
                {
                    "PredefinedNode" => NodeKind.Predefined,
                    "AbstractNode" => NodeKind.Abstract,
                    "Node" => NodeKind.Concrete,
                    _ => throw new NotSupportedException($"Unknown element name '{element.Name}'"),
                };

                this.Name = element.RequiredAttribute("Name").Value;

                this.ExistingType = compilationData.ExistingTypes.GetValueOrDefault($"Microsoft.CodeAnalysis.CSharp.Syntax.{this.Name}")
                    ?? compilationData.ExistingTypes.GetValueOrDefault($"Microsoft.CodeAnalysis.CSharp.{this.Name}")
                    ?? compilationData.ExistingTypes.GetValueOrDefault($"Microsoft.CodeAnalysis.{this.Name}");
                if (this.ExistingType is not null && !ClassesThatShouldBeAlwaysGenerated.Contains(this.Name)) // Sonar
                {
                    this.WrapperName = null;
                }
                else
                {
                    this.WrapperName = this.Name + "Wrapper";
                }

                this.BaseName = element.RequiredAttribute("Base").Value;
                this.Fields = element.XPathSelectElements("descendant::Field").Select(field => new FieldData(this, field)).ToImmutableArray();
            }

            public NodeKind Kind { get; }

            public string Name { get; }

            public ExistingTypeData? ExistingType { get; }

            public string? WrapperName { get; }

            public string BaseName { get; }

            public ImmutableArray<FieldData> Fields { get; }

            internal FieldData? TryGetField(string name)
            {
                return this.Fields.SingleOrDefault(field => field.Name == name);
            }
        }

        private sealed class FieldData
        {
            private readonly NodeData nodeData;

            public FieldData(NodeData nodeData, XElement element)
            {
                this.nodeData = nodeData;

                this.Name = element.RequiredAttribute("Name").Value;

                var type = element.RequiredAttribute("Type").Value;
                this.Type = type switch
                {
                    "SyntaxList<SyntaxToken>" => nameof(SyntaxTokenList),
                    _ => type,
                };

                this.IsOverride = element.Attribute("Override")?.Value == "true";

                this.AccessorName = this.Name + "Accessor";
                this.WithAccessorName = "With" + this.Name + "Accessor";
            }

            public bool IsSkipped => false;

            public string Name { get; }

            public string AccessorName { get; }

            public string WithAccessorName { get; }

            public string Type { get; }

            public bool IsOverride { get; }

            /// <summary>
            /// Gets a value indicating whether this field is implemented as an extension method in the lightup layer.
            /// </summary>
            public bool IsExtensionField
            {
                get
                {
                    return this.nodeData.ExistingType is not null
                        && !this.nodeData.ExistingType.MemberNames.Contains(this.Name);
                }
            }

            public NodeData GetDeclaringNode(SyntaxData syntaxData)
            {
                for (var current = this.nodeData; current is not null; current = syntaxData.TryGetNode(current.BaseName))
                {
                    var currentField = current.TryGetField(this.Name);
                    if (currentField is { IsOverride: false })
                    {
                        return currentField.nodeData;
                    }
                }

                throw new NotSupportedException("Unable to find declaring node.");
            }

            public bool IsWrappedSeparatedSyntaxList(SyntaxData syntaxData, [NotNullWhen(true)] out NodeData? element)
            {
                if (this.Type.StartsWith("SeparatedSyntaxList<") && this.Type.EndsWith(">"))
                {
                    var elementTypeName = this.Type.Substring("SeparatedSyntaxList<".Length, this.Type.Length - "SeparatedSyntaxList<".Length - ">".Length);
                    var elementTypeNode = syntaxData.TryGetNode(elementTypeName);
                    if (elementTypeNode is { WrapperName: not null })
                    {
                        element = elementTypeNode;
                        return true;
                    }
                }

                element = null;
                return false;
            }

            public string GetAccessorResultType(SyntaxData syntaxData)
            {
                var typeNode = syntaxData.TryGetNode(this.Type);
                if (typeNode is not null)
                {
                    return syntaxData.TryGetConcreteType(typeNode)?.Name ?? nameof(SyntaxNode);
                }

                if (this.IsWrappedSeparatedSyntaxList(syntaxData, out var elementTypeNode))
                {
                    return $"SeparatedSyntaxListWrapper<{elementTypeNode.WrapperName}>";
                }

                return this.Type;
            }

            public string? GetAccessorResultElementType(SyntaxData syntaxData)
            {
                if (this.IsWrappedSeparatedSyntaxList(syntaxData, out var elementTypeNode))
                {
                    return elementTypeNode.WrapperName;
                }

                return null;
            }
        }

        private sealed record CompilationData
        {
            public EquatableValue<ImmutableDictionary<string, ExistingTypeData>> ExistingTypesWrapper { get; } // Sonar

            public ImmutableDictionary<string, ExistingTypeData> ExistingTypes => this.ExistingTypesWrapper.Value;

            public CompilationData(EquatableValue<ImmutableDictionary<string, ExistingTypeData>> ExistingTypesWrapper) // Sonar
            {
                this.ExistingTypesWrapper = ExistingTypesWrapper;
            }
        }

        private record class ExistingTypeData
        {
            public string TypeName { get; } // Sonar

            public EquatableValue<ImmutableArray<string>> MemberNamesWrapper { get; } // Sonar

            public ImmutableArray<string> MemberNames => this.MemberNamesWrapper.Value;

            public ExistingTypeData(string TypeName, EquatableValue<ImmutableArray<string>> MemberNamesWrapper) // Sonar
            {
                this.TypeName = TypeName;
                this.MemberNamesWrapper = MemberNamesWrapper;
            }

            public static ExistingTypeData FromNamedType(INamedTypeSymbol namedType, string typeName)
            {
                var memberNames = ImmutableArray.CreateRange(namedType.GetMembers(), member => member.Name);
                return new ExistingTypeData(
                    TypeName: typeName,
                    MemberNamesWrapper: new EquatableValue<ImmutableArray<string>>(memberNames, ImmutableArrayEqualityComparer<string>.Default));
            }
        }

        private sealed class EquatableValue<T> : IEquatable<EquatableValue<T>?>
        {
            public EquatableValue(T value, IEqualityComparer<T> comparer)
            {
                this.Value = value;
                this.Comparer = comparer;
            }

            public T Value { get; }

            public IEqualityComparer<T> Comparer { get; }

            public bool Equals(EquatableValue<T>? other)
            {
                if (other is null)
                {
                    return false;
                }

                return this.Comparer.Equals(this.Value, other.Value);
            }
        }

        private sealed class ImmutableDictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<ImmutableDictionary<TKey, TValue>?>
            where TKey : notnull
        {
            public static ImmutableDictionaryEqualityComparer<TKey, TValue> Default { get; } = new ImmutableDictionaryEqualityComparer<TKey, TValue>();

            public bool Equals(ImmutableDictionary<TKey, TValue>? x, ImmutableDictionary<TKey, TValue>? y)
            {
                if (x == y)
                {
                    return true;
                }
                else if (x is null || y is null)
                {
                    return false;
                }
                else if (x.Count != y.Count)
                {
                    return false;
                }

                var keyEqualityComparer = EqualityComparer<TKey>.Default;
                var valueEqualityComparer = EqualityComparer<TValue>.Default;

                using var first = x.GetEnumerator();
                using var second = y.GetEnumerator();
                while (first.MoveNext() && second.MoveNext())
                {
                    if (!keyEqualityComparer.Equals(first.Current.Key, second.Current.Key)
                        || !valueEqualityComparer.Equals(first.Current.Value, second.Current.Value))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(ImmutableDictionary<TKey, TValue>? obj)
            {
                if (obj is null)
                {
                    return 0;
                }

                var hashCode = default(RoslynHashCode);

                var keyEqualityComparer = EqualityComparer<TKey>.Default;
                var valueEqualityComparer = EqualityComparer<TValue>.Default;
                foreach (var i in obj)
                {
                    hashCode.Add(keyEqualityComparer.GetHashCode(i.Key));
                    hashCode.Add(valueEqualityComparer.GetHashCode(i.Value));
                }

                return hashCode.ToHashCode();
            }
        }

        private sealed class ImmutableArrayEqualityComparer<T> : IEqualityComparer<ImmutableArray<T>>
        {
            public static ImmutableArrayEqualityComparer<T> Default { get; } = new ImmutableArrayEqualityComparer<T>();

            public bool Equals(ImmutableArray<T> x, ImmutableArray<T> y)
            {
                if (x == y)
                {
                    return true;
                }
                else if (x == null || y == null)
                {
                    return false;
                }
                else if (x.Length != y.Length)
                {
                    return false;
                }

                var equalityComparer = EqualityComparer<T>.Default;
                for (var i = 0; i < x.Length; i++)
                {
                    if (!equalityComparer.Equals(x[i], y[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(ImmutableArray<T> obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                var hashCode = default(RoslynHashCode);

                var equalityComparer = EqualityComparer<T>.Default;
                foreach (var i in obj)
                {
                    hashCode.Add(equalityComparer.GetHashCode(i));
                }

                return hashCode.ToHashCode();
            }
        }
    }
}
