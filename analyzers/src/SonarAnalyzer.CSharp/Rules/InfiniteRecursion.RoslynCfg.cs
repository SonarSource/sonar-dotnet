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
using SonarAnalyzer.CFG.Helpers;
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
                    var walker = new RecursionSearcherForProperty(
                        new RecursionContext<ControlFlowGraph>(cfg, propertySymbol, property.Identifier.GetLocation(), c, "property's recursion"),
                        isSetAccessor: false);
                    walker.CheckPaths();
                    // cannot meet goto s here, check for unreachable exit node is not needed here
                }
                else if (property.AccessorList != null)
                {
                    foreach (var accessor in property.AccessorList.Accessors.Where(a => a.HasBodyOrExpressionBody()))
                    {
                        var cfg = ControlFlowGraph.Create(accessor, c.SemanticModel);
                        var context = new RecursionContext<ControlFlowGraph>(cfg, propertySymbol, accessor.Keyword.GetLocation(), c, "property accessor's recursion");
                        var walker = new RecursionSearcherForProperty(context, isSetAccessor: accessor.Keyword.IsKind(SyntaxKind.SetKeyword));
                        if (!walker.CheckPaths() && !cfg.ExitBlock.IsReachable)
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
                    }

                    cfg = cfg.GetLocalFunctionControlFlowGraph(symbol as IMethodSymbol);
                }

                var walker = new RecursionSearcherForMethod(new RecursionContext<ControlFlowGraph>(cfg, symbol, identifier.GetLocation(), c, "method's recursion"));
                if (!walker.CheckPaths() && !cfg.ExitBlock.IsReachable)
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, identifier.GetLocation(), "method's recursion"));
                }
            }

            private class RecursionSearcherForMethod : RecursionSearcher
            {
                public RecursionSearcherForMethod(RecursionContext<ControlFlowGraph> context)
                    : base(context)
                {
                    var finder = new BooleanBlockFinder(x =>
                        x.Instance.Syntax is InvocationExpressionSyntax invocation
                        && IsInstructionOnThisAndMatchesDeclaringSymbol(invocation.Expression, declaringSymbol, semanticModel));
                    referenceToDeclaringSymbolFinder = block => finder.Matches(block);
                }
            }

            private class RecursionSearcherForProperty : RecursionSearcher
            {
                private readonly bool isSet;

                private static readonly ISet<Type> TypesForReference = new HashSet<Type> { typeof(IdentifierNameSyntax), typeof(MemberAccessExpressionSyntax) };

                public RecursionSearcherForProperty(RecursionContext<ControlFlowGraph> context, bool isSetAccessor)
                    : base(context)
                {
                    isSet = isSetAccessor;
                    var finder = new BooleanBlockFinder(x =>
                        x.Instance.Syntax is var syntax
                        && TypesForReference.Contains(syntax.GetType())
                        && MatchesAccessor(syntax)
                        && IsInstructionOnThisAndMatchesDeclaringSymbol(syntax, declaringSymbol, semanticModel));
                    referenceToDeclaringSymbolFinder = block => finder.Matches(block);
                }

                private bool MatchesAccessor(SyntaxNode node)
                {
                    var propertyAccess = ((ExpressionSyntax)node).GetSelfOrTopParenthesizedExpression();
                    var isNodeASet = propertyAccess.Parent is AssignmentExpressionSyntax assignment && assignment.Left == propertyAccess;
                    return isNodeASet == isSet;
                }
            }

            private class RecursionSearcher : CfgAllPathValidator
            {
                protected readonly SemanticModel semanticModel;
                protected readonly ISymbol declaringSymbol;
                protected Func<BasicBlock, bool> referenceToDeclaringSymbolFinder;
                private readonly Action reportIssue;

                protected RecursionSearcher(RecursionContext<ControlFlowGraph> context)
                    : base(context.ControlFlowGraph)
                {
                    declaringSymbol = context.AnalyzedSymbol;
                    semanticModel = context.SemanticModel;
                    reportIssue = context.ReportIssue;
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
                    referenceToDeclaringSymbolFinder(block);

                protected override bool IsInvalid(BasicBlock block) => false;
            }

            private class BooleanBlockFinder : OperationFinder<bool>
            {
                private readonly Func<IOperationWrapperSonar, bool> matcher;

                public BooleanBlockFinder(Func<IOperationWrapperSonar, bool> matcher) =>
                    this.matcher = matcher;

                public bool Matches(BasicBlock block) =>
                    TryFind(block, out _);

                protected override bool TryFindOperation(IOperationWrapperSonar operation, out bool result)
                {
                    result = matcher(operation);
                    return result;
                }
            }
        }
    }
}
