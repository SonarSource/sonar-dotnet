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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StringLiteralShouldNotBeDuplicated : StringLiteralShouldNotBeDuplicatedBase<SyntaxKind, LiteralExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
            SyntaxKind.CompilationUnit
        };

        private HashSet<SyntaxKind> TypeDeclarationSyntaxKinds { get; } =
        [
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration
        ];

        protected override bool IsMatchingMethodParameterName(LiteralExpressionSyntax literalExpression) =>
            literalExpression.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>()
                ?.ParameterList
                ?.Parameters
                .Any(p => p.Identifier.ValueText == literalExpression.Token.ValueText)
            ?? false;

        protected override bool IsInnerInstance(SonarSyntaxNodeReportingContext context) =>
            context.Node.Ancestors().Any(x =>
                x.IsAnyKind(TypeDeclarationSyntaxKinds)
                || (x.IsKind(SyntaxKind.CompilationUnit) && x.ChildNodes().Any(y => y.IsKind(SyntaxKind.GlobalStatement))));

        protected override IEnumerable<LiteralExpressionSyntax> FindLiteralExpressions(SyntaxNode node) =>
            node.DescendantNodes(x => !x.IsKind(SyntaxKind.AttributeList))
                .Where(x => x.IsKind(SyntaxKind.StringLiteralExpression))
                .Cast<LiteralExpressionSyntax>();

        protected override SyntaxToken LiteralToken(LiteralExpressionSyntax literal) =>
            literal.Token;

        protected override bool IsNamedTypeOrTopLevelMain(SonarSyntaxNodeReportingContext context) =>
            IsNamedType(context) || context.IsTopLevelMain();
    }
}
