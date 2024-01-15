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
    public sealed class NameOfShouldBeUsed : NameOfShouldBeUsedBase<BaseMethodDeclarationSyntax, SyntaxKind, ThrowStatementSyntax>
    {
        private static readonly HashSet<SyntaxKind> StringTokenTypes = new HashSet<SyntaxKind>
        {
            SyntaxKind.InterpolatedStringTextToken,
            SyntaxKind.StringLiteralToken,
            SyntaxKindEx.SingleLineRawStringLiteralToken,
            SyntaxKindEx.MultiLineRawStringLiteralToken
        };

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override string NameOf => "nameof";

        protected override BaseMethodDeclarationSyntax MethodSyntax(SyntaxNode node) =>
             (BaseMethodDeclarationSyntax)node;

        protected override bool IsStringLiteral(SyntaxToken t) =>
            t.IsAnyKind(StringTokenTypes);

        protected override IEnumerable<string> GetParameterNames(BaseMethodDeclarationSyntax method)
        {
            var paramGroups = method.ParameterList?.Parameters.GroupBy(p => p.Identifier.ValueText);
            return paramGroups == null || paramGroups.Any(g => g.Count() != 1)
                ? Enumerable.Empty<string>()
                : paramGroups.Select(g => g.First().Identifier.ValueText);
        }

        protected override bool LeastLanguageVersionMatches(SonarSyntaxNodeReportingContext context) =>
            context.Compilation.IsAtLeastLanguageVersion(LanguageVersion.CSharp6);

        protected override bool IsArgumentExceptionCallingNameOf(SyntaxNode node, IEnumerable<string> arguments) =>
            ((ThrowStatementSyntax)node).Expression is ObjectCreationExpressionSyntax objectCreation
            && ArgumentExceptionNameOfPosition(objectCreation.Type.ToString()) is var idx
            && objectCreation.ArgumentList?.Arguments is { } creationArguments
            && creationArguments.Count >= idx + 1
            && creationArguments[idx].Expression is InvocationExpressionSyntax invocation
            && invocation.Expression.ToString() == "nameof"
            && invocation.ArgumentList.Arguments.Count == 1
            && arguments.Contains(invocation.ArgumentList.Arguments[0].Expression.ToString());
    }
}
