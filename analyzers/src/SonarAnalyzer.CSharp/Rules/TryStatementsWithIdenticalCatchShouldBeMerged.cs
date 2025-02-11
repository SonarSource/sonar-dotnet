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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TryStatementsWithIdenticalCatchShouldBeMerged : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2327";
        private const string MessageFormat = "Combine this 'try' with the one starting on line {0}.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
            {
                var tryStatement = (TryStatementSyntax)c.Node;

                var mergeableTry = tryStatement.GetPreviousStatementsCurrentBlock()
                    .OfType<TryStatementSyntax>()
                    .FirstOrDefault(t => SameCatches(t.Catches) && SameFinally(t));

                if (mergeableTry != null)
                {
                    c.ReportIssue(rule, tryStatement, messageArgs: mergeableTry.GetLineNumberToReport().ToString());
                }

                bool SameCatches(IReadOnlyCollection<CatchClauseSyntax> other) =>
                    tryStatement.Catches.Count == other.Count &&
                    tryStatement.Catches.All(x => other.Any(o => o.IsEquivalentTo(x)));

                bool SameFinally(TryStatementSyntax other) =>
                    tryStatement.Finally == null && other.Finally == null ||
                    tryStatement.Finally != null && other.Finally != null && tryStatement.Finally.IsEquivalentTo(other.Finally);

            },
            SyntaxKind.TryStatement);
        }
    }
}
