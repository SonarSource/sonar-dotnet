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
    public sealed class EmptyMethod : EmptyMethodBase<SyntaxKind>
    {
        internal static readonly SyntaxKind[] SupportedSyntaxKinds =
        {
            SyntaxKind.MethodDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKindEx.InitAccessorDeclaration
        };

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds => SupportedSyntaxKinds;

        protected override void CheckMethod(SonarSyntaxNodeReportingContext context)
        {
            // No need to check for ExpressionBody as arrowed methods can't be empty
            if (context.Node.GetBody() is { } body
                && body.IsEmpty()
                && !ShouldBeExcluded(context, context.Node, context.Node.GetModifiers()))
            {
                context.ReportIssue(Rule, ReportingToken(context.Node));
            }
        }

        private static bool ShouldBeExcluded(SonarSyntaxNodeReportingContext context, SyntaxNode node, SyntaxTokenList modifiers) =>
            modifiers.Any(SyntaxKind.VirtualKeyword) // This quick check only works for methods, for accessors we need to check the symbol
            || (context.SemanticModel.GetDeclaredSymbol(node) is IMethodSymbol symbol
                && (symbol is { IsVirtual: true }
                    || symbol is { IsOverride: true, OverriddenMethod.IsAbstract: true }
                    || !symbol.ExplicitOrImplicitInterfaceImplementations().IsEmpty))
            || (modifiers.Any(SyntaxKind.OverrideKeyword) && context.IsTestProject());

        private static SyntaxToken ReportingToken(SyntaxNode node) =>
            node switch
            {
                MethodDeclarationSyntax method => method.Identifier,
                AccessorDeclarationSyntax accessor => accessor.Keyword,
                _ => ((LocalFunctionStatementSyntaxWrapper)node).Identifier
            };
    }
}
