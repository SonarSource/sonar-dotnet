// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.CodeGeneration
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [Generator]
    internal sealed class OperationLightupGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var operationInterfacesFile = context.AdditionalFiles.Single(x => Path.GetFileName(x.Path) == "OperationInterfaces.xml");
            var operationInterfacesText = operationInterfacesFile.GetText(context.CancellationToken);
            if (operationInterfacesText is null)
            {
                throw new InvalidOperationException("Failed to read OperationInterfaces.xml");
            }

            var operationInterfaces = XDocument.Parse(operationInterfacesText.ToString());
            this.GenerateOperationInterfaces(in context, operationInterfaces);
        }

        private void GenerateOperationInterfaces(in GeneratorExecutionContext context, XDocument operationInterfaces)
        {
            var tree = operationInterfaces.XPathSelectElement("/Tree");
            if (tree is null)
            {
                throw new InvalidOperationException("Failed to find the IOperation root.");
            }

            var documentData = new DocumentData(operationInterfaces);
            foreach (var pair in documentData.Interfaces)
            {
                this.GenerateOperationInterface(in context, pair.Value);
                this.GenerateOperationInterfaceExtension(in context, pair.Value);
            }

            this.GenerateOperationWrapperHelper(in context, documentData.Interfaces.Values.ToImmutableArray());
            this.GenerateOperationKindEx(in context, documentData.Interfaces.Values.ToImmutableArray());
        }

        private void GenerateOperationInterfaceExtension(in GeneratorExecutionContext context, InterfaceData value)
        {
            var wrapperName = IdentifierName(value.WrapperName);
            var methodName = value.InterfaceName.Substring(1);
            methodName = methodName.Substring(0, methodName.Length - "Operation".Length);
            var publicStatic = TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));

            // public static ILoopOperationWrapper AsLoop(this IOperation operation) => ILoopOperationWrapper.FromOperation(operation);
            var asMethod = MethodDeclaration(wrapperName, Identifier($"As{methodName}"))
                .WithModifiers(publicStatic)
                .WithParameterList(ParameterList(SingletonSeparatedList(
                    Parameter(Identifier("operation")).WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword))).WithType(IdentifierName("IOperation")))))
                .WithExpressionBody(ArrowExpressionClause(
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, wrapperName, IdentifierName("FromOperation")))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("operation")))))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(value.Summary);

            // public static bool TryAsLoop(this IOperation operation, out ILoopOperationWrapper wrapper)
            // {
            //     if (ILoopOperationWrapper.IsInstance(operation))
            //     {
            //         wrapper = ILoopOperationWrapper.FromOperation(operation);
            //         return true;
            //     }
            //     else
            //     {
            //         wrapper = default;
            //         return false;
            //     }
            // }
            var tryAsMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier($"TryAs{methodName}"))
                .WithModifiers(publicStatic)
                .WithParameterList(ParameterList(SeparatedList(new[]
                    {
                        Parameter(Identifier("operation")).WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword))).WithType(IdentifierName("IOperation")),
                        Parameter(Identifier("wrapper")).WithModifiers(TokenList(Token(SyntaxKind.OutKeyword))).WithType(wrapperName)
                    })))
                .WithBody(Block(SingletonList(IfStatement(
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, wrapperName, IdentifierName("IsInstance")))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("operation"))))),
                    Block(
                        ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("wrapper"),
                            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, wrapperName, IdentifierName("FromOperation")))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("operation"))))))),
                        ReturnStatement(LiteralExpression(SyntaxKind.TrueLiteralExpression))))
                .WithElse(ElseClause(Block(
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("wrapper"), LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword)))),
                    ReturnStatement(LiteralExpression(SyntaxKind.FalseLiteralExpression))))))))
                .WithLeadingTrivia(value.Summary);
            var extensionClass = ClassDeclaration($"{value.WrapperName}Extensions").WithModifiers(publicStatic)
                .WithMembers(List(new MemberDeclarationSyntax[] { asMethod, tryAsMethod }));

            var wrapperNamespace = NamespaceDeclaration(
                name: ParseName("StyleCop.Analyzers.Lightup"),
                externs: default,
                usings: List<UsingDirectiveSyntax>()
                    .Add(UsingDirective(ParseName("System")))
                    .Add(UsingDirective(ParseName("System.Collections.Immutable")))
                    .Add(UsingDirective(ParseName("Microsoft.CodeAnalysis"))),
                members: SingletonList<MemberDeclarationSyntax>(extensionClass));
            context.AddSource($"{value.WrapperName}Extensions.g.cs", SourceText.From(wrapperNamespace.NormalizeWhitespace().ToFullString(), Encoding.UTF8));
        }

        private void GenerateOperationInterface(in GeneratorExecutionContext context, InterfaceData node)
        {
            var members = List<MemberDeclarationSyntax>();

            // internal const string WrappedTypeName = "Microsoft.CodeAnalysis.Operations.IArgumentOperation";
            members = members.Add(FieldDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ConstKeyword)),
                declaration: VariableDeclaration(
                    type: PredefinedType(Token(SyntaxKind.StringKeyword)),
                    variables: SingletonSeparatedList(VariableDeclarator(
                        identifier: Identifier("WrappedTypeName"),
                        argumentList: null,
                        initializer: EqualsValueClause(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal($"Microsoft.CodeAnalysis.{node.Namespace}.{node.InterfaceName}"))))))));

            // private static readonly Type WrappedType;
            members = members.Add(FieldDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
                declaration: VariableDeclaration(
                    type: IdentifierName("Type"),
                    variables: SingletonSeparatedList(VariableDeclarator("WrappedType")))));

            foreach (var property in node.Properties)
            {
                if (property.IsSkipped)
                {
                    continue;
                }

                // private static readonly Func<IOperation, IMethodSymbol> ConstructorAccessor;
                members = members.Add(FieldDeclaration(
                    attributeLists: default,
                    modifiers: TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
                    declaration: VariableDeclaration(
                        type: GenericName(
                            identifier: Identifier("Func"),
                            typeArgumentList: TypeArgumentList(SeparatedList(
                                new[]
                                {
                                    IdentifierName("IOperation"),
                                    property.AccessorResultType,
                                }))),
                        variables: SingletonSeparatedList(VariableDeclarator(property.AccessorName)))));
            }

            // private readonly IOperation operation;
            members = members.Add(FieldDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
                declaration: VariableDeclaration(
                    type: IdentifierName("IOperation"),
                    variables: SingletonSeparatedList(VariableDeclarator("operation")))));

            var staticCtorStatements = SingletonList<StatementSyntax>(
                ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    left: IdentifierName("WrappedType"),
                    right: InvocationExpression(
                        expression: MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: IdentifierName("OperationWrapperHelper"),
                            name: IdentifierName("GetWrappedType")),
                        argumentList: ArgumentList(SingletonSeparatedList(Argument(
                            TypeOfExpression(IdentifierName(node.WrapperName)))))))));

            foreach (var property in node.Properties)
            {
                if (property.IsSkipped)
                {
                    continue;
                }

                SimpleNameSyntax helperName;
                if (property.IsDerivedOperationArray)
                {
                    // CreateOperationListPropertyAccessor<IOperation>
                    helperName = GenericName(
                        identifier: Identifier("CreateOperationListPropertyAccessor"),
                        typeArgumentList: TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName("IOperation"))));
                }
                else
                {
                    // CreateOperationPropertyAccessor<IOperation, IMethodSymbol>
                    helperName = GenericName(
                        identifier: Identifier("CreateOperationPropertyAccessor"),
                        typeArgumentList: TypeArgumentList(SeparatedList(
                            new[]
                            {
                                IdentifierName("IOperation"),
                                property.AccessorResultType,
                            })));
                }

                // ConstructorAccessor = LightupHelpers.CreateOperationPropertyAccessor<IOperation, IMethodSymbol>(WrappedType, nameof(Constructor));
                staticCtorStatements = staticCtorStatements.Add(ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    left: IdentifierName(property.AccessorName),
                    right: InvocationExpression(
                        expression: MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: IdentifierName("LightupHelpers"),
                            name: helperName),
                        argumentList: ArgumentList(SeparatedList(
                            new[]
                            {
                                Argument(IdentifierName("WrappedType")),
                                Argument(InvocationExpression(
                                    expression: IdentifierName("nameof"),
                                    argumentList: ArgumentList(SingletonSeparatedList(Argument(IdentifierName(property.Name)))))),
                            }))))));
            }

            // static IArgumentOperationWrapper()
            // {
            //     WrappedType = OperationWrapperHelper.GetWrappedType(typeof(IObjectCreationOperationWrapper));
            // }
            members = members.Add(ConstructorDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.StaticKeyword)),
                identifier: Identifier(node.WrapperName),
                parameterList: ParameterList(),
                initializer: null,
                body: Block(staticCtorStatements),
                expressionBody: null));

            // private IArgumentOperationWrapper(IOperation operation)
            // {
            //     this.operation = operation;
            // }
            members = members.Add(ConstructorDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PrivateKeyword)),
                identifier: Identifier(node.WrapperName),
                parameterList: ParameterList(SingletonSeparatedList(Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: IdentifierName("IOperation"),
                    identifier: Identifier("operation"),
                    @default: null))),
                initializer: null,
                body: Block(
                    ExpressionStatement(AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        left: MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: ThisExpression(),
                            name: IdentifierName("operation")),
                        right: IdentifierName("operation")))),
                expressionBody: null));

            // public IOperation WrappedOperation => this.operation;
            members = members.Add(PropertyDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                type: IdentifierName("IOperation"),
                explicitInterfaceSpecifier: null,
                identifier: Identifier("WrappedOperation"),
                accessorList: null,
                expressionBody: ArrowExpressionClause(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression: ThisExpression(),
                    name: IdentifierName("operation"))),
                initializer: null,
                semicolonToken: Token(SyntaxKind.SemicolonToken)));

            // public ITypeSymbol Type => this.WrappedOperation.Type;
            members = members.Add(PropertyDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                type: IdentifierName("ITypeSymbol"),
                explicitInterfaceSpecifier: null,
                identifier: Identifier("Type"),
                accessorList: null,
                expressionBody: ArrowExpressionClause(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression: MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression: ThisExpression(),
                        name: IdentifierName("WrappedOperation")),
                    name: IdentifierName("Type"))),
                initializer: null,
                semicolonToken: Token(SyntaxKind.SemicolonToken)));

            foreach (var property in node.Properties)
            {
                if (property.IsSkipped)
                {
                    // Generate a NotImplementedException for public properties that do not have a supported type
                    if (property.IsPublicProperty)
                    {
                        // public object Constructor => throw new NotImplementedException("Property 'Type.Property' has unsupported type 'Type'");
                        members = members.Add(PropertyDeclaration(
                            attributeLists: default,
                            modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                            type: PredefinedType(Token(SyntaxKind.ObjectKeyword)),
                            explicitInterfaceSpecifier: null,
                            identifier: Identifier(property.Name),
                            accessorList: null,
                            expressionBody: ArrowExpressionClause(ThrowExpression(ObjectCreationExpression(
                                type: IdentifierName(nameof(NotImplementedException)),
                                argumentList: ArgumentList(SingletonSeparatedList(Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal($"Property '{node.InterfaceName}.{property.Name}' has unsupported type '{property.Type}'"))))),
                                initializer: null))),
                            initializer: null,
                            semicolonToken: Token(SyntaxKind.SemicolonToken))
                            .WithLeadingTrivia(property.Summary));
                    }

                    continue;
                }

                if (property.IsOverride)
                {
                    continue;
                }

                var propertyType = property.NeedsWrapper ? IdentifierName(property.WrapperType) : property.AccessorResultType;
                var propertyTypeWithNullable = property.NeedsWrapper ? IdentifierName(property.WrapperTypeWithNullable) : property.AccessorResultType;

                // ConstructorAccessor(this.WrappedOperation)
                var evaluatedAccessor = InvocationExpression(
                    expression: IdentifierName(property.AccessorName),
                    argumentList: ArgumentList(SingletonSeparatedList(Argument(
                        expression: MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: ThisExpression(),
                            name: IdentifierName("WrappedOperation"))))));

                ExpressionSyntax convertedResult;
                if (property.NeedsWrapper)
                {
                    // IObjectOrCollectionInitializerOperationWrapper.FromOperation(...)
                    convertedResult = InvocationExpression(
                        expression: MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: propertyType,
                            name: IdentifierName("FromOperation")),
                        argumentList: ArgumentList(SingletonSeparatedList(Argument(evaluatedAccessor))));
                }
                else
                {
                    convertedResult = evaluatedAccessor;
                }

                // public IMethodSymbol Constructor => ConstructorAccessor(this.WrappedOperation);
                members = members.Add(PropertyDeclaration(
                    attributeLists: default,
                    modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                    type: propertyTypeWithNullable,
                    explicitInterfaceSpecifier: null,
                    identifier: Identifier(property.Name),
                    accessorList: null,
                    expressionBody: ArrowExpressionClause(convertedResult),
                    initializer: null,
                    semicolonToken: Token(SyntaxKind.SemicolonToken))
                    .WithLeadingTrivia(property.Summary));
            }

            if (node.BaseInterface is { } baseDefinition)
            {
                var inheritedProperties = baseDefinition.Properties;
                foreach (var property in inheritedProperties)
                {
                    if (node.Properties.Any(derivedProperty => derivedProperty.Name == property.Name && derivedProperty.IsNew))
                    {
                        continue;
                    }

                    if (!property.IsPublicProperty)
                    {
                        continue;
                    }

                    var propertyType = property.IsSkipped
                        ? PredefinedType(Token(SyntaxKind.ObjectKeyword))
                        : property.NeedsWrapper ? IdentifierName(property.WrapperTypeWithNullable) : property.AccessorResultType;

                    // public IOperation Instance => ((IMemberReferenceOperationWrapper)this).Instance;
                    members = members.Add(PropertyDeclaration(
                        attributeLists: default,
                        modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                        type: propertyType,
                        explicitInterfaceSpecifier: null,
                        identifier: Identifier(property.Name),
                        accessorList: null,
                        expressionBody: ArrowExpressionClause(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: ParenthesizedExpression(CastExpression(
                                type: IdentifierName(baseDefinition.WrapperName),
                                expression: ThisExpression())),
                            name: IdentifierName(property.Name))),
                        initializer: null,
                        semicolonToken: Token(SyntaxKind.SemicolonToken))
                        .WithLeadingTrivia(property.Summary));
                }

                // public static explicit operator IFieldReferenceOperationWrapper(IMemberReferenceOperationWrapper wrapper)
                //     => FromOperation(wrapper.WrappedOperation);
                members = members.Add(ConversionOperatorDeclaration(
                    attributeLists: default,
                    modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                    implicitOrExplicitKeyword: Token(SyntaxKind.ExplicitKeyword),
                    operatorKeyword: Token(SyntaxKind.OperatorKeyword),
                    type: IdentifierName(node.WrapperName),
                    parameterList: ParameterList(SingletonSeparatedList(Parameter(
                        attributeLists: default,
                        modifiers: default,
                        type: IdentifierName(baseDefinition.WrapperName),
                        identifier: Identifier("wrapper"),
                        @default: null))),
                    body: null,
                    expressionBody: ArrowExpressionClause(InvocationExpression(
                        expression: IdentifierName("FromOperation"),
                        argumentList: ArgumentList(SingletonSeparatedList(Argument(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: IdentifierName("wrapper"),
                                name: IdentifierName("WrappedOperation"))))))),
                    semicolonToken: Token(SyntaxKind.SemicolonToken)));

                // public static implicit operator IMemberReferenceOperationWrapper(IFieldReferenceOperationWrapper wrapper)
                //     => IMemberReferenceOperationWrapper.FromUpcast(wrapper.WrappedOperation);
                members = members.Add(ConversionOperatorDeclaration(
                    attributeLists: default,
                    modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                    implicitOrExplicitKeyword: Token(SyntaxKind.ImplicitKeyword),
                    operatorKeyword: Token(SyntaxKind.OperatorKeyword),
                    type: IdentifierName(baseDefinition.WrapperName),
                    parameterList: ParameterList(SingletonSeparatedList(Parameter(
                        attributeLists: default,
                        modifiers: default,
                        type: IdentifierName(node.WrapperName),
                        identifier: Identifier("wrapper"),
                        @default: null))),
                    body: null,
                    expressionBody: ArrowExpressionClause(InvocationExpression(
                        expression: MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: IdentifierName(baseDefinition.WrapperName),
                            name: IdentifierName("FromUpcast")),
                        argumentList: ArgumentList(SingletonSeparatedList(Argument(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: IdentifierName("wrapper"),
                                name: IdentifierName("WrappedOperation"))))))),
                    semicolonToken: Token(SyntaxKind.SemicolonToken)));
            }

            // public static IArgumentOperationWrapper FromOperation(IOperation operation)
            // {
            //     if (operation == null)
            //     {
            //         return default;
            //     }
            //
            //     if (!IsInstance(operation))
            //     {
            //         throw new InvalidCastException($"Cannot cast '{operation.GetType().FullName}' to '{WrappedTypeName}'");
            //     }
            //
            //     return new IArgumentOperationWrapper(operation);
            // }
            members = members.Add(MethodDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                returnType: IdentifierName(node.WrapperName),
                explicitInterfaceSpecifier: null,
                identifier: Identifier("FromOperation"),
                typeParameterList: null,
                parameterList: ParameterList(SingletonSeparatedList(Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: IdentifierName("IOperation"),
                    identifier: Identifier("operation"),
                    @default: null))),
                constraintClauses: default,
                body: Block(
                    IfStatement(
                        condition: BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            left: IdentifierName("operation"),
                            right: LiteralExpression(SyntaxKind.NullLiteralExpression)),
                        statement: Block(
                            ReturnStatement(LiteralExpression(SyntaxKind.DefaultLiteralExpression)))),
                    IfStatement(
                        condition: PrefixUnaryExpression(
                            SyntaxKind.LogicalNotExpression,
                            operand: InvocationExpression(
                                expression: IdentifierName("IsInstance"),
                                argumentList: ArgumentList(SingletonSeparatedList(Argument(IdentifierName("operation")))))),
                        statement: Block(
                            ThrowStatement(ObjectCreationExpression(
                                type: IdentifierName("InvalidCastException"),
                                argumentList: ArgumentList(SingletonSeparatedList(Argument(
                                    InterpolatedStringExpression(
                                        Token(SyntaxKind.InterpolatedStringStartToken),
                                        List(new InterpolatedStringContentSyntax[]
                                        {
                                            InterpolatedStringText(Token(
                                                leading: default,
                                                SyntaxKind.InterpolatedStringTextToken,
                                                "Cannot cast '",
                                                "Cannot cast '",
                                                trailing: default)),
                                            Interpolation(MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                expression: InvocationExpression(
                                                    expression: MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        expression: IdentifierName("operation"),
                                                        name: IdentifierName("GetType")),
                                                    argumentList: ArgumentList()),
                                                name: IdentifierName("FullName"))),
                                            InterpolatedStringText(Token(
                                                leading: default,
                                                SyntaxKind.InterpolatedStringTextToken,
                                                "' to '",
                                                "' to '",
                                                trailing: default)),
                                            Interpolation(IdentifierName("WrappedTypeName")),
                                            InterpolatedStringText(Token(
                                                leading: default,
                                                SyntaxKind.InterpolatedStringTextToken,
                                                "'",
                                                "'",
                                                trailing: default)),
                                        }))))),
                                initializer: null)))),
                    ReturnStatement(ObjectCreationExpression(
                        type: IdentifierName(node.WrapperName),
                        argumentList: ArgumentList(SingletonSeparatedList(Argument(IdentifierName("operation")))),
                        initializer: null))),
                expressionBody: null));

            // public static bool IsInstance(IOperation operation)
            // {
            //     return operation != null && LightupHelpers.CanWrapOperation(operation, WrappedType);
            // }
            members = members.Add(MethodDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                returnType: PredefinedType(Token(SyntaxKind.BoolKeyword)),
                explicitInterfaceSpecifier: null,
                identifier: Identifier("IsInstance"),
                typeParameterList: null,
                parameterList: ParameterList(SingletonSeparatedList(Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: IdentifierName("IOperation"),
                    identifier: Identifier("operation"),
                    @default: null))),
                constraintClauses: default,
                body: Block(
                    ReturnStatement(BinaryExpression(
                        SyntaxKind.LogicalAndExpression,
                        left: BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            left: IdentifierName("operation"),
                            right: LiteralExpression(SyntaxKind.NullLiteralExpression)),
                        right: InvocationExpression(
                            expression: MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: IdentifierName("LightupHelpers"),
                                name: IdentifierName("CanWrapOperation")),
                            argumentList: ArgumentList(SeparatedList(
                                new[]
                                {
                                    Argument(IdentifierName("operation")),
                                    Argument(IdentifierName("WrappedType")),
                                })))))),
                expressionBody: null));

            if (node.IsAbstract)
            {
                // internal static IMemberReferenceOperationWrapper FromUpcast(IOperation operation)
                // {
                //     return new IMemberReferenceOperationWrapper(operation);
                // }
                members = members.Add(MethodDeclaration(
                    attributeLists: default,
                    modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                    returnType: IdentifierName(node.WrapperName),
                    explicitInterfaceSpecifier: null,
                    identifier: Identifier("FromUpcast"),
                    typeParameterList: null,
                    parameterList: ParameterList(SingletonSeparatedList(Parameter(
                        attributeLists: default,
                        modifiers: default,
                        type: IdentifierName("IOperation"),
                        identifier: Identifier("operation"),
                        @default: null))),
                    constraintClauses: default,
                    body: Block(
                        ReturnStatement(ObjectCreationExpression(
                            type: IdentifierName(node.WrapperName),
                            argumentList: ArgumentList(SingletonSeparatedList(Argument(IdentifierName("operation")))),
                            initializer: null))),
                    expressionBody: null));
            }

            var wrapperStruct = StructDeclaration(
                attributeLists: default,
                modifiers: SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)).Add(Token(SyntaxKind.ReadOnlyKeyword)),
                identifier: Identifier(node.WrapperName),
                typeParameterList: null,
                baseList: BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName("IOperationWrapper")))),
                constraintClauses: default,
                members: members)
                .WithLeadingTrivia(node.Summary);
            var wrapperNamespace = NamespaceDeclaration(
                name: ParseName("StyleCop.Analyzers.Lightup"),
                externs: default,
                usings: List<UsingDirectiveSyntax>()
                    .Add(UsingDirective(ParseName("System")))
                    .Add(UsingDirective(ParseName("System.Collections.Immutable")))
                    .Add(UsingDirective(ParseName("Microsoft.CodeAnalysis"))),
                members: SingletonList<MemberDeclarationSyntax>(wrapperStruct));

            wrapperNamespace = wrapperNamespace
                .NormalizeWhitespace()
                .WithLeadingTrivia(
                    Comment("// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved."),
                    CarriageReturnLineFeed,
                    Comment("// Licensed under the MIT License. See LICENSE in the project root for license information."),
                    CarriageReturnLineFeed,
                    CarriageReturnLineFeed,
                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)
                        .WithNullableKeyword(Token(TriviaList(), SyntaxKind.NullableKeyword, TriviaList(Space)))),
                    CarriageReturnLineFeed,
                    CarriageReturnLineFeed)
                .WithTrailingTrivia(
                    CarriageReturnLineFeed);

            context.AddSource(node.WrapperName + ".g.cs", SourceText.From(wrapperNamespace.ToFullString(), Encoding.UTF8));
        }

        private void GenerateOperationWrapperHelper(in GeneratorExecutionContext context, ImmutableArray<InterfaceData> wrapperTypes)
        {
            // private static readonly ImmutableDictionary<Type, Type> WrappedTypes;
            var wrappedTypes = FieldDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
                declaration: VariableDeclaration(
                    type: GenericName(
                        identifier: Identifier("ImmutableDictionary"),
                        typeArgumentList: TypeArgumentList(
                            SeparatedList<TypeSyntax>(
                                new[]
                                {
                                    IdentifierName("Type"),
                                    IdentifierName("Type"),
                                }))),
                    variables: SingletonSeparatedList(VariableDeclarator(Identifier("WrappedTypes")))));

            // var codeAnalysisAssembly = typeof(SyntaxNode).GetTypeInfo().Assembly;
            // var builder = ImmutableDictionary.CreateBuilder<Type, Type>();
            var staticCtorStatements = List<StatementSyntax>()
                .Add(LocalDeclarationStatement(VariableDeclaration(
                    type: IdentifierName("var"),
                    variables: SingletonSeparatedList(VariableDeclarator(
                        identifier: Identifier("codeAnalysisAssembly"),
                        argumentList: null,
                        initializer: EqualsValueClause(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        expression: TypeOfExpression(IdentifierName("SyntaxNode")),
                                        name: IdentifierName("GetTypeInfo"))),
                                name: IdentifierName("Assembly"))))))))
                .Add(LocalDeclarationStatement(VariableDeclaration(
                    type: IdentifierName("var"),
                    variables: SingletonSeparatedList(VariableDeclarator(
                        identifier: Identifier("builder"),
                        argumentList: null,
                        initializer: EqualsValueClause(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    expression: IdentifierName("ImmutableDictionary"),
                                    name: GenericName(
                                        identifier: Identifier("CreateBuilder"),
                                        typeArgumentList: TypeArgumentList(
                                            SeparatedList<TypeSyntax>(
                                                new[]
                                                {
                                                    IdentifierName("Type"),
                                                    IdentifierName("Type"),
                                                })))))))))));

            foreach (var node in wrapperTypes)
            {
                // builder.Add(typeof(IArgumentOperationWrapper), codeAnalysisAssembly.GetType(IArgumentOperationWrapper.WrappedTypeName));
                staticCtorStatements = staticCtorStatements.Add(ExpressionStatement(
                    InvocationExpression(
                        expression: MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: IdentifierName("builder"),
                            name: IdentifierName("Add")),
                        argumentList: ArgumentList(
                            SeparatedList(
                                new[]
                                {
                                    Argument(TypeOfExpression(IdentifierName(node.WrapperName))),
                                    Argument(
                                        InvocationExpression(
                                            expression: MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                expression: IdentifierName("codeAnalysisAssembly"),
                                                name: IdentifierName("GetType")),
                                            argumentList: ArgumentList(SingletonSeparatedList(Argument(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    expression: IdentifierName(node.WrapperName),
                                                    name: IdentifierName("WrappedTypeName"))))))),
                                })))));
            }

            // WrappedTypes = builder.ToImmutable();
            staticCtorStatements = staticCtorStatements.Add(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    left: IdentifierName("WrappedTypes"),
                    right: InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression: IdentifierName("builder"),
                            name: IdentifierName("ToImmutable"))))));

            var staticCtor = ConstructorDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.StaticKeyword)),
                identifier: Identifier("OperationWrapperHelper"),
                parameterList: ParameterList(),
                initializer: null,
                body: Block(staticCtorStatements),
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
            var getWrappedType = MethodDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                returnType: IdentifierName("Type"),
                explicitInterfaceSpecifier: null,
                identifier: Identifier("GetWrappedType"),
                typeParameterList: null,
                parameterList: ParameterList(SingletonSeparatedList(Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: IdentifierName("Type"),
                    identifier: Identifier("wrapperType"),
                    @default: null))),
                constraintClauses: default,
                body: Block(
                    IfStatement(
                        condition: InvocationExpression(
                            expression: MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                expression: IdentifierName("WrappedTypes"),
                                name: IdentifierName("TryGetValue")),
                            argumentList: ArgumentList(SeparatedList(
                                new[]
                                {
                                    Argument(IdentifierName("wrapperType")),
                                    Argument(
                                        nameColon: null,
                                        refKindKeyword: Token(SyntaxKind.OutKeyword),
                                        expression: DeclarationExpression(
                                            type: IdentifierName("Type"),
                                            designation: SingleVariableDesignation(Identifier("wrappedType")))),
                                }))),
                        statement: Block(
                            ReturnStatement(IdentifierName("wrappedType")))),
                    ReturnStatement(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                expressionBody: null);

            getWrappedType = getWrappedType.WithLeadingTrivia(TriviaList(
                Trivia(DocumentationComment(
                    XmlText(" "),
                    XmlSummaryElement(
                        XmlNewLine(Environment.NewLine),
                        XmlText(" Gets the type that is wrapped by the given wrapper."),
                        XmlNewLine(Environment.NewLine),
                        XmlText(" ")),
                    XmlNewLine(Environment.NewLine),
                    XmlText(" "),
                    XmlParamElement(
                        "wrapperType",
                        XmlText("Type of the wrapper for which the wrapped type should be retrieved.")),
                    XmlNewLine(Environment.NewLine),
                    XmlText(" "),
                    XmlReturnsElement(
                        XmlText("The wrapped type, or null if there is no info.")),
                    XmlNewLine(Environment.NewLine).WithoutTrailingTrivia()))));

            var wrapperHelperClass = ClassDeclaration(
                attributeLists: default,
                modifiers: SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)).Add(Token(SyntaxKind.StaticKeyword)),
                identifier: Identifier("OperationWrapperHelper"),
                typeParameterList: null,
                baseList: null,
                constraintClauses: default,
                members: List<MemberDeclarationSyntax>()
                    .Add(wrappedTypes)
                    .Add(staticCtor)
                    .Add(getWrappedType));
            var wrapperNamespace = NamespaceDeclaration(
                name: ParseName("StyleCop.Analyzers.Lightup"),
                externs: default,
                usings: List<UsingDirectiveSyntax>()
                    .Add(UsingDirective(ParseName("System")))
                    .Add(UsingDirective(ParseName("System.Collections.Immutable")))
                    .Add(UsingDirective(ParseName("System.Reflection")))
                    .Add(UsingDirective(ParseName("Microsoft.CodeAnalysis"))),
                members: SingletonList<MemberDeclarationSyntax>(wrapperHelperClass));

            wrapperNamespace = wrapperNamespace
                .NormalizeWhitespace()
                .WithLeadingTrivia(
                    Comment("// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved."),
                    CarriageReturnLineFeed,
                    Comment("// Licensed under the MIT License. See LICENSE in the project root for license information."),
                    CarriageReturnLineFeed,
                    CarriageReturnLineFeed,
                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)
                        .WithNullableKeyword(Token(TriviaList(), SyntaxKind.NullableKeyword, TriviaList(Space)))),
                    CarriageReturnLineFeed,
                    CarriageReturnLineFeed)
                .WithTrailingTrivia(
                    CarriageReturnLineFeed);

            context.AddSource("OperationWrapperHelper.g.cs", SourceText.From(wrapperNamespace.ToFullString(), Encoding.UTF8));
        }

        private void GenerateOperationKindEx(in GeneratorExecutionContext context, ImmutableArray<InterfaceData> wrapperTypes)
        {
            var operationKinds = wrapperTypes
                .SelectMany(type => type.OperationKinds)
                .OrderBy(kind => kind.value)
                .ToImmutableArray();

            var members = List<MemberDeclarationSyntax>();
            foreach (var operationKind in operationKinds)
            {
                // public const OperationKind FieldReference = (OperationKind)26;
                members = members.Add(FieldDeclaration(
                    attributeLists: default,
                    modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ConstKeyword)),
                    declaration: VariableDeclaration(
                        type: IdentifierName("OperationKind"),
                        variables: SingletonSeparatedList(VariableDeclarator(
                            identifier: Identifier(operationKind.name),
                            argumentList: null,
                            initializer: EqualsValueClause(CastExpression(
                                type: IdentifierName("OperationKind"),
                                expression: LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal($"0x{operationKind.value:x}", operationKind.value)))))))));
            }

            var operationKindExClass = ClassDeclaration(
                attributeLists: default,
                modifiers: SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)).Add(Token(SyntaxKind.StaticKeyword)),
                identifier: Identifier("OperationKindEx"),
                typeParameterList: null,
                baseList: null,
                constraintClauses: default,
                members: members);
            var wrapperNamespace = NamespaceDeclaration(
                name: ParseName("StyleCop.Analyzers.Lightup"),
                externs: default,
                usings: List<UsingDirectiveSyntax>()
                    .Add(UsingDirective(ParseName("Microsoft.CodeAnalysis"))),
                members: SingletonList<MemberDeclarationSyntax>(operationKindExClass));

            wrapperNamespace = wrapperNamespace
                .NormalizeWhitespace()
                .WithLeadingTrivia(
                    Comment("// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved."),
                    CarriageReturnLineFeed,
                    Comment("// Licensed under the MIT License. See LICENSE in the project root for license information."),
                    CarriageReturnLineFeed,
                    CarriageReturnLineFeed,
                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)
                        .WithNullableKeyword(Token(TriviaList(), SyntaxKind.NullableKeyword, TriviaList(Space)))),
                    CarriageReturnLineFeed,
                    CarriageReturnLineFeed)
                .WithTrailingTrivia(
                    CarriageReturnLineFeed);

            context.AddSource("OperationKindEx.g.cs", SourceText.From(wrapperNamespace.ToFullString(), Encoding.UTF8));
        }

        private static SyntaxTriviaList ParseSummaryDocComment(XElement node)
        {
            var summary = node.Element("Comments")?.Element("summary")?.ToString();
            if (!string.IsNullOrEmpty(summary))
            {
                summary = Regex.Replace(summary, "^", "/// ", RegexOptions.Multiline);
                return ParseLeadingTrivia($"{summary}\r\n");
            }
            return SyntaxTriviaList.Empty;
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

                    if (!operationKinds.TryGetValue(node.Attribute("Name").Value, out var kinds))
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

                    if (!operationKinds.TryGetValue(node.Attribute("Name").Value, out var kinds))
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

                                int parsedValue = ParsePrefixHexValue(entry.Attribute("Value").Value);
                                nodeBuilder.Add((entry.Attribute("Name").Value, parsedValue, entry.Attribute("ExtraDescription")?.Value));
                            }

                            builder.Add(node.Attribute("Name").Value, nodeBuilder.ToImmutable());
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

                    var nodeName = node.Attribute("Name").Value;
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
                    builder.Add(ParsePrefixHexValue(skippedKind.Attribute("Value").Value));
                }

                foreach (var explicitKind in document.XPathSelectElements("/Tree/*/OperationKind/Entry"))
                {
                    builder.Add(ParsePrefixHexValue(explicitKind.Attribute("Value").Value));
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
                this.InterfaceName = node.Attribute("Name").Value;
                this.Namespace = node.Attribute("Namespace")?.Value ?? "Operations";
                this.Name = this.InterfaceName.Substring("I".Length, this.InterfaceName.Length - "I".Length - "Operation".Length);
                this.WrapperName = this.InterfaceName + "Wrapper";
                this.BaseInterfaceName = node.Attribute("Base").Value;
                this.IsAbstract = node.Name == "AbstractNode";
                this.Properties = node.XPathSelectElements("Property").Select(property => new PropertyData(property)).ToImmutableArray();
                this.Summary = ParseSummaryDocComment(node);
            }

            public ImmutableArray<(string name, int value, string? extraDescription)> OperationKinds { get; }

            public string InterfaceName { get; }

            public string Namespace { get; }

            public string Name { get; }

            public string WrapperName { get; }

            public string BaseInterfaceName { get; }

            public bool IsAbstract { get; }

            public SyntaxTriviaList Summary { get; }

            public ImmutableArray<PropertyData> Properties { get; }

            public InterfaceData? BaseInterface
            {
                get
                {
                    if (this.documentData.Interfaces.TryGetValue(this.BaseInterfaceName, out var baseInterface))
                    {
                        return baseInterface;
                    }

                    return null;
                }
            }
        }

        private sealed class PropertyData
        {
            public PropertyData(XElement node)
            {
                this.Name = node.Attribute("Name").Value;
                this.AccessorName = this.Name + "Accessor";
                this.Type = node.Attribute("Type").Value;
                this.WrapperType = $"{(IsNullable(Type) ? Type.Substring(0, Type.Length - 1) : Type)}Wrapper";
                this.WrapperTypeWithNullable = IsNullable(Type) ? $"{WrapperType}?" : WrapperType;

                this.IsNew = node.Attribute("New")?.Value == "true";
                this.IsPublicProperty = node.Attribute("Internal")?.Value != "true";
                this.IsOverride = node.Attribute("Override")?.Value == "true";

                this.IsSkipped = this.Type switch
                {
                    "ArgumentKind" => true,
                    "BranchKind" => true,
                    "CaseKind" => true,
                    "CommonConversion" => true,
                    "ForEachLoopOperationInfo" => true,
                    "IDiscardSymbol" => true,
                    "InstanceReferenceKind" => true,
                    "LoopKind" => true,
                    "PlaceholderKind" => true,
                    "InterpolatedStringArgumentPlaceholderKind" => true,
                    _ => !this.IsPublicProperty,
                };
                this.Summary = ParseSummaryDocComment(node);
                this.NeedsWrapper = IsAnyOperation(this.Type) && this.Type != $"IOperation{(IsNullable(this.Type) ? "?" : string.Empty)}";
                this.IsDerivedOperationArray = IsAnyOperationArray(this.Type) && this.Type != "ImmutableArray<IOperation>";

                if (this.IsDerivedOperationArray)
                {
                    this.AccessorResultType = GenericName(
                        identifier: Identifier("ImmutableArray"),
                        typeArgumentList: TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName("IOperation"))));
                }
                else if (IsAnyOperation(this.Type))
                {
                    this.AccessorResultType = IdentifierName("IOperation");
                }
                else
                {
                    this.AccessorResultType = ParseTypeName(this.Type);
                }
            }

            public bool IsNew { get; }

            public bool IsPublicProperty { get; }

            public bool IsSkipped { get; }

            public bool IsOverride { get; }

            public string Name { get; }

            public SyntaxTriviaList Summary { get; }

            public string AccessorName { get; }

            public string Type { get; }

            public string WrapperType { get; }

            public string WrapperTypeWithNullable { get; }

            public bool NeedsWrapper { get; }

            public bool IsDerivedOperationArray { get; }

            public TypeSyntax AccessorResultType { get; }

            private static bool IsNullable(string type)
                => type.EndsWith("?");

            private static bool IsAnyOperation(string type)
            {
                return type.StartsWith("I") && IsNullable(type) ? type.EndsWith("Operation?") : type.EndsWith("Operation");
            }

            private static bool IsAnyOperationArray(string type)
            {
                return type.StartsWith("ImmutableArray<I") && type.EndsWith("Operation>");
            }
        }
    }
}
