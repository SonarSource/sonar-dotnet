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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    public sealed partial class MemberInitializerRedundant
    {
        private class MemberInitializerRedundancyCheckerSonar : CfgAllPathValidator
        {
            private readonly RedundancyChecker redundancyChecker;

            public MemberInitializerRedundancyCheckerSonar(IControlFlowGraph cfg, ISymbol memberToCheck, SemanticModel semanticModel) : base(cfg) =>
                redundancyChecker = new RedundancyChecker(memberToCheck, semanticModel);

            // Returns true if the block contains assignment before access
            protected override bool IsBlockValid(Block block)
            {
                foreach (var instruction in block.Instructions)
                {
                    switch (instruction.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                        case SyntaxKind.SimpleMemberAccessExpression:
                            {
                                if (RedundancyChecker.PossibleMemberAccessParent(instruction) is { } memberAccess
                                    && redundancyChecker.TryGetReadWriteFromMemberAccess(memberAccess, out var isRead))
                                {
                                    return !isRead;
                                }
                            }
                            break;
                        case SyntaxKind.SimpleAssignmentExpression:
                            {
                                var assignment = (AssignmentExpressionSyntax)instruction;
                                if (redundancyChecker.IsMatchingMember(assignment.Left.RemoveParentheses()))
                                {
                                    return true;
                                }
                            }
                            break;
                    }
                }

                return false;
            }

            // Returns true if the block contains access before assignment
            protected override bool IsBlockInvalid(Block block)
            {
                foreach (var instruction in block.Instructions)
                {
                    switch (instruction.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                        case SyntaxKind.SimpleMemberAccessExpression:
                            {
                                var memberAccess = RedundancyChecker.PossibleMemberAccessParent(instruction);
                                if (memberAccess != null && redundancyChecker.TryGetReadWriteFromMemberAccess(memberAccess, out var isRead))
                                {
                                    return isRead;
                                }
                            }
                            break;
                        case SyntaxKind.SimpleAssignmentExpression:
                            {
                                var assignment = (AssignmentExpressionSyntax)instruction;
                                if (redundancyChecker.IsMatchingMember(assignment.Left))
                                {
                                    return false;
                                }
                            }
                            break;

                        case SyntaxKind.AnonymousMethodExpression:
                        case SyntaxKind.ParenthesizedLambdaExpression:
                        case SyntaxKind.SimpleLambdaExpression:
                        case SyntaxKind.QueryExpression:
                            {
                                if (redundancyChecker.IsMemberUsedInsideLambda(instruction))
                                {
                                    return true;
                                }
                            }
                            break;
                    }
                }
                return false;
            }

            private class RedundancyChecker
            {
                private readonly ISymbol memberToCheck;
                private readonly SemanticModel semanticModel;

                public RedundancyChecker(ISymbol memberToCheck, SemanticModel semanticModel)
                {
                    this.memberToCheck = memberToCheck;
                    this.semanticModel = semanticModel;
                }

                public bool IsMemberUsedInsideLambda(SyntaxNode instruction) =>
                    instruction.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Select(PossibleMemberAccessParent)
                        .Any(IsMatchingMember);

                public bool IsMatchingMember(ExpressionSyntax expression)
                {
                    return ExtractIdentifier(expression) is { } identifier
                           && semanticModel.GetSymbolInfo(identifier).Symbol is { } assignedSymbol
                           && memberToCheck.Equals(assignedSymbol);

                    IdentifierNameSyntax ExtractIdentifier(ExpressionSyntax expressionSyntax)
                    {
                        if (expressionSyntax.IsKind(SyntaxKind.IdentifierName))
                        {
                            return (IdentifierNameSyntax)expressionSyntax;
                        }
                        if (expressionSyntax is MemberAccessExpressionSyntax memberAccess
                            && memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                        {
                            return memberAccess.Name as IdentifierNameSyntax;
                        }
                        if (expressionSyntax is ConditionalAccessExpressionSyntax conditionalAccess
                            && conditionalAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                        {
                            return (conditionalAccess.WhenNotNull as MemberBindingExpressionSyntax)?.Name as IdentifierNameSyntax;
                        }
                        return null;
                    }
                }

                public static ExpressionSyntax PossibleMemberAccessParent(SyntaxNode node) =>
                    node is MemberAccessExpressionSyntax memberAccess
                        ? memberAccess
                        : PossibleMemberAccessParent(node as IdentifierNameSyntax);

                private static ExpressionSyntax PossibleMemberAccessParent(IdentifierNameSyntax identifier)
                {
                    if (identifier.Parent is MemberAccessExpressionSyntax memberAccess)
                    {
                        return memberAccess;
                    }

                    if (identifier.Parent is MemberBindingExpressionSyntax memberBinding)
                    {
                        return (ExpressionSyntax)memberBinding.Parent;
                    }

                    return identifier;
                }

                public bool TryGetReadWriteFromMemberAccess(ExpressionSyntax expression, out bool isRead)
                {
                    isRead = false;

                    var parenthesized = expression.GetSelfOrTopParenthesizedExpression();

                    if (!IsMatchingMember(expression))
                    {
                        return false;
                    }

                    if (IsOutArgument(parenthesized))
                    {
                        isRead = false;
                        return true;
                    }

                    if (IsReadAccess(parenthesized, this.semanticModel))
                    {
                        isRead = true;
                        return true;
                    }

                    return false;
                }

                private static bool IsBeingAssigned(ExpressionSyntax expression) =>
                    expression.Parent is AssignmentExpressionSyntax assignment
                    && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
                    && assignment.Left == expression;

                private static bool IsOutArgument(ExpressionSyntax parenthesized) =>
                    parenthesized.Parent is ArgumentSyntax argument
                    && argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword);

                private static bool IsReadAccess(ExpressionSyntax parenthesized, SemanticModel semanticModel) =>
                    !IsBeingAssigned(parenthesized)
                    && !parenthesized.IsInNameOfArgument(semanticModel);
            }
        }
    }
}
