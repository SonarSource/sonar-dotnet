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
        private class InfiniteRecursion_RoslynCfg : IInfiniteRecursion
        {
            public void CheckForNoExitProperty(SyntaxNodeAnalysisContext c, PropertyDeclarationSyntax property, IPropertySymbol propertySymbol)
            {
                if (property.ExpressionBody?.Expression != null)
                {
                    var cfg = ControlFlowGraph.Create(property.ExpressionBody, c.SemanticModel);
                    var walker = new CfgWalkerForProperty(
                        new RecursionAnalysisContext<ControlFlowGraph>(cfg, propertySymbol, property.Identifier.GetLocation(), c),
                        "property's recursion",
                        isSetAccessor: false);
                    walker.CheckPaths();
                    return;
                }

                var accessors = property.AccessorList?.Accessors.Where(a => a.HasBodyOrExpressionBody());
                if (accessors != null)
                {
                    foreach (var accessor in accessors)
                    {
                        var cfg = ControlFlowGraph.Create(accessor, c.SemanticModel);
                        var walker = new CfgWalkerForProperty(
                            new RecursionAnalysisContext<ControlFlowGraph>(cfg, propertySymbol, accessor.Keyword.GetLocation(), c),
                            "property accessor's recursion",
                            isSetAccessor: accessor.Keyword.IsKind(SyntaxKind.SetKeyword));
                        walker.CheckPaths();
                    }
                }
            }

            public void CheckForNoExitMethod(SyntaxNodeAnalysisContext c, CSharpSyntaxNode body, SyntaxToken identifier, ISymbol symbol)
            {
                var operation = c.SemanticModel.GetOperation(body.Parent);
                var wrapper = new IOperationWrapperSonar(operation);
                while (wrapper.Parent != null)
                {
                    wrapper = new IOperationWrapperSonar(wrapper.Parent);
                }

                var cfg = ControlFlowGraph.Create(wrapper.Instance.Syntax, c.SemanticModel);
                var enclosingKinds = new HashSet<SyntaxKind>
                {
                    SyntaxKindEx.LocalFunctionStatement, SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression, SyntaxKind.ParenthesizedLambdaExpression
                };
                if (c.Node.IsKind(SyntaxKindEx.LocalFunctionStatement))
                {
                    // we need to go up and track all possible enclosing local function statements
                    foreach (var enclosingFunction in c.Node.Ancestors().Where(x => enclosingKinds.Contains(x.Kind())).Reverse())
                    {
                        if (enclosingFunction.IsAnyKind(SyntaxKindEx.LocalFunctionStatement))
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

                var walker = new CfgWalkerForMethod(new RecursionAnalysisContext<ControlFlowGraph>(cfg, symbol, identifier.GetLocation(), c));
                walker.CheckPaths();
            }

            private class CfgWalkerForMethod : CfgRecursionSearcher
            {
                private readonly BooleanBlockMatcher existReferenceToDeclaringSymbolMatcher;

                public CfgWalkerForMethod(RecursionAnalysisContext<ControlFlowGraph> context)
                    : base(
                        context.ControlFlowGraph,
                        context.AnalyzedSymbol,
                        context.SemanticModel,
                        () => context.AnalysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.IssueLocation, "method's recursion")))
                {
                    existReferenceToDeclaringSymbolMatcher = new BooleanBlockMatcher(operation =>
                    {
                        if (!(operation.Instance.Syntax is InvocationExpressionSyntax invocation))
                        {
                            return false;
                        }

                        return IsInstructionOnThisAndMatchesDeclaringSymbol(invocation.Expression, declaringSymbol, semanticModel);
                    });
                }

                protected override bool BlockHasReferenceToDeclaringSymbol(BasicBlock block) =>
                    existReferenceToDeclaringSymbolMatcher.Matches(block);
            }

            private class CfgWalkerForProperty : CfgRecursionSearcher
            {
                private readonly bool isSet;
                private readonly BooleanBlockMatcher existReferenceToDeclaringSymbolMatcher;

                public CfgWalkerForProperty(RecursionAnalysisContext<ControlFlowGraph> context, string reportOn, bool isSetAccessor)
                    : base(
                        context.ControlFlowGraph,
                        context.AnalyzedSymbol,
                        context.SemanticModel,
                        () => context.AnalysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.IssueLocation, reportOn)))
                {
                    isSet = isSetAccessor;
                    existReferenceToDeclaringSymbolMatcher = new BooleanBlockMatcher(operation =>
                        operation.Instance.Syntax is var syntax
                        && TypesForReference.Contains(syntax.GetType())
                        && MatchesAccessor(syntax)
                        && IsInstructionOnThisAndMatchesDeclaringSymbol(syntax, declaringSymbol, semanticModel));
                }

                private static readonly ISet<Type> TypesForReference = new HashSet<Type> { typeof(IdentifierNameSyntax), typeof(MemberAccessExpressionSyntax) };

                protected override bool BlockHasReferenceToDeclaringSymbol(BasicBlock block) =>
                    existReferenceToDeclaringSymbolMatcher.Matches(block);

                private bool MatchesAccessor(SyntaxNode node)
                {
                    var expr = (ExpressionSyntax)node;
                    var propertyAccess = expr.GetSelfOrTopParenthesizedExpression();
                    var isNodeASet = propertyAccess.Parent is AssignmentExpressionSyntax assignment && assignment.Left == propertyAccess;
                    return isNodeASet == isSet;
                }
            }

            private abstract class CfgRecursionSearcher : CfgAllPathValidator
            {
                protected readonly SemanticModel semanticModel;
                protected readonly ISymbol declaringSymbol;
                private readonly Action reportIssue;

                protected CfgRecursionSearcher(ControlFlowGraph cfg, ISymbol declaringSymbol, SemanticModel semanticModel, Action reportIssue)
                    : base(cfg)
                {
                    this.declaringSymbol = declaringSymbol;
                    this.semanticModel = semanticModel;
                    this.reportIssue = reportIssue;
                }

                public void CheckPaths()
                {
                    if (CheckAllPaths())
                    {
                        reportIssue();
                    }
                }

                protected override bool IsValid(BasicBlock block) =>
                    BlockHasReferenceToDeclaringSymbol(block);

                protected override bool IsInvalid(BasicBlock block) => false;

                protected abstract bool BlockHasReferenceToDeclaringSymbol(BasicBlock block);
            }

            private class BooleanBlockMatcher : OperationFinder<bool>
            {
                private readonly Func<IOperationWrapperSonar, bool> matcher;

                public BooleanBlockMatcher(Func<IOperationWrapperSonar, bool> matcher) =>
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
