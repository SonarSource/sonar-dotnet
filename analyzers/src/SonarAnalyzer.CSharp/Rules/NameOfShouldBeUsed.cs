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
