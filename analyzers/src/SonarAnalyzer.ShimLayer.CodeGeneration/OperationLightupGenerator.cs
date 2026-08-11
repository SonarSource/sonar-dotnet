// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Generator]
    internal sealed class OperationLightupGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var operationInterfacesFiles = context.AdditionalTextsProvider.Where(static x => Path.GetFileName(x.Path) == "OperationInterfaces.xml");
            context.RegisterSourceOutput(operationInterfacesFiles, this.Execute);
        }

        private void Execute(SourceProductionContext context, AdditionalText operationInterfacesFile)
        {
            var operationInterfacesText = operationInterfacesFile.GetText(context.CancellationToken);
            if (operationInterfacesText is null)
            {
                throw new InvalidOperationException("Failed to read OperationInterfaces.xml");
            }

            var operationInterfaces = XDocument.Parse(operationInterfacesText.ToString());
            this.GenerateOperationInterfaces(in context, operationInterfaces);
        }

        private void GenerateOperationInterfaces(in SourceProductionContext context, XDocument operationInterfaces)
        {
            var tree = operationInterfaces.XPathSelectElement("/Tree");
            if (tree is null)
            {
                throw new InvalidOperationException("Failed to find the IOperation root.");
            }

            var documentData = new DocumentData(operationInterfaces);
            this.GenerateOperationWrapperHelper(in context, documentData.Interfaces.Values.ToImmutableArray());
        }

        private void GenerateOperationWrapperHelper(in SourceProductionContext context, ImmutableArray<InterfaceData> wrapperTypes)
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

            // var codeAnalysisAssembly = typeof(SyntaxNode).GetTypeInfo().Assembly;
            // var builder = ImmutableDictionary.CreateBuilder<Type, Type>();
            var staticCtorStatements = SyntaxFactory.List<StatementSyntax>()
                .Add(SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                    type: SyntaxFactory.IdentifierName("var"),
                    variables: SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(
                        identifier: SyntaxFactory.Identifier("codeAnalysisAssembly"),
                        argumentList: null,
                        initializer: SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        expression: SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName("SyntaxNode")),
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

            foreach (var node in wrapperTypes)
            {
                // For the base IOperation node:
                //   builder.Add(typeof(IArgumentOperationWrapper), typeof(IOperation));
                //
                // For all other nodes:
                //   builder.Add(typeof(IArgumentOperationWrapper), codeAnalysisAssembly.GetType(IArgumentOperationWrapper.WrappedTypeName));
                ArgumentSyntax typeArgument;
                if (node.InterfaceName == "IOperation")
                {
                    typeArgument = SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName("IOperation")));
                }
                else
                {
                    typeArgument = SyntaxFactory.Argument(
                        SyntaxFactory.InvocationExpression(
                            expression: SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: SyntaxFactory.IdentifierName("codeAnalysisAssembly"),
                                name: SyntaxFactory.IdentifierName("GetType")),
                            argumentList: SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    expression: SyntaxFactory.IdentifierName(node.WrapperName),
                                    name: SyntaxFactory.IdentifierName("WrappedTypeName")))))));
                }

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
                                    typeArgument,
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
                            name: SyntaxFactory.IdentifierName("ToImmutable"))))));

            var staticCtor = SyntaxFactory.ConstructorDeclaration(
                attributeLists: default,
                modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
                identifier: SyntaxFactory.Identifier("OperationWrapperHelper"),
                parameterList: SyntaxFactory.ParameterList(),
                initializer: null,
                body: SyntaxFactory.Block(staticCtorStatements),
                expressionBody: null);

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
                modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)), // Sonar
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
                modifiers: SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)).Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)), // Sonar
                identifier: SyntaxFactory.Identifier("OperationWrapperHelper"),
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
                    .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.CodeAnalysis"))),
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

            context.AddSource("OperationWrapperHelper.g.cs", wrapperNamespace.GetText(Encoding.UTF8));
        }

        private void GenerateOperationKindEx(in SourceProductionContext context, ImmutableArray<InterfaceData> wrapperTypes)
        {
            var operationKinds = wrapperTypes
                .SelectMany(type => type.OperationKinds)
                .OrderBy(kind => kind.value)
                .ToImmutableArray();

            var members = SyntaxFactory.List<MemberDeclarationSyntax>();
            foreach (var operationKind in operationKinds)
            {
                // public const OperationKind FieldReference = (OperationKind)26;
                members = members.Add(SyntaxFactory.FieldDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ConstKeyword)),
                    declaration: SyntaxFactory.VariableDeclaration(
                        type: SyntaxFactory.IdentifierName("OperationKind"),
                        variables: SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(
                            identifier: SyntaxFactory.Identifier(operationKind.name),
                            argumentList: null,
                            initializer: SyntaxFactory.EqualsValueClause(SyntaxFactory.CastExpression(
                                type: SyntaxFactory.IdentifierName("OperationKind"),
                                expression: SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal($"0x{operationKind.value:x}", operationKind.value)))))))));
            }

            var operationKindExClass = SyntaxFactory.ClassDeclaration(
                attributeLists: default,
                modifiers: SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)).Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)), // Sonar
                identifier: SyntaxFactory.Identifier("OperationKindEx"),
                typeParameterList: null,
                baseList: null,
                constraintClauses: default,
                members: members);
            var wrapperNamespace = SyntaxFactory.NamespaceDeclaration(
                name: SyntaxFactory.ParseName("StyleCop.Analyzers.Lightup"),
                externs: default,
                usings: SyntaxFactory.List<UsingDirectiveSyntax>()
                    .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.CodeAnalysis"))),
                members: SyntaxFactory.SingletonList<MemberDeclarationSyntax>(operationKindExClass));

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

            context.AddSource("OperationKindEx.g.cs", wrapperNamespace.GetText(Encoding.UTF8));
        }

        private sealed class DocumentData
        {
            public DocumentData(XDocument document)
            {
                var operationKinds = GetOperationKinds(document);

                var interfaces = new Dictionary<string, InterfaceData>();
                foreach (var node in document.XPathSelectElements("/Tree/AbstractNode"))
                {
                    if (node.Attribute("Internal")?.Value == "true")
                    {
                        continue;
                    }

                    if (!operationKinds.TryGetValue(node.RequiredAttribute("Name").Value, out var kinds))
                    {
                        kinds = ImmutableArray<(string name, int value, string? extraDescription)>.Empty;
                    }

                    var interfaceData = new InterfaceData(this, node, kinds);
                    interfaces.Add(interfaceData.InterfaceName, interfaceData);
                }

                foreach (var node in document.XPathSelectElements("/Tree/Node"))
                {
                    if (node.Attribute("Internal")?.Value == "true")
                    {
                        continue;
                    }

                    if (!operationKinds.TryGetValue(node.RequiredAttribute("Name").Value, out var kinds))
                    {
                        kinds = ImmutableArray<(string name, int value, string? extraDescription)>.Empty;
                    }

                    var interfaceData = new InterfaceData(this, node, kinds);
                    interfaces.Add(interfaceData.InterfaceName, interfaceData);
                }

                this.Interfaces = new ReadOnlyDictionary<string, InterfaceData>(interfaces);
            }

            public ReadOnlyDictionary<string, InterfaceData> Interfaces { get; }

            private static ImmutableDictionary<string, ImmutableArray<(string name, int value, string? extraDescription)>> GetOperationKinds(XDocument document)
            {
                var skippedOperationKinds = GetSkippedOperationKinds(document);

                var builder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<(string name, int value, string? extraDescription)>>();

                int operationKind = 0;
                foreach (var node in document.XPathSelectElements("/Tree/AbstractNode|/Tree/Node"))
                {
                    if (node.Attribute("Internal")?.Value == "true")
                    {
                        continue;
                    }

                    if (node.XPathSelectElement("OperationKind") is { } explicitKind)
                    {
                        if (node.Name == "AbstractNode" && explicitKind.Attribute("Include")?.Value != "true")
                        {
                            continue;
                        }
                        else if (explicitKind.Attribute("Include")?.Value == "false")
                        {
                            // The node is explicitly excluded
                            continue;
                        }
                        else if (explicitKind.XPathSelectElements("Entry").Any())
                        {
                            var nodeBuilder = ImmutableArray.CreateBuilder<(string name, int value, string? extraDescription)>();
                            foreach (var entry in explicitKind.XPathSelectElements("Entry"))
                            {
                                if (entry.Attribute("EditorBrowsable")?.Value == "false")
                                {
                                    // Skip code generation for this operation kind
                                    continue;
                                }

                                int parsedValue = ParsePrefixHexValue(entry.RequiredAttribute("Value").Value);
                                nodeBuilder.Add((entry.RequiredAttribute("Name").Value, parsedValue, entry.Attribute("ExtraDescription")?.Value));
                            }

                            builder.Add(node.RequiredAttribute("Name").Value, nodeBuilder.ToImmutable());
                            continue;
                        }
                    }
                    else if (node.Name == "AbstractNode")
                    {
                        // Abstract nodes without explicit Include="true" are skipped
                        continue;
                    }

                    // Implicit operation kind
                    operationKind++;
                    while (skippedOperationKinds.Contains(operationKind))
                    {
                        operationKind++;
                    }

                    var nodeName = node.RequiredAttribute("Name").Value;
                    var kindName = nodeName.Substring("I".Length, nodeName.Length - "I".Length - "Operation".Length);
                    builder.Add(nodeName, ImmutableArray.Create((kindName, operationKind, (string?)null)));
                }

                return builder.ToImmutable();
            }

            private static ImmutableHashSet<int> GetSkippedOperationKinds(XDocument document)
            {
                var builder = ImmutableHashSet.CreateBuilder<int>();
                foreach (var skippedKind in document.XPathSelectElements("/Tree/UnusedOperationKinds/Entry"))
                {
                    builder.Add(ParsePrefixHexValue(skippedKind.RequiredAttribute("Value").Value));
                }

                foreach (var explicitKind in document.XPathSelectElements("/Tree/*/OperationKind/Entry"))
                {
                    builder.Add(ParsePrefixHexValue(explicitKind.RequiredAttribute("Value").Value));
                }

                return builder.ToImmutable();
            }

            private static int ParsePrefixHexValue(string value)
            {
                if (!value.StartsWith("0x"))
                {
                    throw new InvalidOperationException($"Unexpected number format: '{value}'");
                }

                return int.Parse(value.Substring("0x".Length), NumberStyles.AllowHexSpecifier);
            }
        }

        private sealed class InterfaceData
        {
            private readonly DocumentData documentData;

            public InterfaceData(DocumentData documentData, XElement node, ImmutableArray<(string name, int value, string? extraDescription)> operationKinds)
            {
                this.documentData = documentData;

                this.OperationKinds = operationKinds;
                this.InterfaceName = node.RequiredAttribute("Name").Value;

                if (node.Attribute("Namespace") is { } namespaceNode)
                {
                    if (namespaceNode.Value == string.Empty)
                    {
                        this.Namespace = "Microsoft.CodeAnalysis";
                    }
                    else
                    {
                        this.Namespace = $"Microsoft.CodeAnalysis.{namespaceNode.Value}";
                    }
                }
                else
                {
                    this.Namespace = "Microsoft.CodeAnalysis.Operations";
                }

                this.Name = this.InterfaceName.Substring("I".Length, this.InterfaceName.Length - "I".Length - "Operation".Length);
                this.WrapperName = this.InterfaceName + "Wrapper";
                this.BaseInterfaceName = node.Attribute("Base")?.Value;
                this.IsAbstract = node.Name == "AbstractNode";
                this.Properties = node.XPathSelectElements("Property").Select(property => new PropertyData(property)).ToImmutableArray();
            }

            public ImmutableArray<(string name, int value, string? extraDescription)> OperationKinds { get; }

            public string InterfaceName { get; }

            public string Namespace { get; }

            public string Name { get; }

            public string WrapperName { get; }

            public string? BaseInterfaceName { get; }

            public bool IsAbstract { get; }

            public ImmutableArray<PropertyData> Properties { get; }

            public InterfaceData? BaseInterface
            {
                get
                {
                    if (this.BaseInterfaceName is not null
                        && this.documentData.Interfaces.TryGetValue(this.BaseInterfaceName, out var baseInterface))
                    {
                        return baseInterface;
                    }

                    return null;
                }
            }

            public IEnumerable<InterfaceData> InheritedInterfaces
            {
                get
                {
                    var inheritedInterfaces = new List<InterfaceData>();
                    for (var baseDefinition = this.BaseInterface; baseDefinition is not null; baseDefinition = baseDefinition.BaseInterface)
                    {
                        inheritedInterfaces.Add(baseDefinition);
                    }

                    inheritedInterfaces.Reverse();
                    return inheritedInterfaces;
                }
            }
        }

        private sealed class PropertyData
        {
            public PropertyData(XElement node)
            {
                this.Name = node.RequiredAttribute("Name").Value;
                this.AccessorName = this.Name + "Accessor";
                this.Type = node.RequiredAttribute("Type").Value;
                this.TypeNonNullable = Type.TrimEnd('?'); // Sonar - When comparing types as strings, the nullable suffix should be ignored.
                this.WrappedType = TypeNonNullable + "Wrapper"; // Sonar

                this.IsNew = node.Attribute("New")?.Value == "true";
                this.IsPublicProperty = node.Attribute("Internal")?.Value != "true";
                this.IsOverride = node.Attribute("Override")?.Value == "true"; // Sonar

                this.IsSkipped = this.TypeNonNullable switch // Sonar
                {
                    "ArgumentKind" => true,
                    "BranchKind" => true,
                    "CaseKind" => true,
                    "CommonConversion" => true,
                    "ForEachLoopOperationInfo" => true,
                    "IDiscardSymbol" => true,
                    "InstanceReferenceKind" => true,
                    "InterpolatedStringArgumentPlaceholderKind" => true,    // Sonar: Skipped because it's not available
                    "LoopKind" => true,
                    "PlaceholderKind" => true,
                    _ => !this.IsPublicProperty || this.IsOverride, // Sonar
                };

                this.NeedsAccessor = this.Name switch
                {
                    nameof(IOperation.Kind) => false,
                    nameof(IOperation.Syntax) => false,
                    nameof(IOperation.Type) => false,
                    nameof(IOperation.ConstantValue) => false,
                    _ => true,
                };
                this.NeedsWrapper = IsAnyOperation(TypeNonNullable) && TypeNonNullable != "IOperation"; // Sonar
                this.IsDerivedOperationArray = IsAnyOperationArray(TypeNonNullable) && TypeNonNullable != "ImmutableArray<IOperation>"; // Sonar

                if (this.IsDerivedOperationArray)
                {
                    this.AccessorResultType = SyntaxFactory.GenericName(
                        identifier: SyntaxFactory.Identifier("ImmutableArray"),
                        typeArgumentList: SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName("IOperation"))));
                }
                else if (IsAnyOperation(TypeNonNullable)) // Sonar
                {
                    this.AccessorResultType = SyntaxFactory.IdentifierName("IOperation");
                }
                else
                {
                    this.AccessorResultType = SyntaxFactory.ParseTypeName(this.Type);
                }
            }

            public bool IsNew { get; }

            public bool IsPublicProperty { get; }

            public bool IsOverride { get; } // Added by Sonar. Usages are also by Sonar.

            public bool IsSkipped { get; }

            public string Name { get; }

            public string AccessorName { get; }

            public string Type { get; }

            public bool NeedsAccessor { get; }

            public string TypeNonNullable { get; } // Sonar

            public string WrappedType { get; } // Sonar

            public bool NeedsWrapper { get; }

            public bool IsDerivedOperationArray { get; }

            public TypeSyntax AccessorResultType { get; }

            private static bool IsAnyOperation(string type)
            {
                return type.StartsWith("I") && type.EndsWith("Operation");
            }

            private static bool IsAnyOperationArray(string type)
            {
                return type.StartsWith("ImmutableArray<I") && type.EndsWith("Operation>");
            }
        }
    }
}
