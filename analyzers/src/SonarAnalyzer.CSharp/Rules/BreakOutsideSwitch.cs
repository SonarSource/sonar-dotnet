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
    public sealed class BreakOutsideSwitch : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1227";
        private const string MessageFormat = "Refactor the code in order to remove this break statement.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var breakNode = (BreakStatementSyntax)c.Node;
                    if (!IsInSwitch(breakNode))
                    {
                        c.ReportIssue(rule, breakNode);
                    }
                },
                SyntaxKind.BreakStatement);
        }

        private static bool IsInSwitch(BreakStatementSyntax node)
        {
            var ancestor = node.FirstAncestorOrSelf<SyntaxNode>(e => LoopOrSwitch.Contains(e.Kind()));

            return ancestor != null && ancestor.IsKind(SyntaxKind.SwitchStatement);
        }

        private static IEnumerable<SyntaxKind> LoopOrSwitch =>
            new[]
                {
                    SyntaxKind.SwitchStatement,
                    SyntaxKind.WhileStatement,
                    SyntaxKind.DoStatement,
                    SyntaxKind.ForStatement,
                    SyntaxKind.ForEachStatement,
                    SyntaxKindEx.ForEachVariableStatement
                };
    }
}
