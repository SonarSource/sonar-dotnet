/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ForLoopConditionAlwaysFalse : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2252";
        private const string MessageFormat = "This loop will never execute.";

        private static readonly CSharpExpressionNumericConverter ExpressionNumericConverter = new();

        private static readonly ISet<SyntaxKind> ConditionsToCheck = new HashSet<SyntaxKind>
        {
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression
        };

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var forNode = (ForStatementSyntax)c.Node;
                    if (forNode.Condition != null && (IsAlwaysFalseCondition(forNode.Condition) || IsConditionFalseAtInitialization(forNode)))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, forNode.Condition.GetLocation()));
                    }
                },
                SyntaxKind.ForStatement);

        private bool IsAlwaysFalseCondition(ExpressionSyntax condition) =>
            condition.IsKind(SyntaxKind.FalseLiteralExpression)
            || (IsLogicalNot(condition, out var logicalNode)
                && IsAlwaysTrueCondition(logicalNode.Operand.RemoveParentheses()));

        private bool IsAlwaysTrueCondition(ExpressionSyntax condition) =>
            condition.IsKind(SyntaxKind.TrueLiteralExpression)
            || (IsLogicalNot(condition, out var logicalNode)
                && IsAlwaysFalseCondition(logicalNode.Operand.RemoveParentheses()));

        private static bool IsConditionFalseAtInitialization(ForStatementSyntax forNode)
        {
            var condition = forNode.Condition;
            if (!ConditionsToCheck.Contains(condition.Kind()))
            {
                return false;
            }

            var loopVariableDeclarationMapping = VariableDeclarationMapping(forNode.Declaration);
            var loopInitializerMapping = LoopInitializerMapping(forNode.Initializers);

            var variableNameToDoubleMapping = loopVariableDeclarationMapping
                .Union(loopInitializerMapping)
                .ToDictionary(d => d.Key, d => d.Value);

            var binaryCondition = (BinaryExpressionSyntax)condition;
            if (DoubleValue(variableNameToDoubleMapping, binaryCondition.Left, out var leftValue)
                && DoubleValue(variableNameToDoubleMapping, binaryCondition.Right, out var rightValue))
            {
                return !ConditionIsTrue(condition.Kind(), leftValue, rightValue);
            }
            return false;
        }

        private static bool ConditionIsTrue(SyntaxKind syntaxKind, double leftValue, double rightValue) =>
            syntaxKind switch
            {
                SyntaxKind.GreaterThanExpression => leftValue > rightValue,
                SyntaxKind.GreaterThanOrEqualExpression => leftValue >= rightValue,
                SyntaxKind.LessThanExpression => leftValue < rightValue,
                SyntaxKind.LessThanOrEqualExpression => leftValue <= rightValue,
                SyntaxKind.EqualsExpression => leftValue == rightValue,
                SyntaxKind.NotEqualsExpression => leftValue != rightValue,
                _ => true
            };

        private static bool IsLogicalNot(ExpressionSyntax expression, out PrefixUnaryExpressionSyntax logicalNot)
        {
            var prefixUnaryExpression = expression.RemoveParentheses() as PrefixUnaryExpressionSyntax;

            logicalNot = prefixUnaryExpression;

            return prefixUnaryExpression != null
                && prefixUnaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationToken);
        }

        private static bool DoubleValue(IDictionary<string, double> variableNameToDoubleValue, ExpressionSyntax expression, out double parsedValue)
        {
            if (ExpressionNumericConverter.TryGetConstantDoubleValue(expression, out parsedValue)
                || (expression is SimpleNameSyntax simpleName
                    && variableNameToDoubleValue.TryGetValue(simpleName.Identifier.ValueText, out parsedValue)))
            {
                return true;
            }

            parsedValue = default;
            return false;
        }

        /// <summary>
        /// Retrieves the mapping of variable names to their double value from the variable declaration part of a for loop.
        /// This will find the mapping for such cases:
        /// <code>
        /// for (var i = 0;;) {}
        /// </code>
        /// </summary>
        private static IDictionary<string, double> VariableDeclarationMapping(VariableDeclarationSyntax variableDeclarationSyntax)
        {
            var loopInitializerValues = new Dictionary<string, double>();
            if (variableDeclarationSyntax != null)
            {
                foreach (var variableDeclaration in variableDeclarationSyntax.Variables)
                {
                    if (variableDeclaration.Initializer is EqualsValueClauseSyntax initializer
                        && ExpressionNumericConverter.TryGetConstantDoubleValue(initializer.Value, out var doubleValue))
                    {
                        loopInitializerValues.Add(variableDeclaration.Identifier.ValueText, doubleValue);
                    }
                }
            }
            return loopInitializerValues;
        }

        /// <summary>
        /// Retrieves the mapping of variable names to their double value from the initializer part of a for loop.
        /// This will find the mapping for such cases:
        /// <code>
        /// int i;
        /// for (i = 0;;) {}
        /// </code>
        /// </summary>
        private static IDictionary<string, double> LoopInitializerMapping(IEnumerable<ExpressionSyntax> initializers)
        {
            var loopInitializerValues = new Dictionary<string, double>();
            if (initializers != null)
            {
                foreach (var initializer in initializers)
                {
                    if (initializer.IsKind(SyntaxKind.SimpleAssignmentExpression)
                        && initializer is AssignmentExpressionSyntax assignment
                        && assignment.Left is SimpleNameSyntax simpleName
                        && ExpressionNumericConverter.TryGetConstantDoubleValue(assignment.Right, out var doubleValue))
                    {
                        loopInitializerValues.Add(simpleName.Identifier.ValueText, doubleValue);
                    }
                }
            }
            return loopInitializerValues;
        }
    }
}
