﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules.CSharp
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

            protected override TokenTypeInfo.Types.TokenInfo ClassifyIdentifier(SyntaxToken token) =>
                // Based on <Kind Name="IdentifierToken"/> in SonarAnalyzer.CFG/ShimLayer\Syntax.xml
                token.Parent switch
                {
                    SimpleNameSyntax x when token == x.Identifier && ClassifySimpleName(x) is TokenType { } tokenType => TokenInfo(token, tokenType),
                    FromClauseSyntax x when token == x.Identifier => null,
                    LetClauseSyntax x when token == x.Identifier => null,
                    JoinClauseSyntax x when token == x.Identifier => null,
                    JoinIntoClauseSyntax x when token == x.Identifier => null,
                    QueryContinuationSyntax x when token == x.Identifier => null,
                    VariableDeclaratorSyntax x when token == x.Identifier => null,
                    LabeledStatementSyntax x when token == x.Identifier => null,
                    ForEachStatementSyntax x when token == x.Identifier => null,
                    CatchDeclarationSyntax x when token == x.Identifier => null,
                    ExternAliasDirectiveSyntax x when token == x.Identifier => null,
                    EnumMemberDeclarationSyntax x when token == x.Identifier => null,
                    MethodDeclarationSyntax x when token == x.Identifier => null,
                    PropertyDeclarationSyntax x when token == x.Identifier => null,
                    EventDeclarationSyntax x when token == x.Identifier => null,
                    AccessorDeclarationSyntax x when token == x.Keyword => null,
                    ParameterSyntax x when token == x.Identifier => null,
                    var x when FunctionPointerUnmanagedCallingConventionSyntaxWrapper.IsInstance(x) && token == ((FunctionPointerUnmanagedCallingConventionSyntaxWrapper)x).Name => null,
                    var x when TupleElementSyntaxWrapper.IsInstance(x) && token == ((TupleElementSyntaxWrapper)x).Identifier => null,
                    var x when LocalFunctionStatementSyntaxWrapper.IsInstance(x) && token == ((LocalFunctionStatementSyntaxWrapper)x).Identifier => null,
                    var x when SingleVariableDesignationSyntaxWrapper.IsInstance(x) && token == ((SingleVariableDesignationSyntaxWrapper)x).Identifier => null,
                    TypeParameterSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    BaseTypeDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    DelegateDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    ConstructorDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    DestructorDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    AttributeTargetSpecifierSyntax x when token == x.Identifier => TokenInfo(token, TokenType.Keyword), // for unknown target specifier [unknown: Obsolete]
                    _ => base.ClassifyIdentifier(token),
                };

            private TokenType? ClassifySimpleName(SimpleNameSyntax x) =>
                IsInTypeContext(x)
                    ? ClassifySimpleNameType(x)
                    : ClassifySimpleNameExpression(x);

            private TokenType? ClassifySimpleNameExpression(SimpleNameSyntax name) =>
                name.Parent switch
                {
                    MemberAccessExpressionSyntax => ClassifyMemberAccess(name),
                    _ => CheckIdentifierExpressionSpecialContext(name, name),
                };

            private TokenType? CheckIdentifierExpressionSpecialContext(SyntaxNode context, SimpleNameSyntax name) =>
                context.Parent switch
                {
                    var x when ConstantPatternSyntaxWrapper.IsInstance(x) => ClassifyIdentifierByModel(name),
                    MemberAccessExpressionSyntax x => CheckIdentifierExpressionSpecialContext(x, name),
                    ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "nameof"} } } } => null,
                    _ => TokenType.UnknownTokentype,
                };

            private TokenType? ClassifyMemberAccess(SimpleNameSyntax name) =>
                name switch
                {
                    { Parent: MemberAccessExpressionSyntax { Parent: not MemberAccessExpressionSyntax } x } when x.Name == name => CheckIdentifierExpressionSpecialContext(x, name),
                    { } x => ClassifyIdentifierByModel(x)
                };

            private TokenType ClassifyIdentifierByModel(SimpleNameSyntax x) =>
                SemanticModel.GetSymbolInfo(x).Symbol is INamedTypeSymbol
                    ? TokenType.TypeName
                    : TokenType.UnknownTokentype;

            private static TokenType? ClassifySimpleNameType(SimpleNameSyntax x) =>
                null;

            private static bool IsInTypeContext(SimpleNameSyntax name) =>
                name.Parent switch
                {
                    QualifiedNameSyntax => true,
                    AliasQualifiedNameSyntax x => x.Name == name,
                    BaseTypeSyntax x => x.Type == name,
                    BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AsExpression or (int)SyntaxKind.IsExpression } x => x.Right == name,
                    ArrayTypeSyntax x => x.ElementType == name,
                    TypeArgumentListSyntax => true,
                    RefValueExpressionSyntax x => x.Type == name,
                    DefaultExpressionSyntax x => x.Type == name,
                    ParameterSyntax x => x.Type == name,
                    TypeOfExpressionSyntax x => x.Type == name,
                    SizeOfExpressionSyntax x => x.Type == name,
                    CastExpressionSyntax x => x.Type == name,
                    ObjectCreationExpressionSyntax x => x.Type == name,
                    StackAllocArrayCreationExpressionSyntax x => x.Type == name,
                    FromClauseSyntax x => x.Type == name,
                    JoinClauseSyntax x => x.Type == name,
                    VariableDeclarationSyntax x => x.Type == name,
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
                    var x when BaseParameterSyntaxWrapper.IsInstance(x) => ((BaseParameterSyntaxWrapper)x).Type == name,
                    var x when DeclarationPatternSyntaxWrapper.IsInstance(x) => ((DeclarationPatternSyntaxWrapper)x).Type == name,
                    var x when RecursivePatternSyntaxWrapper.IsInstance(x) => ((RecursivePatternSyntaxWrapper)x).Type == name,
                    var x when TypePatternSyntaxWrapper.IsInstance(x) => ((TypePatternSyntaxWrapper)x).Type == name,
                    var x when LocalFunctionStatementSyntaxWrapper.IsInstance(x) => ((LocalFunctionStatementSyntaxWrapper)x).ReturnType == name,
                    var x when DeclarationExpressionSyntaxWrapper.IsInstance(x) => ((DeclarationExpressionSyntaxWrapper)x).Type == name,
                    var x when ParenthesizedLambdaExpressionSyntaxWrapper.IsInstance(x) => ((ParenthesizedLambdaExpressionSyntaxWrapper)x).ReturnType == name,
                    _ => false,
                };
        }

        internal sealed class TriviaClassifier : TriviaClassifierBase
        {
            private static readonly SyntaxKind[] RegularCommentToken =
            {
                SyntaxKind.SingleLineCommentTrivia,
                SyntaxKind.MultiLineCommentTrivia,
            };

            private static readonly SyntaxKind[] DocCommentToken =
            {
                SyntaxKind.SingleLineDocumentationCommentTrivia,
                SyntaxKind.MultiLineDocumentationCommentTrivia,
            };

            protected override bool IsRegularComment(SyntaxTrivia trivia) =>
                trivia.IsAnyKind(RegularCommentToken);

            protected override bool IsDocComment(SyntaxTrivia trivia) =>
                trivia.IsAnyKind(DocCommentToken);
        }
    }
}
