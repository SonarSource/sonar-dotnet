/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UnconditionalJumpStatement : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1751";
        private const string MessageFormat = "Remove this '{0}' statement or make it conditional.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> loopStatements = new HashSet<SyntaxKind>
        {
            SyntaxKind.ForEachStatement,
            SyntaxKind.ForStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement
        };

        private static readonly ISet<SyntaxKind> StatementsThatCanThrow = ImmutableHashSet.Create(
            SyntaxKind.InvocationExpression,
            SyntaxKind.ObjectCreationExpression);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var walker = new LoopWalker(c);
                walker.Visit(c.Node);
                foreach (var node in walker.GetRuleViolations())
                {
                    c.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation(), GetKeywordText(node)));
                }
            },
            loopStatements.ToArray());
        }

        private static string GetKeywordText(StatementSyntax statement) =>
            (statement as BreakStatementSyntax)?.BreakKeyword.ToString() ??
            (statement as ContinueStatementSyntax)?.ContinueKeyword.ToString() ??
            (statement as ReturnStatementSyntax)?.ReturnKeyword.ToString() ??
            (statement as ThrowStatementSyntax)?.ThrowKeyword.ToString();

        private class LoopWalker : CSharpSyntaxWalker
        {
            private readonly SyntaxNode rootExpression;

            private readonly List<StatementSyntax> conditionalContinues = new List<StatementSyntax>();
            private readonly List<StatementSyntax> conditionalTerminates = new List<StatementSyntax>();

            private readonly List<StatementSyntax> unconditionalContinues = new List<StatementSyntax>();
            private readonly List<StatementSyntax> unconditionalTerminates = new List<StatementSyntax>();

            public LoopWalker(SyntaxNodeAnalysisContext context)
            {
                rootExpression = context.Node;
            }

            public List<StatementSyntax> GetRuleViolations()
            {
                var ruleViolations = new List<StatementSyntax>();

                ruleViolations.AddRange(unconditionalContinues);

                if (!conditionalContinues.Any())
                {
                    ruleViolations.AddRange(unconditionalTerminates);
                }

                return ruleViolations;
            }

            public override void VisitContinueStatement(ContinueStatementSyntax node)
            {
                base.VisitContinueStatement(node);
                StoreVisitData(node, conditionalContinues, unconditionalContinues);
            }

            public override void VisitBreakStatement(BreakStatementSyntax node)
            {
                base.VisitBreakStatement(node);
                StoreVisitData(node, conditionalTerminates, unconditionalTerminates);
            }

            public override void VisitReturnStatement(ReturnStatementSyntax node)
            {
                base.VisitReturnStatement(node);
                StoreVisitData(node, conditionalTerminates, unconditionalTerminates);
            }

            public override void VisitThrowStatement(ThrowStatementSyntax node)
            {
                base.VisitThrowStatement(node);
                StoreVisitData(node, conditionalTerminates, unconditionalTerminates);
            }

            private void StoreVisitData(StatementSyntax node, List<StatementSyntax> conditionalCollection,
                List<StatementSyntax> unconditionalCollection)
            {
                var ancestors = node
                    .Ancestors()
                    .TakeWhile(n => !rootExpression.Equals(n))
                    .ToList();

                if (ancestors.Any(n => n.IsAnyKind(lambdaOrLoop)))
                {
                    return;
                }

                if (ancestors.Any(n => n.IsAnyKind(conditionalStatements)) ||
                    IsInTryCatchWithMethodInvocation(node, ancestors))
                {
                    conditionalCollection.Add(node);
                }
                else
                {
                    unconditionalCollection.Add(node);
                }
            }

            private static bool IsInTryCatchWithMethodInvocation(StatementSyntax node, List<SyntaxNode> ancestors)
            {
                var tryAncestor = (TryStatementSyntax)ancestors.FirstOrDefault(n => n.IsKind(SyntaxKind.TryStatement));

                if (tryAncestor == null ||
                    tryAncestor.Catches.Count == 0)
                {
                    return false;
                }

                if (node.IsKind(SyntaxKind.ReturnStatement) &&
                    node.DescendantNodes().Any(n => n.IsAnyKind(StatementsThatCanThrow)))
                {
                    return true;
                }

                return tryAncestor.Block.Statements
                    .TakeWhile(statement => !statement.Equals(node))
                    .SelectMany(statement => statement.DescendantNodes())
                    .Any(statement => statement.IsAnyKind(StatementsThatCanThrow));
            }

            private static readonly ISet<SyntaxKind> lambdaOrLoop
                = new HashSet<SyntaxKind>(
                    loopStatements.Union(
                        new[] { SyntaxKind.ParenthesizedLambdaExpression,
                                SyntaxKind.SimpleLambdaExpression}));

            private static readonly ISet<SyntaxKind> conditionalStatements = new HashSet<SyntaxKind>
            {
                SyntaxKind.IfStatement,
                SyntaxKind.SwitchStatement,
                SyntaxKind.CatchClause
            };
        };
    }
}
