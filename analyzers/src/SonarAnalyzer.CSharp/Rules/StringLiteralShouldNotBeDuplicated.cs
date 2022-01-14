/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

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
            SyntaxKindEx.RecordClassDeclaration,
            SyntaxKind.CompilationUnit
        };

        private SyntaxKind[] TypeDeclarationSyntaxKinds { get; } =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordClassDeclaration
        };

        protected override bool IsMatchingMethodParameterName(LiteralExpressionSyntax literalExpression) =>
            literalExpression.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>()
                ?.ParameterList
                ?.Parameters
                .Any(p => p.Identifier.ValueText == literalExpression.Token.ValueText)
            ?? false;

        protected override bool IsInnerInstance(SyntaxNodeAnalysisContext context) =>
            context.Node.Ancestors().Any(x =>
                x.IsAnyKind(TypeDeclarationSyntaxKinds)
                || (x.IsKind(SyntaxKind.CompilationUnit) && x.ChildNodes().Any(y => y.IsKind(SyntaxKind.GlobalStatement))));

        protected override IEnumerable<LiteralExpressionSyntax> RetrieveLiteralExpressions(SyntaxNode node) =>
            node.DescendantNodes(n => !n.IsKind(SyntaxKind.AttributeList))
                .Where(les => les.IsKind(SyntaxKind.StringLiteralExpression))
                .Cast<LiteralExpressionSyntax>();

        protected override string GetLiteralValue(LiteralExpressionSyntax literal) =>
            literal.Token.Text;

        protected override bool IsNamedTypeOrTopLevelMain(SyntaxNodeAnalysisContext context) =>
            IsNamedType(context) || IsTopLevelMain(context);

        private static bool IsTopLevelMain(SyntaxNodeAnalysisContext context) =>
            context.ContainingSymbol.IsTopLevelMain();
    }
}
