/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
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

        private SyntaxKind[] TypeDeclarationSyntaxKinds { get; } =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration
        };

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
            node.DescendantNodes(n => !n.IsKind(SyntaxKind.AttributeList))
                .Where(les => les.IsKind(SyntaxKind.StringLiteralExpression))
                .Cast<LiteralExpressionSyntax>();

        protected override SyntaxToken LiteralToken(LiteralExpressionSyntax literal) =>
            literal.Token;

        protected override bool IsNamedTypeOrTopLevelMain(SonarSyntaxNodeReportingContext context) =>
            IsNamedType(context) || context.IsTopLevelMain();
    }
}
