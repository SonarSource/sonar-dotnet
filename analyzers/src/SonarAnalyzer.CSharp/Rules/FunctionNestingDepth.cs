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
    public class FunctionNestingDepth : FunctionNestingDepthBase
    {
        protected override ILanguageFacade Language => CSharpFacade.Instance;

        protected override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var walker = new NestingDepthWalker(Maximum, token => c.ReportIssue(CreateDiagnostic(rule, token.GetLocation(), Maximum)));
                walker.SafeVisit(c.Node);
            },
            SyntaxKind.MethodDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKindEx.InitAccessorDeclaration,
            SyntaxKind.AddAccessorDeclaration,
            SyntaxKind.RemoveAccessorDeclaration,
            SyntaxKind.GlobalStatement);

        private sealed class NestingDepthWalker : SafeCSharpSyntaxWalker
        {
            private readonly NestingDepthCounter counter;

            public NestingDepthWalker(int maximumNestingDepth, Action<SyntaxToken> actionMaximumExceeded) =>
                counter = new NestingDepthCounter(maximumNestingDepth, actionMaximumExceeded);

            public override void VisitIfStatement(IfStatementSyntax node)
            {
                var isPartOfChainedElseIfClause = node.Parent != null && node.Parent.IsKind(SyntaxKind.ElseClause);
                if (isPartOfChainedElseIfClause)
                {
                    base.VisitIfStatement(node);
                }
                else
                {
                    counter.CheckNesting(node.IfKeyword, () => base.VisitIfStatement(node));
                }
            }

            public override void VisitForStatement(ForStatementSyntax node) =>
                counter.CheckNesting(node.ForKeyword, () => base.VisitForStatement(node));

            public override void VisitForEachStatement(ForEachStatementSyntax node) =>
                counter.CheckNesting(node.ForEachKeyword, () => base.VisitForEachStatement(node));

            public override void VisitWhileStatement(WhileStatementSyntax node) =>
                counter.CheckNesting(node.WhileKeyword, () => base.VisitWhileStatement(node));

            public override void VisitDoStatement(DoStatementSyntax node) =>
                counter.CheckNesting(node.DoKeyword, () => base.VisitDoStatement(node));

            public override void VisitSwitchStatement(SwitchStatementSyntax node) =>
                counter.CheckNesting(node.SwitchKeyword, () => base.VisitSwitchStatement(node));

            public override void VisitTryStatement(TryStatementSyntax node) =>
                counter.CheckNesting(node.TryKeyword, () => base.VisitTryStatement(node));
        }
    }
}
