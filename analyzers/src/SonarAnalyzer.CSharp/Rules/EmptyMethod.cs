/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    public sealed class EmptyMethod : EmptyMethodBase<SyntaxKind>
    {
        internal static readonly HashSet<SyntaxKind> SupportedSyntaxKinds =
        [
            SyntaxKind.MethodDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKindEx.InitAccessorDeclaration
        ];

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override HashSet<SyntaxKind> SyntaxKinds => SupportedSyntaxKinds;

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
            || (context.Model.GetDeclaredSymbol(node) is IMethodSymbol symbol
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
