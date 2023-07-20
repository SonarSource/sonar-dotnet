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

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var forNode = (ForStatementSyntax)c.Node;
                    if (forNode.Condition != null && (IsAlwaysFalseCondition(forNode.Condition) || IsConditionFalseAtInitialization(forNode)))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, forNode.Condition.GetLocation()));
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

            var variableNameToDecimalMapping = loopVariableDeclarationMapping
                .Union(loopInitializerMapping)
                .ToDictionary(d => d.Key, d => d.Value);

            var binaryCondition = (BinaryExpressionSyntax)condition;
            if (DecimalValue(variableNameToDecimalMapping, binaryCondition.Left, out var leftValue)
                && DecimalValue(variableNameToDecimalMapping, binaryCondition.Right, out var rightValue))
            {
                return !ConditionIsTrue(condition.Kind(), leftValue, rightValue);
            }
            return false;
        }

        private static bool ConditionIsTrue(SyntaxKind syntaxKind, decimal leftValue, decimal rightValue) =>
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

        private static bool DecimalValue(IDictionary<string, decimal> variableNameToDecimalValue, ExpressionSyntax expression, out decimal parsedValue)
        {
            if (ExpressionNumericConverter.TryGetConstantDecimalValue(expression, out parsedValue)
                || (expression is SimpleNameSyntax simpleName
                    && variableNameToDecimalValue.TryGetValue(simpleName.Identifier.ValueText, out parsedValue)))
            {
                return true;
            }

            parsedValue = default;
            return false;
        }

        /// <summary>
        /// Retrieves the mapping of variable names to their decimal value from the variable declaration part of a for loop.
        /// This will find the mapping for such cases:
        /// <code>
        /// for (var i = 0;;) {}
        /// </code>
        /// </summary>
        private static IDictionary<string, decimal> VariableDeclarationMapping(VariableDeclarationSyntax variableDeclarationSyntax)
        {
            var loopInitializerValues = new Dictionary<string, decimal>();
            if (variableDeclarationSyntax != null)
            {
                foreach (var variableDeclaration in variableDeclarationSyntax.Variables)
                {
                    if (variableDeclaration.Initializer is { } initializer
                        && ExpressionNumericConverter.TryGetConstantDecimalValue(initializer.Value, out var decimalValue))
                    {
                        loopInitializerValues.Add(variableDeclaration.Identifier.ValueText, decimalValue);
                    }
                }
            }
            return loopInitializerValues;
        }

        /// <summary>
        /// Retrieves the mapping of variable names to their decimal value from the initializer part of a for loop.
        /// This will find the mapping for such cases:
        /// <code>
        /// int i;
        /// for (i = 0;;) {}
        /// </code>
        /// </summary>
        private static IDictionary<string, decimal> LoopInitializerMapping(IEnumerable<ExpressionSyntax> initializers)
        {
            var loopInitializerValues = new Dictionary<string, decimal>();
            if (initializers != null)
            {
                foreach (var initializer in initializers)
                {
                    if (initializer.IsKind(SyntaxKind.SimpleAssignmentExpression)
                        && initializer is AssignmentExpressionSyntax { Left: SimpleNameSyntax simpleName } assignment
                        && ExpressionNumericConverter.TryGetConstantDecimalValue(assignment.Right, out var decimalValue))
                    {
                        loopInitializerValues.Add(simpleName.Identifier.ValueText, decimalValue);
                    }
                }
            }
            return loopInitializerValues;
        }
    }
}
