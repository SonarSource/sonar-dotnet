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

using System;
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
                    var walker = new CommonRecursionSearcher(new RecursionContext<ControlFlowGraph>(cfg, propertySymbol, property.Identifier.GetLocation(), c, "property's recursion"));
                    walker.CheckPaths();
                    // cannot meet goto s here, check for unreachable exit node is not needed here
                }
                else if (property.AccessorList != null)
                {
                    foreach (var accessor in property.AccessorList.Accessors.Where(a => a.HasBodyOrExpressionBody()))
                    {
                        var cfg = ControlFlowGraph.Create(accessor, c.SemanticModel);
                        var context = new RecursionContext<ControlFlowGraph>(cfg, propertySymbol, accessor.Keyword.GetLocation(), c, "property accessor's recursion");
                        var walker = new CommonRecursionSearcher(context, !accessor.Keyword.IsKind(SyntaxKind.SetKeyword));
                        if (!walker.CheckPaths() && CfgHasUnescapableLoop(cfg))
                        {
                            context.ReportIssue(accessor.Keyword.GetLocation());
                        }
                    }
                }
            }

            public void CheckForNoExitMethod(SyntaxNodeAnalysisContext c, CSharpSyntaxNode body, SyntaxToken identifier, ISymbol symbol)
            {
                var operation = c.SemanticModel.GetOperation(body.Parent);
                var cfg = ControlFlowGraph.Create(operation.RootOperation().Syntax, c.SemanticModel);
                var enclosingKinds = new HashSet<SyntaxKind>
                {
                    SyntaxKindEx.LocalFunctionStatement, SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression, SyntaxKind.ParenthesizedLambdaExpression
                };
                if (c.Node.IsKind(SyntaxKindEx.LocalFunctionStatement))
                {
                    // we need to go up and track all possible enclosing local function statements
                    foreach (var enclosingFunction in c.Node.Ancestors().Where(x => enclosingKinds.Contains(x.Kind())).Reverse())
                    {
                        if (enclosingFunction.IsKind(SyntaxKindEx.LocalFunctionStatement))
                        {
                            cfg = cfg.GetLocalFunctionControlFlowGraph(c.SemanticModel.GetDeclaredSymbol(enclosingFunction) as IMethodSymbol);
                        }
                        else
                        {
                            var operationWrapper = cfg.FlowAnonymousFunctionOperations().Single(x => x.WrappedOperation.Syntax == enclosingFunction);
                            cfg = cfg.GetAnonymousFunctionControlFlowGraph(operationWrapper);
                        }
                        if (cfg == null)
                        {
                            return;
                        }
                    }

                    cfg = cfg.GetLocalFunctionControlFlowGraph(symbol as IMethodSymbol);
                    if (cfg == null)
                    {
                        return;
                    }
                }

                var walker = new CommonRecursionSearcher(new RecursionContext<ControlFlowGraph>(cfg, symbol, identifier.GetLocation(), c, "method's recursion"));
                if (!walker.CheckPaths() && CfgHasUnescapableLoop(cfg))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, identifier.GetLocation(), "method's recursion"));
                }
            }

            private static bool CfgHasUnescapableLoop(ControlFlowGraph cfg) =>
                !cfg.ExitBlock.IsReachable && !cfg.Blocks.Any(x => x.FallThroughSuccessor?.Semantics == ControlFlowBranchSemantics.Throw && x.IsReachable);

            private class CommonRecursionSearcher : CfgAllPathValidator
            {
                private readonly ISymbol memberToCheck;
                private readonly Action reportIssue;
                private readonly ControlFlowGraph cfg;
                private readonly bool isGetAccesor;

                public CommonRecursionSearcher(RecursionContext<ControlFlowGraph> context, bool isGet = true)
                    : base(context.ControlFlowGraph)
                {
                    memberToCheck = context.AnalyzedSymbol;
                    reportIssue = context.ReportIssue;
                    cfg = context.ControlFlowGraph;
                    isGetAccesor = isGet;
                }

                public bool CheckPaths()
                {
                    if (CheckAllPaths())
                    {
                        reportIssue();
                        return true;
                    }

                    return false;
                }

                protected override bool IsValid(BasicBlock block) =>
                    FindReferenceToSelf(block);

                protected override bool IsInvalid(BasicBlock block) => false;

                private bool FindReferenceToSelf(BasicBlock block)
                {
                    foreach (var operation in block.OperationsAndBranchValue)
                    {
                        if (ProcessOperation(operation, cfg, out var result))
                        {
                            return result;
                        }
                    }
                    return false;

                    bool ProcessOperation(IOperation operation, ControlFlowGraph controlFlowGraph, out bool result)
                    {
                        foreach (var child in operation.DescendantsAndSelf().ToReversedExecutionOrder())
                        {
                            if (child.Instance.Kind == OperationKindEx.FlowAnonymousFunction)
                            {
                                var anonymousFunctionCfg = controlFlowGraph.GetAnonymousFunctionControlFlowGraph(IFlowAnonymousFunctionOperationWrapper.FromOperation(child.Instance));
                                foreach (var subOperation in anonymousFunctionCfg.Blocks.SelectMany(x => x.OperationsAndBranchValue).SelectMany(x => x.DescendantsAndSelf()))
                                {
                                    if (ProcessOperation(subOperation, anonymousFunctionCfg, out result))
                                    {
                                        return true;
                                    }
                                }
                            }
                            else if (memberToCheck.Equals(MemberSymbol(child.Instance)))
                            {
                                var isWrite = child.Parent is { Kind: OperationKindEx.SimpleAssignment } parent && ISimpleAssignmentOperationWrapper.FromOperation(parent).Target == child.Instance;
                                result = isGetAccesor ^ isWrite;
                                return true;
                            }
                        }

                        result = false;
                        return false;
                    }

                    static ISymbol MemberSymbol(IOperation operation) =>
                        operation.Kind switch
                        {
                            OperationKindEx.PropertyReference => IPropertyReferenceOperationWrapper.FromOperation(operation).Property,
                            OperationKindEx.Invocation => IInvocationOperationWrapper.FromOperation(operation).TargetMethod,
                            _ => null
                        };
                }
            }
        }
    }
}
