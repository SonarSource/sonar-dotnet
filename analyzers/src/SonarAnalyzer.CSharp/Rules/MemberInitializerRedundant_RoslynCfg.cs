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
using SonarAnalyzer.CFG.Helpers;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;
using SymbolWithInitializer = System.Collections.Generic.KeyValuePair<Microsoft.CodeAnalysis.ISymbol, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax>;

namespace SonarAnalyzer.Rules.CSharp
{
    public sealed partial class MemberInitializerRedundant
    {
        private class MemberInitializerRedundancyCheckerRoslyn : CfgAllPathValidator
        {
            private readonly IsValidMatcher isValidMatcher;
            private readonly IsInvalidMatcher isInvalidMatcher;

            public MemberInitializerRedundancyCheckerRoslyn(ControlFlowGraph cfg, ISymbol memberToCheck, SemanticModel semanticModel) : base(cfg)
            {
                var redundancyChecker = new RedundancyChecker(memberToCheck, semanticModel);
                this.isValidMatcher = new IsValidMatcher(redundancyChecker);
                this.isInvalidMatcher = new IsInvalidMatcher(redundancyChecker);
            }

            // Returns true if the block contains assignment before access
            protected override bool IsValid(BasicBlock block) =>
                isValidMatcher.Matches(block);

            // Returns true if the block contains access before assignment
            protected override bool IsInvalid(BasicBlock block) =>
                isInvalidMatcher.Matches(block);

            private abstract class RedundancyMatcherBase : OperationFinder<bool>
            {
                protected readonly RedundancyChecker redundancyChecker;

                protected RedundancyMatcherBase(RedundancyChecker redundancyChecker) =>
                    this.redundancyChecker = redundancyChecker;

                public bool Matches(BasicBlock block)
                {
                    TryFind(block, out var result);
                    return result;
                }
            }

            private  class IsValidMatcher : RedundancyMatcherBase
            {
                public IsValidMatcher(RedundancyChecker redundancyChecker) : base(redundancyChecker) { }

                protected override bool TryFindOperation(IOperationWrapperSonar operation, out bool result)
                {
                    var instruction = operation.Instance.Syntax;
                    switch (instruction.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                        case SyntaxKind.SimpleMemberAccessExpression:
                            {
                                var memberAccess = RedundancyChecker.GetPossibleMemberAccessParent(instruction);
                                if (memberAccess != null && redundancyChecker.TryGetReadWriteFromMemberAccess(memberAccess, out var isRead))
                                {
                                    result = !isRead;
                                    return true;
                                }
                                break;
                            }
                        case SyntaxKind.SimpleAssignmentExpression:
                            {
                                var assignment = (AssignmentExpressionSyntax)instruction;
                                if (redundancyChecker.IsMatchingMember(assignment.Left.RemoveParentheses()))
                                {
                                    result = true;
                                    return true;
                                }
                                break;
                            }
                        default:
                            // do nothing
                            break;
                    }

                    result = default;
                    return false;
                }
            }

            private  class IsInvalidMatcher : RedundancyMatcherBase
            {
                public IsInvalidMatcher(RedundancyChecker redundancyChecker) : base(redundancyChecker) { }

                protected override bool TryFindOperation(IOperationWrapperSonar operation, out bool result)
                {
                    var instruction = operation.Instance.Syntax;
                    switch (instruction.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                        case SyntaxKind.SimpleMemberAccessExpression:
                            {
                                var memberAccess = RedundancyChecker.GetPossibleMemberAccessParent(instruction);
                                if (memberAccess != null && redundancyChecker.TryGetReadWriteFromMemberAccess(memberAccess, out result))
                                {
                                    return true;
                                }
                            }
                            break;
                        case SyntaxKind.SimpleAssignmentExpression:
                            {
                                var assignment = (AssignmentExpressionSyntax)instruction;
                                if (redundancyChecker.IsMatchingMember(assignment.Left))
                                {
                                    result = false;
                                    return true;
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
                                    result = true;
                                    return true;
                                }
                            }
                            break;
                        default:
                            // do nothing
                            break;
                    }

                    result = default;
                    return false;
                }
            }
        }
    }
}
