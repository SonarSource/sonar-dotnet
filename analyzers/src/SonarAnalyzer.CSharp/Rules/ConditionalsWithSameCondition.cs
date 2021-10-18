/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
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
    public sealed class ConditionalsWithSameCondition : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2760";
        private const string MessageFormat = "This condition was just checked on line {0}.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckMatchingExpressionsInSucceedingStatements<IfStatementSyntax>(c, x => x.Condition),
                SyntaxKind.IfStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckMatchingExpressionsInSucceedingStatements<SwitchStatementSyntax>(c, x => x.Expression),
                SyntaxKind.SwitchStatement);
        }

        private static void CheckMatchingExpressionsInSucceedingStatements<T>(SyntaxNodeAnalysisContext context, Func<T, ExpressionSyntax> expression) where T : StatementSyntax
        {
            var currentStatement = (T)context.Node;
            if (currentStatement.GetPrecedingStatement() is T previousStatement)
            {
                var currentExpression = expression(currentStatement);
                var previousExpression = expression(previousStatement);
                if (CSharpEquivalenceChecker.AreEquivalent(currentExpression, previousExpression) && !ContainsPossibleUpdate(previousStatement, currentExpression, context.SemanticModel))
                {
                    context.ReportIssue(Diagnostic.Create(Rule, currentExpression.GetLocation(), previousExpression.GetLineNumberToReport()));
                }
            }
        }

        private static bool ContainsPossibleUpdate(StatementSyntax statement, ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var checkedSymbols = expression.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Select(x => semanticModel.GetSymbolInfo(x).Symbol).WhereNotNull().ToHashSet();
            var checkedSymbolNames = checkedSymbols.Select(x => x.Name).ToArray();
            var statementDescendents = statement.DescendantNodesAndSelf().ToArray();
            return statementDescendents.OfType<AssignmentExpressionSyntax>().Any(x => HasCheckedSymbol(x.Left))
                || statementDescendents.OfType<PostfixUnaryExpressionSyntax>().Any(x => HasCheckedSymbol(x.Operand))
                || statementDescendents.OfType<PrefixUnaryExpressionSyntax>().Any(x => HasCheckedSymbol(x.Operand));

            bool HasCheckedSymbol(SyntaxNode node) =>
                checkedSymbolNames.Any(node.ToStringContains)
                && semanticModel.GetSymbolInfo(node).Symbol is { } symbol
                && checkedSymbols.Contains(symbol);
        }
    }
}
