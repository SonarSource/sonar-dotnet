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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;
using CfgAllPathValidator = SonarAnalyzer.CFG.Roslyn.CfgAllPathValidator;

namespace SonarAnalyzer.Rules.CSharp
{
    public partial class InfiniteRecursion
    {
        private class RoslynChecker : IChecker
        {
            public void CheckForNoExitProperty(SyntaxNodeAnalysisContext c, PropertyDeclarationSyntax property, IPropertySymbol propertySymbol)
            {
                if (property.ExpressionBody?.Expression != null)
                {
                    var cfg = ControlFlowGraph.Create(property.ExpressionBody, c.SemanticModel);
                    var walker = new RecursionSearcher(new RecursionContext<ControlFlowGraph>(cfg, propertySymbol, property.Identifier.GetLocation(), c, "property's recursion"));
                    walker.CheckPaths();
                }
                else if (property.AccessorList != null)
                {
                    foreach (var accessor in property.AccessorList.Accessors.Where(a => a.HasBodyOrExpressionBody()))
                    {
                        var cfg = ControlFlowGraph.Create(accessor, c.SemanticModel);
                        var context = new RecursionContext<ControlFlowGraph>(cfg, propertySymbol, accessor.Keyword.GetLocation(), c, "property accessor's recursion");
                        var walker = new RecursionSearcher(context, !accessor.Keyword.IsKind(SyntaxKind.SetKeyword));
                        walker.CheckPaths();
                    }
                }
            }

            public void CheckForNoExitMethod(SyntaxNodeAnalysisContext c, CSharpSyntaxNode body, SyntaxToken identifier, IMethodSymbol symbol)
            {
                var cfg = body.CreateCfg(c.SemanticModel);
                var context = new RecursionContext<ControlFlowGraph>(cfg, symbol, identifier.GetLocation(), c, "method's recursion");
                var walker = new RecursionSearcher(context);
                walker.CheckPaths();
            }

            private class RecursionSearcher : CfgAllPathValidator
            {
                private readonly RecursionContext<ControlFlowGraph> context;
                private readonly bool isGetAccesor;

                public RecursionSearcher(RecursionContext<ControlFlowGraph> context, bool isGetAccesor = true)
                    : base(context.ControlFlowGraph)
                {
                    this.context = context;
                    this.isGetAccesor = isGetAccesor;
                }

                public void CheckPaths()
                {
                    if (!CfgCanExit() || CheckAllPaths())
                    {
                        context.ReportIssue();
                    }
                }

                protected override bool IsValid(BasicBlock block)
                {
                    foreach (var operation in block.OperationsAndBranchValue.ToReversedExecutionOrder())
                    {
                        if (context.AnalyzedSymbol.Equals(MemberSymbol(operation.Instance)))
                        {
                            var isWrite = operation.Parent is { Kind: OperationKindEx.SimpleAssignment } parent
                                          && ISimpleAssignmentOperationWrapper.FromOperation(parent).Target == operation.Instance;
                            return isGetAccesor ^ isWrite;
                        }
                    }
                    return false;

                    static ISymbol MemberSymbol(IOperation operation) =>
                        operation.Kind switch
                        {
                            OperationKindEx.PropertyReference => IPropertyReferenceOperationWrapper.FromOperation(operation).Property,
                            OperationKindEx.Invocation => IInvocationOperationWrapper.FromOperation(operation).TargetMethod,
                            _ => null
                        };
                }

                protected override bool IsInvalid(BasicBlock block) => false;

                private bool CfgCanExit() =>
                    context.ControlFlowGraph.ExitBlock.IsReachable
                    || context.ControlFlowGraph.Blocks.Any(x => x.FallThroughSuccessor?.Semantics == ControlFlowBranchSemantics.Throw && x.IsReachable);
            }
        }
    }
}
