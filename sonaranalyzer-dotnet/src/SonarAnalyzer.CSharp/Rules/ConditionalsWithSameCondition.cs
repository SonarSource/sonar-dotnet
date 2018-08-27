/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ConditionalsWithSameCondition : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2760";
        private const string MessageFormat = "This condition was just checked on line {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    CheckMatchingExpressionsInSucceedingStatements((IfStatementSyntax)c.Node, syntax => syntax.Condition, c);
                },
                SyntaxKind.IfStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    CheckMatchingExpressionsInSucceedingStatements((SwitchStatementSyntax)c.Node, syntax => syntax.Expression, c);
                },
                SyntaxKind.SwitchStatement);
        }

        private static void CheckMatchingExpressionsInSucceedingStatements<T>(T statement,
            Func<T, ExpressionSyntax> expressionSelector, SyntaxNodeAnalysisContext context) where T : StatementSyntax
        {
            if (!(statement.GetPrecedingStatement() is T previousStatement))
            {
                return;
            }

            var currentExpression = expressionSelector(statement);
            var previousExpression = expressionSelector(previousStatement);

            if (EquivalenceChecker.AreEquivalent(currentExpression, previousExpression) &&
                !ContainsPossibleUpdate(previousStatement, currentExpression, context.SemanticModel))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, currentExpression.GetLocation(),
                    previousExpression.GetLineNumberToReport()));
            }
        }

        private static bool ContainsPossibleUpdate(StatementSyntax statement, ExpressionSyntax expression,
            SemanticModel semanticModel)
        {
            var checkedSymbols = expression.DescendantNodesAndSelf()
                .Select(node => semanticModel.GetSymbolInfo(node).Symbol)
                .WhereNotNull()
                .ToHashSet();

            var statementDescendents = statement.DescendantNodesAndSelf().ToList();
            var isAnyCheckedSymbolUpdated = statementDescendents
                .OfType<AssignmentExpressionSyntax>()
                .Any(assignment => IsCheckedSymbolUpdated(assignment.Left, checkedSymbols, semanticModel));

            if (isAnyCheckedSymbolUpdated)
            {
                return true;
            }

            var postfixUnaryExpression = statementDescendents
                .OfType<PostfixUnaryExpressionSyntax>()
                .Any(expressionSyntax =>
                {
                    var symbol = semanticModel.GetSymbolInfo(expressionSyntax.Operand).Symbol;
                    return symbol != null && checkedSymbols.Contains(symbol);
                });

            if (postfixUnaryExpression)
            {
                return true;
            }

            var prefixUnaryExpression = statementDescendents
                .OfType<PrefixUnaryExpressionSyntax>()
                .Any(expressionSyntax =>
                {
                    var symbol = semanticModel.GetSymbolInfo(expressionSyntax.Operand).Symbol;
                    return symbol != null && checkedSymbols.Contains(symbol);
                });

            return prefixUnaryExpression;
        }

        private static bool IsCheckedSymbolUpdated(ExpressionSyntax expression, ISet<ISymbol> checkedSymbols, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetSymbolInfo(expression).Symbol;
            return symbol != null && checkedSymbols.Contains(symbol);
        }
    }
}
