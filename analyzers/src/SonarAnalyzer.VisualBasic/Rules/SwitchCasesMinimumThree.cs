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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class SwitchCasesMinimumThree : SwitchCasesMinimumThreeBase
    {
        private const string MessageFormat = "Replace this 'Select' statement with 'If' statements to increase readability.";
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var selectNode = (SelectBlockSyntax)c.Node;
                    if (!HasAtLeastThreeLabels(selectNode))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, selectNode.SelectStatement.SelectKeyword.GetLocation()));
                    }
                },
                SyntaxKind.SelectBlock);
        }

        private static bool HasAtLeastThreeLabels(SelectBlockSyntax node) =>
            node.CaseBlocks.Sum(caseBlock => caseBlock.CaseStatement.Cases.Count) >= 3;
    }
}
