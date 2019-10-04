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
    public sealed class ForLoopIncrementSignCheck : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2251";
        private const string MessageFormat = "'{0}' is {1}remented and will never reach 'stop condition'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private enum ArithmeticOperation
        {
            None,
            Addition,
            Substraction
        }

        private enum Condition
        {
            None,
            Less,
            Greater
        }

        private readonly Dictionary<SyntaxKind, ArithmeticOperation> incrementorToOperation = new Dictionary<SyntaxKind, ArithmeticOperation>
        {
            { SyntaxKind.PreDecrementExpression, ArithmeticOperation.Substraction },
            { SyntaxKind.PreIncrementExpression, ArithmeticOperation.Addition },
            { SyntaxKind.PostDecrementExpression, ArithmeticOperation.Substraction },
            { SyntaxKind.PostIncrementExpression, ArithmeticOperation.Addition },
            { SyntaxKind.SubtractAssignmentExpression, ArithmeticOperation.Substraction },
            { SyntaxKind.AddAssignmentExpression, ArithmeticOperation.Addition },
            { SyntaxKind.AddExpression, ArithmeticOperation.Addition },
            { SyntaxKind.SubtractExpression, ArithmeticOperation.Substraction }
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var forNode = (ForStatementSyntax)c.Node;

                    var conditionSyntax = forNode.Condition;
                    if (conditionSyntax == null || !(conditionSyntax is BinaryExpressionSyntax binaryCondition))
                    {
                        return;
                    }

                    if (forNode.Incrementors.Count != 1)
                    {
                        return;
                    }

                    var incrementor = forNode.Incrementors[0];

                    var incrementorData = GetIncrementData(incrementor);
                    if (incrementorData.Operation == ArithmeticOperation.None)
                    {
                        return;
                    }

                    var condition = GetCondition(binaryCondition, incrementorData.IdentifierName);
                    if (condition == Condition.None)
                    {
                        return;
                    }

                    if (incrementorData.Operation == ArithmeticOperation.Addition &&
                        condition == Condition.Greater)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule,
                            forNode.Incrementors.First().GetLocation(),
                            new[] { forNode.Condition.GetLocation() },
                            incrementorData.IdentifierName, "inc"));
                    }
                    else if (incrementorData.Operation == ArithmeticOperation.Substraction &&
                             condition == Condition.Less)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule,
                            forNode.Incrementors.First().GetLocation(),
                            new[] { forNode.Condition.GetLocation() },
                            incrementorData.IdentifierName, "dec"));
                    }
                },
                SyntaxKind.ForStatement);
        }

        private IncrementData GetIncrementData(ExpressionSyntax incrementor)
        {
            var identifierName = string.Empty;
            var opp = ArithmeticOperation.None;

            var incrementorKind = incrementor.Kind();

            switch (incrementorKind)
            {
                case SyntaxKind.PreDecrementExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                case SyntaxKind.PostIncrementExpression:
                    if (GetIdentifierFromUnaryExpression(incrementor) is IdentifierNameSyntax operand)
                    {
                        identifierName = operand.Identifier.ValueText;
                        opp =  incrementorToOperation.GetValueOrDefault(incrementorKind, ArithmeticOperation.None);
                    }
                    break;

                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                    if (incrementor is AssignmentExpressionSyntax add &&
                        add.Left is IdentifierNameSyntax leftId &&
                        add.Right.IsKind(SyntaxKind.NumericLiteralExpression))
                    {
                        identifierName = leftId.Identifier.ValueText;
                        opp = incrementorToOperation.GetValueOrDefault(incrementorKind, ArithmeticOperation.None);
                    }
                    break;

                case SyntaxKind.SimpleAssignmentExpression:
                    if (incrementor is AssignmentExpressionSyntax simpleAssignment &&
                        simpleAssignment.Left is IdentifierNameSyntax simpleAssignmentId &&
                        simpleAssignment.Right is BinaryExpressionSyntax right &&
                        IsVariableAndLiteralBinaryExpression(right, simpleAssignmentId.Identifier.ValueText))
                    {
                        identifierName = simpleAssignmentId.Identifier.ValueText;
                        opp = incrementorToOperation.GetValueOrDefault(right.Kind(), ArithmeticOperation.None);
                    }
                    break;

                default:
                    break;
            }

            return new IncrementData(identifierName, opp);
        }

        private Condition GetCondition(BinaryExpressionSyntax conditionSyntax, string identifierName)
        {
            // Since the incremented variable can be on any side of the condition (i < 10 or 10 > i),
            // both sides of the binary expression need to be considered.

            if (IsIdentifier(conditionSyntax.Left, identifierName))
            {
                if (conditionSyntax.IsAnyKind(SyntaxKind.LessThanExpression, SyntaxKind.LessThanOrEqualExpression))
                {
                    return Condition.Less;
                }
                else if (conditionSyntax.IsAnyKind(SyntaxKind.GreaterThanExpression, SyntaxKind.GreaterThanOrEqualExpression))
                {
                    return Condition.Greater;
                }
            }
            else if (IsIdentifier(conditionSyntax.Right, identifierName))
            {
                if (conditionSyntax.IsAnyKind(SyntaxKind.LessThanExpression, SyntaxKind.LessThanOrEqualExpression))
                {
                    return Condition.Greater;
                }
                else if (conditionSyntax.IsAnyKind(SyntaxKind.GreaterThanExpression, SyntaxKind.GreaterThanOrEqualExpression))
                {
                    return Condition.Less;
                }
            }

            return Condition.None;
        }

        private bool IsVariableAndLiteralBinaryExpression(BinaryExpressionSyntax binaryExpression, string identifierName) =>
            (IsIdentifier(binaryExpression.Left, identifierName) && binaryExpression.Right.IsKind(SyntaxKind.NumericLiteralExpression)) ||
            (binaryExpression.Left.IsKind(SyntaxKind.NumericLiteralExpression) && IsIdentifier(binaryExpression.Right, identifierName));

        private bool IsIdentifier(SyntaxNode node, string identifierName) =>
            node is IdentifierNameSyntax identifier &&
            identifier.NameIs(identifierName);

        private IdentifierNameSyntax GetIdentifierFromUnaryExpression(ExpressionSyntax syntax)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PreDecrementExpression:
                    var prefix = (PrefixUnaryExpressionSyntax)syntax;
                    return (IdentifierNameSyntax)prefix.Operand;

                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                    var postfix = (PostfixUnaryExpressionSyntax)syntax;
                    return (IdentifierNameSyntax)postfix.Operand;

                default:
                    break;
            }

            return default(IdentifierNameSyntax);
        }

        private class IncrementData
        {
            public string IdentifierName { get; }

            public ArithmeticOperation Operation { get; }

            public IncrementData(string identifierName, ArithmeticOperation operation)
            {
                IdentifierName = identifierName;
                Operation = operation;
            }
        }
    }
}
