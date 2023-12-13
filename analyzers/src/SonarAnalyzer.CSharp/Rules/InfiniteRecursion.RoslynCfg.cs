/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.CFG.Roslyn;
using CfgAllPathValidator = SonarAnalyzer.CFG.Roslyn.CfgAllPathValidator;

namespace SonarAnalyzer.Rules.CSharp
{
    public partial class InfiniteRecursion
    {
        private sealed class RoslynChecker : IChecker
        {
            public void CheckForNoExitProperty(SonarSyntaxNodeReportingContext c, PropertyDeclarationSyntax property, IPropertySymbol propertySymbol) =>
                CheckForNoExit(c,
                    propertySymbol,
                    property.ExpressionBody,
                    property.AccessorList,
                    property.Identifier.GetLocation(),
                    "property's recursion",
                    "property accessor's recursion");

            public void CheckForNoExitIndexer(SonarSyntaxNodeReportingContext c, IndexerDeclarationSyntax indexer, IPropertySymbol propertySymbol) =>
                CheckForNoExit(c,
                    propertySymbol,
                    indexer.ExpressionBody,
                    indexer.AccessorList,
                    indexer.ThisKeyword.GetLocation(),
                    "indexer's recursion",
                    "indexer accessor's recursion");

            public void CheckForNoExitEvent(SonarSyntaxNodeReportingContext c, EventDeclarationSyntax eventDeclaration, IEventSymbol eventSymbol)
            {
                if (eventDeclaration.AccessorList != null)
                {
                    foreach (var accessor in eventDeclaration.AccessorList.Accessors.Where(a => a.HasBodyOrExpressionBody()))
                    {
                        var cfg = ControlFlowGraph.Create(accessor, c.SemanticModel, c.Cancel);
                        var context = new RecursionContext<ControlFlowGraph>(c, cfg, eventSymbol, accessor.Keyword.GetLocation(), "event accessor's recursion");
                        var walker = new RecursionSearcher(context);
                        walker.CheckPaths();
                    }
                }
            }

            public void CheckForNoExitMethod(SonarSyntaxNodeReportingContext c, SyntaxNode body, SyntaxToken identifier, IMethodSymbol symbol)
            {
                if (body.CreateCfg(c.SemanticModel, c.Cancel) is { } cfg)
                {
                    var context = new RecursionContext<ControlFlowGraph>(c, cfg, symbol, identifier.GetLocation(), "method's recursion");
                    var walker = new RecursionSearcher(context);
                    walker.CheckPaths();
                }
            }

            private static void CheckForNoExit(SonarSyntaxNodeReportingContext c,
                                       IPropertySymbol propertySymbol,
                                       ArrowExpressionClauseSyntax expressionBody,
                                       AccessorListSyntax accessorList,
                                       Location location,
                                       string arrowExpressionMessageArg,
                                       string accessorMessageArg)
            {
                if (expressionBody?.Expression != null)
                {
                    var cfg = ControlFlowGraph.Create(expressionBody, c.SemanticModel, c.Cancel);
                    var walker = new RecursionSearcher(new RecursionContext<ControlFlowGraph>(c, cfg, propertySymbol, location, arrowExpressionMessageArg));
                    walker.CheckPaths();
                }
                else if (accessorList != null)
                {
                    foreach (var accessor in accessorList.Accessors.Where(a => a.HasBodyOrExpressionBody()))
                    {
                        var cfg = ControlFlowGraph.Create(accessor, c.SemanticModel, c.Cancel);
                        var context = new RecursionContext<ControlFlowGraph>(c, cfg, propertySymbol, accessor.Keyword.GetLocation(), accessorMessageArg);
                        var walker = new RecursionSearcher(context, !accessor.Keyword.IsAnyKind(SyntaxKind.SetKeyword, SyntaxKindEx.InitKeyword));
                        walker.CheckPaths();
                    }
                }
            }

            private sealed class RecursionSearcher : CfgAllPathValidator
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
                    if (block.OperationsAndBranchValue.ToReversedExecutionOrder().FirstOrDefault(x => context.AnalyzedSymbol.Equals(MemberSymbol(x.Instance))) is { Instance: { } } operation)
                    {
                        var isWrite = operation.Parent is { Kind: OperationKindEx.SimpleAssignment } parent
                                      && ISimpleAssignmentOperationWrapper.FromOperation(parent).Target == operation.Instance;
                        return isGetAccesor ^ isWrite;
                    }

                    return false;

                    static ISymbol MemberSymbol(IOperation operation) =>
                        operation.Kind switch
                        {
                            OperationKindEx.PropertyReference
                                when IPropertyReferenceOperationWrapper.FromOperation(operation) is var propertyReference && InstanceReferencesThis(propertyReference.Instance) =>
                                propertyReference.Property,
                            OperationKindEx.Invocation
                                when IInvocationOperationWrapper.FromOperation(operation) is var invocation && (!invocation.IsVirtual || InstanceReferencesThis(invocation.Instance)) =>
                                invocation.TargetMethod,
                            OperationKindEx.Binary =>
                                IBinaryOperationWrapper.FromOperation(operation).OperatorMethod,
                            OperationKindEx.Decrement =>
                                IIncrementOrDecrementOperationWrapper.FromOperation(operation).OperatorMethod,
                            OperationKindEx.Increment =>
                                IIncrementOrDecrementOperationWrapper.FromOperation(operation).OperatorMethod,
                            OperationKindEx.Unary =>
                                IUnaryOperationWrapper.FromOperation(operation).OperatorMethod,
                            OperationKindEx.Conversion=>
                                IConversionOperationWrapper.FromOperation(operation).OperatorMethod,
                            OperationKindEx.EventReference =>
                                IEventReferenceOperationWrapper.FromOperation(operation).Member,
                            _ => null
                        };

                    static bool InstanceReferencesThis(IOperation instance) =>
                        instance == null || instance.IsAnyKind(OperationKindEx.FlowCaptureReference, OperationKindEx.InstanceReference);
                }

                protected override bool IsInvalid(BasicBlock block) => false;

                private bool CfgCanExit() =>
                    context.ControlFlowGraph.ExitBlock.IsReachable
                    || context.ControlFlowGraph.Blocks.Any(x => x.FallThroughSuccessor?.Semantics == ControlFlowBranchSemantics.Throw && x.IsReachable);
            }
        }
    }
}
