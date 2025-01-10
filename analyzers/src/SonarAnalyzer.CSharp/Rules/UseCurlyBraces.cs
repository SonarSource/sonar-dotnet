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
    public sealed class UseCurlyBraces : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S121";
        private const string MessageFormat = "Add curly braces around the nested statement(s) in this '{0}' block.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private sealed class CheckedKind
        {
            public SyntaxKind Kind { get; set; }
            public string Value { get; set; }
            public Func<SyntaxNode, bool> Validator { get; set; }
            public Func<SyntaxNode, Location> IssueReportLocation { get; set; }
        }

        private static readonly ImmutableList<CheckedKind> CheckedKinds = ImmutableList.Create(
            new CheckedKind
            {
                Kind = SyntaxKind.IfStatement,
                Value = "if",
                Validator = node => ((IfStatementSyntax)node).Statement.IsKind(SyntaxKind.Block),
                IssueReportLocation = node => ((IfStatementSyntax)node).IfKeyword.GetLocation()
            },
            new CheckedKind
            {
                Kind = SyntaxKind.ElseClause,
                Value = "else",
                Validator =
                    node =>
                    {
                        var statement = ((ElseClauseSyntax)node).Statement;
                        return statement.IsKind(SyntaxKind.IfStatement) || statement.IsKind(SyntaxKind.Block);
                    },
                IssueReportLocation = node => ((ElseClauseSyntax)node).ElseKeyword.GetLocation()
            },
            new CheckedKind
            {
                Kind = SyntaxKind.ForStatement,
                Value = "for",
                Validator = node => ((ForStatementSyntax)node).Statement.IsKind(SyntaxKind.Block),
                IssueReportLocation = node => ((ForStatementSyntax)node).ForKeyword.GetLocation()
            },
            new CheckedKind
            {
                Kind = SyntaxKind.ForEachStatement,
                Value = "foreach",
                Validator = node => ((ForEachStatementSyntax)node).Statement.IsKind(SyntaxKind.Block),
                IssueReportLocation = node => ((ForEachStatementSyntax)node).ForEachKeyword.GetLocation()
            },
            new CheckedKind
            {
                Kind = SyntaxKind.DoStatement,
                Value = "do",
                Validator = node => ((DoStatementSyntax)node).Statement.IsKind(SyntaxKind.Block),
                IssueReportLocation = node => ((DoStatementSyntax)node).DoKeyword.GetLocation()
            },
            new CheckedKind
            {
                Kind = SyntaxKind.WhileStatement,
                Value = "while",
                Validator = node => ((WhileStatementSyntax)node).Statement.IsKind(SyntaxKind.Block),
                IssueReportLocation = node => ((WhileStatementSyntax)node).WhileKeyword.GetLocation()
            });

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var checkedKind = CheckedKinds.Single(e => c.Node.IsKind(e.Kind));

                    if (!checkedKind.Validator(c.Node))
                    {
                        c.ReportIssue(rule, checkedKind.IssueReportLocation(c.Node), checkedKind.Value);
                    }
                },
                CheckedKinds.Select(e => e.Kind).ToArray());
        }
    }
}
