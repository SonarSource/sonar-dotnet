﻿/*
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

namespace SonarAnalyzer.CSharp.Rules
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
                    if (forNode.Condition is not null && (IsAlwaysFalseCondition(forNode.Condition) || IsConditionFalseAtInitialization(forNode)))
                    {
                        c.ReportIssue(Rule, forNode.Condition);
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
            if (DecimalValue(variableNameToDecimalMapping, binaryCondition.Left) is { } leftValue
                && DecimalValue(variableNameToDecimalMapping, binaryCondition.Right) is { } rightValue)
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
            return prefixUnaryExpression is not null && prefixUnaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationToken);
        }

        private static decimal? DecimalValue(IDictionary<string, decimal> variableNameToDecimalValue, ExpressionSyntax expression) =>
            ExpressionNumericConverter.ConstantDecimalValue(expression) is { } parsedValue
                || (expression is SimpleNameSyntax simpleName && variableNameToDecimalValue.TryGetValue(simpleName.Identifier.ValueText, out parsedValue))
                    ? parsedValue
                    : (decimal?)null;

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
            if (variableDeclarationSyntax is not null)
            {
                foreach (var variableDeclaration in variableDeclarationSyntax.Variables)
                {
                    if (variableDeclaration.Initializer is { } initializer
                        && ExpressionNumericConverter.ConstantDecimalValue(initializer.Value) is { } decimalValue)
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
            if (initializers is not null)
            {
                foreach (var initializer in initializers)
                {
                    if (initializer.IsKind(SyntaxKind.SimpleAssignmentExpression)
                        && initializer is AssignmentExpressionSyntax { Left: SimpleNameSyntax simpleName } assignment
                        && ExpressionNumericConverter.ConstantDecimalValue(assignment.Right) is { } decimalValue)
                    {
                        loopInitializerValues.Add(simpleName.Identifier.ValueText, decimalValue);
                    }
                }
            }
            return loopInitializerValues;
        }
    }
}
