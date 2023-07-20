/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
                        c.ReportIssue(CreateDiagnostic(rule, breakNode.GetLocation()));
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
