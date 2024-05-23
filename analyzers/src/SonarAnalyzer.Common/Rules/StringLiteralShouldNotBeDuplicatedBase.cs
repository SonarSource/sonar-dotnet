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

namespace SonarAnalyzer.Rules
{
    public abstract class StringLiteralShouldNotBeDuplicatedBase<TSyntaxKind, TLiteralExpressionSyntax> : ParametrizedDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TLiteralExpressionSyntax : SyntaxNode
    {
        private const string DiagnosticId = "S1192";
        private const string MessageFormat = "Define a constant instead of using this literal '{0}' {1} times.";
        private const int MinimumStringLength = 5;
        private const int ThresholdDefaultValue = 3;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        private readonly DiagnosticDescriptor rule;

        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract bool IsMatchingMethodParameterName(TLiteralExpressionSyntax literalExpression);
        protected abstract bool IsInnerInstance(SonarSyntaxNodeReportingContext context);
        protected abstract IEnumerable<TLiteralExpressionSyntax> FindLiteralExpressions(SyntaxNode node);
        protected abstract SyntaxToken LiteralToken(TLiteralExpressionSyntax literal);

        [RuleParameter("threshold", PropertyType.Integer, "Number of times a literal must be duplicated to trigger an issue.", ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected StringLiteralShouldNotBeDuplicatedBase() =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        protected override void Initialize(SonarParametrizedAnalysisContext context) =>
            // Ideally we would like to report at assembly/project level for the primary and all string instances for secondary
            // locations. The problem is that this scenario is not yet supported on SonarQube side.
            // Hence the decision to do like other languages, at class-level
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, ReportOnViolation, SyntaxKinds);

        protected virtual bool IsNamedTypeOrTopLevelMain(SonarSyntaxNodeReportingContext context) =>
            IsNamedType(context);

        protected static bool IsNamedType(SonarSyntaxNodeReportingContext context) =>
            context.ContainingSymbol.Kind == SymbolKind.NamedType;

        private void ReportOnViolation(SonarSyntaxNodeReportingContext context)
        {
            if (!IsNamedTypeOrTopLevelMain(context) || IsInnerInstance(context))
            {
                // Don't report on inner instances
                return;
            }

            var stringLiterals = FindLiteralExpressions(context.Node);
            var duplicateValuesAndPositions = stringLiterals.Select(x => new { literal = x, literalToken = LiteralToken(x) })
                .Where(x => x.literalToken.ValueText is { Length: >= MinimumStringLength } && !IsMatchingMethodParameterName(x.literal))
                .GroupBy(x => x.literalToken.ValueText, x => x.literalToken)
                .Where(x => x.Count() > Threshold);

            // Report duplications
            foreach (var item in duplicateValuesAndPositions)
            {
                var duplicates = item.ToList();
                var firstToken = duplicates[0];
                context.ReportIssue(rule, firstToken, duplicates.Skip(1).Select(x => x.ToSecondaryLocation()), ExtractStringContent(firstToken), duplicates.Count.ToString());
            }
        }

        private static string ExtractStringContent(SyntaxToken literalToken) =>
             // Use literalToken.Text to get the text as written by the developer. The unescaped text (literalToken.ValueText)
             // might contain control characters that may cause trouble when used as error message (e.g. a null-terminator).
             // The literalToken.Text contains leading and trailing double quotes that we strip of.
             literalToken.Text.StartsWith("@\"") ? literalToken.Text.Substring(2, literalToken.Text.Length - 3) : literalToken.Text.Substring(1, literalToken.Text.Length - 2);
    }
}
