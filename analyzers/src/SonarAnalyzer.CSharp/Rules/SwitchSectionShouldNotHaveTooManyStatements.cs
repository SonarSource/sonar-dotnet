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
    public sealed class SwitchSectionShouldNotHaveTooManyStatements : SwitchSectionShouldNotHaveTooManyStatementsBase
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var switchSection = (SwitchSectionSyntax)c.Node;

                    if (switchSection.IsMissing ||
                        switchSection.Labels.Count <= 0)
                    {
                        return;
                    }

                    var statementsCount = switchSection.Statements.SelectMany(GetSubStatements).Count();
                    if (statementsCount > Threshold)
                    {
                        c.ReportIssue(rule, switchSection.Labels.First(), "switch section", statementsCount.ToString(), Threshold.ToString(), "method");
                    }
                },
                SyntaxKind.SwitchSection);
        }

        private IEnumerable<StatementSyntax> GetSubStatements(StatementSyntax statement) =>
            statement.DescendantNodesAndSelf()
                .OfType<StatementSyntax>()
                .Where(s => !(s is BlockSyntax));
    }
}
