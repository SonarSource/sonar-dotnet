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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.Extensions;
using CfgAllPathValidator = SonarAnalyzer.CFG.Sonar.CfgAllPathValidator;
using SymbolWithInitializer = System.Collections.Generic.KeyValuePair<Microsoft.CodeAnalysis.ISymbol, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax>;

namespace SonarAnalyzer.Rules.CSharp
{
    public sealed partial class MemberInitializerRedundant
    {
        private class MemberInitializerRedundancyCheckerSonar : CfgAllPathValidator
        {
            private readonly RedundancyChecker redundancyChecker;

            public MemberInitializerRedundancyCheckerSonar(IControlFlowGraph cfg, ISymbol memberToCheck, SemanticModel semanticModel) : base(cfg) =>
                this.redundancyChecker = new RedundancyChecker(memberToCheck, semanticModel);

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
                                var memberAccess = RedundancyChecker.GetPossibleMemberAccessParent(instruction);
                                if (memberAccess != null && redundancyChecker.TryGetReadWriteFromMemberAccess(memberAccess, out var isRead))
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
                        default:
                            // do nothing
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
                                var memberAccess = RedundancyChecker.GetPossibleMemberAccessParent(instruction);
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
                        default:
                            // do nothing
                            break;
                    }
                }
                return false;
            }
        }
    }
}
