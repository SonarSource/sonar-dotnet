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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class NameOfShouldBeUsed : NameOfShouldBeUsedBase<MethodBlockBaseSyntax, SyntaxKind, ThrowStatementSyntax>
    {
        private static readonly HashSet<SyntaxKind> StringTokenTypes = new HashSet<SyntaxKind>
        {
            SyntaxKind.InterpolatedStringTextToken,
            SyntaxKind.StringLiteralToken
        };

        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;
        protected override string NameOf => "NameOf";

        protected override MethodBlockBaseSyntax MethodSyntax(SyntaxNode node) =>
            node.AncestorsAndSelf().OfType<MethodBlockBaseSyntax>().FirstOrDefault();

        protected override bool IsStringLiteral(SyntaxToken t) =>
            t.IsAnyKind(StringTokenTypes);

        protected override IEnumerable<string> GetParameterNames(MethodBlockBaseSyntax method)
        {
            var paramGroups = method?.BlockStatement.ParameterList?.Parameters.GroupBy(x => x.Identifier.Identifier.ValueText);
            return paramGroups == null || paramGroups.Any(x => x.Count() != 1)
                ? Enumerable.Empty<string>()
                : paramGroups.Select(x => x.First().Identifier.Identifier.ValueText);
        }

        protected override bool LeastLanguageVersionMatches(SonarSyntaxNodeReportingContext context) =>
            context.Compilation.IsAtLeastLanguageVersion(LanguageVersion.VisualBasic14);

        protected override bool IsArgumentExceptionCallingNameOf(SyntaxNode node, IEnumerable<string> arguments) =>
            ((ThrowStatementSyntax)node).Expression is ObjectCreationExpressionSyntax objectCreation
            && ArgumentExceptionNameOfPosition(objectCreation.Type.ToString()) is var idx
            && objectCreation.ArgumentList?.Arguments is { } creationArguments
            && creationArguments.Count >= idx + 1
            && creationArguments[idx].GetExpression() is NameOfExpressionSyntax nameOfExpression
            && arguments.Contains(nameOfExpression.Argument.ToString());
    }
}
