/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
    public sealed class ForLoopConditionAlwaysFalse : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2252";
        private const string MessageFormat = "This loop will never execute.";

        private static readonly ISet<SyntaxKind> ConditionsToCheck = new HashSet<SyntaxKind>
        {
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression
        };

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var forNode = (ForStatementSyntax)c.Node;
                    if (forNode.Condition != null && (IsAlwaysFalseCondition(forNode.Condition) || IsConditionFalseAtInitialization(forNode)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, forNode.Condition.GetLocation()));
                    }
                },
                SyntaxKind.ForStatement);
        }

        private bool IsAlwaysFalseCondition(ExpressionSyntax condition) =>
            condition.IsKind(SyntaxKind.FalseLiteralExpression)
            || (IsLogicalNot(condition, out var logicalNode)
            && IsAlwaysTrueCondition(logicalNode.Operand.RemoveParentheses()));

        private bool IsAlwaysTrueCondition(ExpressionSyntax condition) =>
            condition.IsKind(SyntaxKind.TrueLiteralExpression)
            || (IsLogicalNot(condition, out var logicalNode)
            && IsAlwaysFalseCondition(logicalNode.Operand.RemoveParentheses()));

        private static bool IsLogicalNot(ExpressionSyntax expression, out PrefixUnaryExpressionSyntax logicalNot)
        {
            var prefixUnaryExpression = expression.RemoveParentheses() as PrefixUnaryExpressionSyntax;

            logicalNot = prefixUnaryExpression;

            return prefixUnaryExpression != null
                && prefixUnaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationToken);
        }

        private bool IsConditionFalseAtInitialization(ForStatementSyntax forNode)
        {
            var condition = forNode.Condition;
            if (!ConditionsToCheck.Contains(condition.Kind()))
            {
                return false;
            }

            var loopInitializerValues = GetLoopInitializerValues(forNode.Declaration);
            loopInitializerValues = loopInitializerValues
                .Union(GetLoopInitializerValues(forNode.Initializers))
                .ToDictionary(d => d.Key, d => d.Value);

            var binaryCondition = (BinaryExpressionSyntax)condition;
            if (GetIntValue(loopInitializerValues, binaryCondition.Left, out var leftValue)
                && GetIntValue(loopInitializerValues, binaryCondition.Right, out var rightValue))
            {
                return !EvaluateCondition(condition.Kind(), leftValue, rightValue);
            }
            return false;
        }

        private bool EvaluateCondition(SyntaxKind syntaxKind, int leftValue, int rightValue)
        {
            var conditionValue = true;
            switch (syntaxKind)
            {
                case SyntaxKind.GreaterThanExpression:
                    conditionValue = leftValue > rightValue;
                    break;
                case SyntaxKind.GreaterThanOrEqualExpression:
                    conditionValue = leftValue >= rightValue;
                    break;
                case SyntaxKind.LessThanExpression:
                    conditionValue = leftValue < rightValue;
                    break;
                case SyntaxKind.LessThanOrEqualExpression:
                    conditionValue = leftValue <= rightValue;
                    break;
                case SyntaxKind.EqualsExpression:
                    conditionValue = leftValue == rightValue;
                    break;
                case SyntaxKind.NotEqualsExpression:
                    conditionValue = leftValue != rightValue;
                    break;
            }
            return conditionValue;
        }

        private bool GetIntValue(IDictionary<string, int> loopInitializerValues, ExpressionSyntax left, out int intValue)
        {
            if (ExpressionNumericConverter.TryGetConstantIntValue(left, out intValue)
                || (left is SimpleNameSyntax simpleName
                && loopInitializerValues.TryGetValue(simpleName.Identifier.ValueText, out intValue)))
            {
                return true;
            }

            intValue = default(int);
            return false;
        }

        private static IDictionary<string, int> GetLoopInitializerValues(VariableDeclarationSyntax variableDeclarationSyntax)
        {
            int x = 0, j = 0;


            var loopInitializerValues = new Dictionary<string, int>();
            if (variableDeclarationSyntax != null)
            {
                foreach (var variableDeclaration in variableDeclarationSyntax.Variables)
                {
                    if (variableDeclaration.Initializer is EqualsValueClauseSyntax initializer
                        && ExpressionNumericConverter.TryGetConstantIntValue(initializer.Value, out var intValue))
                    {
                        loopInitializerValues.Add(variableDeclaration.Identifier.ValueText, intValue);
                    }
                }
            }
            return loopInitializerValues;
        }

        private static IDictionary<string, int> GetLoopInitializerValues(IEnumerable<ExpressionSyntax> initializers)
        {
            var loopInitializerValues = new Dictionary<string, int>();
            if (initializers != null)
            {
                foreach (var initializer in initializers)
                {
                    if (initializer.IsKind(SyntaxKind.SimpleAssignmentExpression)
                        && initializer is AssignmentExpressionSyntax assignment
                        && assignment.Left is SimpleNameSyntax simpleName
                        && ExpressionNumericConverter.TryGetConstantIntValue(assignment.Right, out var intValue))
                    {
                        loopInitializerValues.Add(simpleName.Identifier.ValueText, intValue);
                    }
                }
            }
            return loopInitializerValues;
        }
    }
}
