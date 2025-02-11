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

using Roslyn.Utilities;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TokenTypeAnalyzer : TokenTypeAnalyzerBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = CSharpFacade.Instance;

        protected override TokenClassifierBase GetTokenClassifier(SemanticModel semanticModel, bool skipIdentifierTokens) =>
            new TokenClassifier(semanticModel, skipIdentifierTokens);

        protected override TriviaClassifierBase GetTriviaClassifier() =>
            new TriviaClassifier();

        internal sealed class TokenClassifier : TokenClassifierBase
        {
            private static readonly SyntaxKind[] StringLiteralTokens =
            {
                SyntaxKind.StringLiteralToken,
                SyntaxKind.CharacterLiteralToken,
                SyntaxKindEx.SingleLineRawStringLiteralToken,
                SyntaxKindEx.MultiLineRawStringLiteralToken,
                SyntaxKindEx.Utf8StringLiteralToken,
                SyntaxKindEx.Utf8SingleLineRawStringLiteralToken,
                SyntaxKindEx.Utf8MultiLineRawStringLiteralToken,
                SyntaxKind.InterpolatedStringStartToken,
                SyntaxKind.InterpolatedVerbatimStringStartToken,
                SyntaxKindEx.InterpolatedSingleLineRawStringStartToken,
                SyntaxKindEx.InterpolatedMultiLineRawStringStartToken,
                SyntaxKind.InterpolatedStringTextToken,
                SyntaxKind.InterpolatedStringEndToken,
                SyntaxKindEx.InterpolatedRawStringEndToken,
            };

            public TokenClassifier(SemanticModel semanticModel, bool skipIdentifiers) : base(semanticModel, skipIdentifiers) { }

            protected override SyntaxNode GetBindableParent(SyntaxToken token) =>
                token.GetBindableParent();

            protected override bool IsIdentifier(SyntaxToken token) =>
                token.IsKind(SyntaxKind.IdentifierToken);

            protected override bool IsKeyword(SyntaxToken token) =>
                SyntaxFacts.IsKeywordKind(token.Kind());

            protected override bool IsNumericLiteral(SyntaxToken token) =>
                token.IsKind(SyntaxKind.NumericLiteralToken);

            protected override bool IsStringLiteral(SyntaxToken token) =>
                token.IsAnyKind(StringLiteralTokens);

            [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7805", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
            protected override TokenTypeInfo.Types.TokenInfo ClassifyIdentifier(SyntaxToken token) =>
                // Based on <Kind Name="IdentifierToken"/> in SonarAnalyzer.CFG/ShimLayer\Syntax.xml
                // order by https://docs.google.com/spreadsheets/d/1hb6Oz8NE1y4kfv57npSrGEzMd7tm9gYQtI1dABOneMk
                token.Parent switch
                {
                    SimpleNameSyntax x when token == x.Identifier && ClassifySimpleName(x) is { } tokenType => TokenInfo(token, tokenType),
                    VariableDeclaratorSyntax x when token == x.Identifier => null,
                    ParameterSyntax x when token == x.Identifier => null,
                    MethodDeclarationSyntax x when token == x.Identifier => null,
                    PropertyDeclarationSyntax x when token == x.Identifier => null,
                    TypeParameterSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    BaseTypeDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    ConstructorDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    FromClauseSyntax x when token == x.Identifier => null,
                    LetClauseSyntax x when token == x.Identifier => null,
                    JoinClauseSyntax x when token == x.Identifier => null,
                    JoinIntoClauseSyntax x when token == x.Identifier => null,
                    QueryContinuationSyntax x when token == x.Identifier => null,
                    LabeledStatementSyntax x when token == x.Identifier => null,
                    ForEachStatementSyntax x when token == x.Identifier => null,
                    CatchDeclarationSyntax x when token == x.Identifier => null,
                    ExternAliasDirectiveSyntax x when token == x.Identifier => null,
                    EnumMemberDeclarationSyntax x when token == x.Identifier => null,
                    EventDeclarationSyntax x when token == x.Identifier => null,
                    AccessorDeclarationSyntax x when token == x.Keyword => null,
                    DelegateDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    DestructorDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    AttributeTargetSpecifierSyntax x when token == x.Identifier => TokenInfo(token, TokenType.Keyword), // for unknown target specifier [unknown: Obsolete]
                    // Wrapper checks. HotPath: Make sure to test for SyntaxKind to avoid Wrapper.IsInstance calls
                    // which are slow and allocating. Check the documentation for associated SyntaxKinds and that the
                    // node class is sealed.
                    { RawKind: (int)SyntaxKindEx.FunctionPointerUnmanagedCallingConvention } x when token == ((FunctionPointerUnmanagedCallingConventionSyntaxWrapper)x).Name => null,
                    { RawKind: (int)SyntaxKindEx.TupleElement } x when token == ((TupleElementSyntaxWrapper)x).Identifier => null,
                    { RawKind: (int)SyntaxKindEx.LocalFunctionStatement } x when token == ((LocalFunctionStatementSyntaxWrapper)x).Identifier => null,
                    { RawKind: (int)SyntaxKindEx.SingleVariableDesignation } x when token == ((SingleVariableDesignationSyntaxWrapper)x).Identifier => null,
                    _ => base.ClassifyIdentifier(token),
                };

            [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7805", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
            private TokenType? ClassifySimpleName(SimpleNameSyntax x) =>
                IsInTypeContext(x)
                    ? ClassifySimpleNameType(x)
                    : ClassifySimpleNameExpression(x);

            [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7805", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
            private TokenType? ClassifySimpleNameExpression(SimpleNameSyntax name) =>
                name.Parent is MemberAccessExpressionSyntax
                    ? ClassifyMemberAccess(name)
                    : ClassifySimpleNameExpressionSpecialContext(name, name);

            /// <summary>
            /// The <paramref name="name"/> is likely not referring a type, but there are some <paramref name="context"/> and
            /// special cases where it still might bind to a type or is treated as a keyword. The <paramref name="context"/>
            /// is the member access of the <paramref name="name"/>. e.g. for A.B.C <paramref name="name"/> may
            /// refer to "B" and <paramref name="context"/> would be the parent member access expression A.B and recursively A.B.C.
            /// </summary>
            [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7805", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
            private TokenType? ClassifySimpleNameExpressionSpecialContext(SyntaxNode context, SimpleNameSyntax name) =>
                context.Parent switch
                {
                    // some identifier can be bound to a type or a constant:
                    CaseSwitchLabelSyntax => ClassifyIdentifierByModel(name), // case i:
                    { } parent when NameIsRightOfIsExpression(name, parent) => ClassifyIdentifierByModel(name), // is i
                    { RawKind: (int)SyntaxKindEx.ConstantPattern } => ClassifyIdentifierByModel(name), // is { X: i }
                    // nameof(i) can be bound to a type or a member
                    ArgumentSyntax x when IsNameOf(x) => IsValueParameterOfSetter(name) ? TokenType.Keyword : ClassifyIdentifierByModel(name),
                    // walk up memberaccess to detect cases like above
                    MemberAccessExpressionSyntax x => ClassifySimpleNameExpressionSpecialContext(x, name),
                    _ => ClassifySimpleNameExpressionSpecialNames(name)
                };

            private bool IsNameOf(ArgumentSyntax argument)
                => argument is
                {
                    Parent: ArgumentListSyntax
                    {
                        Arguments.Count: 1,
                        Parent: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "nameof" } }
                    }
                };

            private bool NameIsRightOfIsExpression(NameSyntax name, SyntaxNode binary)
                => binary is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.IsExpression, Right: { } x } && x == name;

            /// <summary>
            /// Some expression identifier are classified differently, like "value" in a setter.
            /// </summary>
            private TokenType ClassifySimpleNameExpressionSpecialNames(SimpleNameSyntax name) =>
                // "value" in a setter is a classified as keyword
                IsValueParameterOfSetter(name)
                    ? TokenType.Keyword
                    : TokenType.UnknownTokentype;

            private bool IsValueParameterOfSetter(SimpleNameSyntax simpleName)
                => simpleName is IdentifierNameSyntax { Identifier.Text: "value" }
                    && IsLeftMostMemberAccess(simpleName)
                    && SemanticModel.GetSymbolInfo(simpleName).Symbol is IParameterSymbol
                    {
                        ContainingSymbol: IMethodSymbol
                        {
                            MethodKind: MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove
                        }
                    };

            private static bool IsLeftMostMemberAccess(SimpleNameSyntax simpleName)
                => simpleName is { Parent: not MemberAccessExpressionSyntax }
                    || (simpleName is { Parent: MemberAccessExpressionSyntax { Expression: { } expression } } && expression == simpleName);

            [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7805", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
            private TokenType? ClassifyMemberAccess(SimpleNameSyntax name) =>
                name switch
                {
                    {
                        Parent: MemberAccessExpressionSyntax // Most right hand side of a member access?
                        {
                            Parent: not MemberAccessExpressionSyntax, // Topmost in a memberaccess tree
                            Name: { } parentName // Right hand side
                        } parent
                    } when parentName == name => ClassifySimpleNameExpressionSpecialContext(parent, name),
                    _ when IsValueParameterOfSetter(name) => TokenType.Keyword,
                    // 'name' can not be a nested type, if there is an expression to the left of the member access,
                    // that can not bind to a type. The only things that can bind to a type are SimpleNames (Identifier or GenericName)
                    // or pre-defined types. None of the pre-defined types have a nested type, so we can exclude these as well.
                    { Parent: MemberAccessExpressionSyntax x } when AnyMemberAccessLeftIsNotAType(x) => TokenType.UnknownTokentype,
                    // The left side of a pointer member access must be a pointer and can not be a type
                    { Parent: MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.PointerMemberAccessExpression } } => TokenType.UnknownTokentype,
                    _ => ClassifyIdentifierByModel(name),
                };

            private static bool AnyMemberAccessLeftIsNotAType(MemberAccessExpressionSyntax memberAccess) =>
                memberAccess switch
                {
                    { Expression: not SimpleNameSyntax and not MemberAccessExpressionSyntax and not AliasQualifiedNameSyntax } => true,
                    { Expression: MemberAccessExpressionSyntax left } => AnyMemberAccessLeftIsNotAType(left),
                    // Heuristic: any MemberAccess that starts with a lowercase on the most left hand side, is assumed to start
                    // as an expression (e.g. s.Length). Rational: It is (almost) granted that Types (including enums) start
                    // with an uppercase in C#. Any identifier, that starts with a lower case is assumed to refer a local, a parameter,
                    // or a field.
                    { Expression: SimpleNameSyntax { Identifier.ValueText: { Length: >= 1 } mostLeftIdentifier } } => char.IsLower(mostLeftIdentifier[0]),
                    _ => false,
                };

            private TokenType ClassifyIdentifierByModel(SimpleNameSyntax name) =>
                SemanticModel.GetSymbolInfo(name).Symbol is INamedTypeSymbol or ITypeParameterSymbol
                    ? TokenType.TypeName
                    : TokenType.UnknownTokentype;

            private TokenType ClassifyAliasDeclarationByModel(UsingDirectiveSyntax usingDirective) =>
                SemanticModel.GetDeclaredSymbol(usingDirective) is { Target: INamedTypeSymbol }
                    ? TokenType.TypeName
                    : TokenType.UnknownTokentype;

            [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7805", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
            private TokenType? ClassifySimpleNameType(SimpleNameSyntax name) =>
                name is GenericNameSyntax
                    ? TokenType.TypeName
                    : ClassifySimpleNameTypeSpecialContext(name, name);

            [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7805", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
            private TokenType? ClassifySimpleNameTypeSpecialContext(SyntaxNode context, SimpleNameSyntax name) =>
                context.Parent switch
                {
                    // namespace X; or namespace X { } -> always unknown
                    NamespaceDeclarationSyntax or { RawKind: (int)SyntaxKindEx.FileScopedNamespaceDeclaration } => TokenType.UnknownTokentype,
                    // using System; -> normal using
                    UsingDirectiveSyntax { Alias: null, StaticKeyword.RawKind: (int)SyntaxKind.None } => TokenType.UnknownTokentype,
                    // using Alias = System; -> "System" can be a type or a namespace
                    UsingDirectiveSyntax { Alias: not null } => ClassifyIdentifierByModel(name),
                    // using Alias = System; -> "Alias" can be a type or a namespace
                    NameEqualsSyntax { Parent: UsingDirectiveSyntax { Alias.Name: { } aliasName } usingDirective } when aliasName == name => ClassifyAliasDeclarationByModel(usingDirective),
                    // using static System.Math; -> most right hand side must be a type
                    UsingDirectiveSyntax
                    {
                        StaticKeyword.RawKind: (int)SyntaxKind.StaticKeyword, Name: QualifiedNameSyntax { Right: SimpleNameSyntax x }
                    } => x == name ? TokenType.TypeName : ClassifyIdentifierByModel(name),
                    // Walk up classified names (to detect namespace and using context)
                    QualifiedNameSyntax parent => ClassifySimpleNameTypeSpecialContext(parent, name),
                    AliasQualifiedNameSyntax parent => ClassifySimpleNameTypeSpecialContext(parent, name),
                    // We are in a "normal" type context like a declaration
                    _ => ClassifySimpleNameTypeInTypeContext(name),
                };

            [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7805", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
            private TokenType ClassifySimpleNameTypeInTypeContext(SimpleNameSyntax name) =>
                name switch
                {
                    // unqualified types called "var" or "dynamic" are classified as keywords.
                    { Parent: not QualifiedNameSyntax and not AliasQualifiedNameSyntax } => name is { Identifier.Text: "var" or "dynamic" }
                        ? TokenType.Keyword
                        : TokenType.TypeName,
                    { Parent: QualifiedNameSyntax { Parent: { } parentOfTopMostQualifiedName, Right: { } right } topMostQualifiedName } when
                        right == name // On the right hand side?
                        && parentOfTopMostQualifiedName is not QualifiedNameSyntax // Is this the most right hand side?

                        // This is a type, except on the right side of "is" where it might also be a constant like Int32.MaxValue
                        && !NameIsRightOfIsExpression(topMostQualifiedName, parentOfTopMostQualifiedName) => TokenType.TypeName,
                    // Name is directly after alias global::SomeType
                    { Parent: AliasQualifiedNameSyntax { Name: { } x, Parent: not (QualifiedNameSyntax or MemberAccessExpressionSyntax) } } when name == x => TokenType.TypeName,
                    // We are somewhere in a qualified name. It probably is a namespace but could also be the outer type of a nested type.
                    _ => ClassifyIdentifierByModel(name),
                };

            [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7805", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
            private static bool IsInTypeContext(SimpleNameSyntax name) =>
                // Based on Syntax.xml search for Type="TypeSyntax" and Type="NameSyntax"
                // order by https://docs.google.com/spreadsheets/d/1hb6Oz8NE1y4kfv57npSrGEzMd7tm9gYQtI1dABOneMk
                // Important: "False" is the default (meaning "expression" context). The "true" returning path must be complete to avoid missclassifications.
                // HotPath: Some "false" returning checks are included for the most common expression context kinds.
                name.Parent switch
                {
                    MemberAccessExpressionSyntax x when x.Expression == name || x.Name == name => false, // Performance optimization
                    ArgumentSyntax x when x.Expression == name => false, // Performance optimization
                    InvocationExpressionSyntax x when x.Expression == name => false, // Performance optimization
                    EqualsValueClauseSyntax x when x.Value == name => false, // Performance optimization
                    AssignmentExpressionSyntax x when x.Right == name || x.Left == name => false, // Performance optimization
                    VariableDeclarationSyntax x => x.Type == name,
                    QualifiedNameSyntax => true,
                    ParameterSyntax x => x.Type == name,
                    NullableTypeSyntax x => x.ElementType == name,
                    NamespaceDeclarationSyntax x => x.Name == name,
                    AliasQualifiedNameSyntax x => x.Name == name,
                    BaseTypeSyntax x => x.Type == name,
                    TypeArgumentListSyntax => true,
                    ObjectCreationExpressionSyntax x => x.Type == name,
                    BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AsExpression } x => x.Right == name,
                    ArrayTypeSyntax x => x.ElementType == name,
                    RefValueExpressionSyntax x => x.Type == name,
                    DefaultExpressionSyntax x => x.Type == name,
                    TypeOfExpressionSyntax x => x.Type == name,
                    SizeOfExpressionSyntax x => x.Type == name,
                    CastExpressionSyntax x => x.Type == name,
                    StackAllocArrayCreationExpressionSyntax x => x.Type == name,
                    FromClauseSyntax x => x.Type == name,
                    JoinClauseSyntax x => x.Type == name,
                    ForEachStatementSyntax x => x.Type == name,
                    CatchDeclarationSyntax x => x.Type == name,
                    DelegateDeclarationSyntax x => x.ReturnType == name,
                    TypeConstraintSyntax x => x.Type == name,
                    TypeParameterConstraintClauseSyntax x => x.Name == name,
                    MethodDeclarationSyntax x => x.ReturnType == name,
                    OperatorDeclarationSyntax x => x.ReturnType == name,
                    ConversionOperatorDeclarationSyntax x => x.Type == name,
                    BasePropertyDeclarationSyntax x => x.Type == name,
                    PointerTypeSyntax x => x.ElementType == name,
                    AttributeSyntax x => x.Name == name,
                    ExplicitInterfaceSpecifierSyntax x => x.Name == name,
                    UsingDirectiveSyntax x => x.Name == name,
                    NameEqualsSyntax { Parent: UsingDirectiveSyntax { Alias.Name: { } x } } => x == name,
                    // Wrapper. HotPath: Use SyntaxKind checks instead of Wrapper.IsInstance (slow and allocating).
                    // Make sure to check the associated syntax kinds in the documentation and/or that the types are sealed.
                    { RawKind: (int)SyntaxKindEx.FunctionPointerParameter } x => ((FunctionPointerParameterSyntaxWrapper)x).Type == name,
                    { RawKind: (int)SyntaxKindEx.DeclarationPattern } x => ((DeclarationPatternSyntaxWrapper)x).Type == name,
                    { RawKind: (int)SyntaxKindEx.RecursivePattern } x => ((RecursivePatternSyntaxWrapper)x).Type == name,
                    { RawKind: (int)SyntaxKindEx.TypePattern } x => ((TypePatternSyntaxWrapper)x).Type == name,
                    { RawKind: (int)SyntaxKindEx.LocalFunctionStatement } x => ((LocalFunctionStatementSyntaxWrapper)x).ReturnType == name,
                    { RawKind: (int)SyntaxKindEx.DeclarationExpression } x => ((DeclarationExpressionSyntaxWrapper)x).Type == name,
                    { RawKind: (int)SyntaxKind.ParenthesizedLambdaExpression } x => ((ParenthesizedLambdaExpressionSyntaxWrapper)x).ReturnType == name,
                    { RawKind: (int)SyntaxKindEx.FileScopedNamespaceDeclaration } x => ((FileScopedNamespaceDeclarationSyntaxWrapper)x).Name == name,
                    { RawKind: (int)SyntaxKindEx.TupleElement } x => ((TupleElementSyntaxWrapper)x).Type == name,
                    { RawKind: (int)SyntaxKindEx.RefType } x => ((RefTypeSyntaxWrapper)x).Type == name,
                    { RawKind: (int)SyntaxKindEx.ScopedType } x => ((ScopedTypeSyntaxWrapper)x).Type == name,
                    _ => false,
                };
        }

        internal sealed class TriviaClassifier : TriviaClassifierBase
        {
            private static readonly HashSet<SyntaxKind> RegularCommentToken =
                [
                    SyntaxKind.SingleLineCommentTrivia,
                    SyntaxKind.MultiLineCommentTrivia,
                ];

            private static readonly HashSet<SyntaxKind> DocCommentToken =
                [
                    SyntaxKind.SingleLineDocumentationCommentTrivia,
                    SyntaxKind.MultiLineDocumentationCommentTrivia,
                ];

            protected override bool IsRegularComment(SyntaxTrivia trivia) =>
                trivia.IsAnyKind(RegularCommentToken);

            protected override bool IsDocComment(SyntaxTrivia trivia) =>
                trivia.IsAnyKind(DocCommentToken);
        }
    }
}
