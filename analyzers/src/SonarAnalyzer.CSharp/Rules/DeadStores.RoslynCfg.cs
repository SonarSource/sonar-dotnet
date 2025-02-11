/*
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

using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.CSharp.Rules
{
    public partial class DeadStores : SonarDiagnosticAnalyzer
    {
        private class RoslynChecker : CheckerBase<ControlFlowGraph, BasicBlock>
        {
            private readonly RoslynLiveVariableAnalysis lva;

            public RoslynChecker(SonarSyntaxNodeReportingContext context, RoslynLiveVariableAnalysis lva) : base(context, lva) =>
                this.lva = lva;

            protected override State CreateState(BasicBlock block) =>
                new RoslynState(this, block);

            private class RoslynState : State
            {
                private readonly RoslynChecker owner;

                public RoslynState(RoslynChecker owner, BasicBlock block) : base(owner, block) =>
                    this.owner = owner;

                public override void AnalyzeBlock()
                {
                    foreach (var operation in block.OperationsAndBranchValue.ToReversedExecutionOrder().Select(x => x.Instance))
                    {
                        switch (operation.Kind)
                        {
                            case OperationKindEx.LocalReference:
                                ProcessParameterOrLocalReference(ILocalReferenceOperationWrapper.FromOperation(operation));
                                break;
                            case OperationKindEx.ParameterReference:
                                ProcessParameterOrLocalReference(IParameterReferenceOperationWrapper.FromOperation(operation));
                                break;
                            case OperationKindEx.SimpleAssignment:
                                ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper.FromOperation(operation));
                                break;
                            case OperationKindEx.CompoundAssignment:
                                ProcessCompoundAssignment(ICompoundAssignmentOperationWrapper.FromOperation(operation));
                                break;
                            case OperationKindEx.DeconstructionAssignment:
                                ProcessDeconstructionAssignment(IDeconstructionAssignmentOperationWrapper.FromOperation(operation));
                                break;
                            case OperationKindEx.Increment:
                            case OperationKindEx.Decrement:
                                ProcessIncrementOrDecrement(IIncrementOrDecrementOperationWrapper.FromOperation(operation));
                                break;
                        }
                    }
                }

                private void ProcessParameterOrLocalReference(IOperationWrapper reference)
                {
                    var symbols = owner.lva.ParameterOrLocalSymbols(reference.WrappedOperation).Where(x => IsSymbolRelevant(x));
                    if (reference.IsOutArgument())
                    {
                        liveOut.ExceptWith(symbols);
                    }
                    else if (!reference.IsAssignmentTarget() && !reference.IsCompoundAssignmentTarget())
                    {
                        liveOut.UnionWith(symbols);
                    }
                }

                private void ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper assignment)
                {
                    var targets = ProcessAssignment(assignment, assignment.Target, assignment.Value);
                    liveOut.ExceptWith(targets);
                }

                private void ProcessCompoundAssignment(ICompoundAssignmentOperationWrapper assignment) =>
                    ProcessAssignment(assignment, assignment.Target);

                private void ProcessIncrementOrDecrement(IIncrementOrDecrementOperationWrapper incrementOrDecrement) =>
                    ProcessAssignment(incrementOrDecrement, incrementOrDecrement.Target);

                private void ProcessDeconstructionAssignment(IDeconstructionAssignmentOperationWrapper deconstructionAssignment)
                {
                    if (ITupleOperationWrapper.IsInstance(deconstructionAssignment.Target))
                    {
                        foreach (var tupleElement in ITupleOperationWrapper.FromOperation(deconstructionAssignment.Target).AllElements())
                        {
                            var targets = ProcessAssignment(deconstructionAssignment, tupleElement);
                            liveOut.ExceptWith(targets);
                        }
                    }
                }

                private ISymbol[] ProcessAssignment(IOperationWrapper operation, IOperation target, IOperation value = null)
                {
                    var targets = owner.lva.ParameterOrLocalSymbols(target).Where(IsSymbolRelevant).ToArray();
                    foreach (var localTarget in targets)
                    {
                        if (TargetRefKind(localTarget) == RefKind.None
                            && !IsConst(localTarget)
                            && !liveOut.Contains(localTarget)
                            && !IsAllowedInitialization(localTarget)
                            && !ICaughtExceptionOperationWrapper.IsInstance(value)
                            && !target.Syntax.Parent.IsKind(SyntaxKind.ForEachStatement)
                            && !IsMuted(target.Syntax, localTarget))
                        {
                            ReportIssue(operation.WrappedOperation.Syntax.GetLocation(), localTarget);
                        }
                    }
                    return targets;

                    static bool IsConst(ISymbol localTarget) =>
                        localTarget is ILocalSymbol local && local.IsConst;

                    static RefKind TargetRefKind(ISymbol localTarget) =>
                        // Only ILocalSymbol and IParameterSymbol can be returned by ParameterOrLocalSymbol
                        localTarget is ILocalSymbol local ? local.RefKind() : ((IParameterSymbol)localTarget).RefKind;

                    bool IsAllowedInitialization(ISymbol localTarget) =>
                        operation.WrappedOperation.Syntax is VariableDeclaratorSyntax variableDeclarator
                        && variableDeclarator.Initializer != null
                        // Avoid collision with S1481: Unused is allowed. Used only in local function is also unused in current CFG.
                        && (IsAllowedInitializationValue(variableDeclarator.Initializer.Value, value == null ? default : value.ConstantValue) || IsUnusedInCurrentCfg(localTarget, target));
                }

                private bool IsUnusedInCurrentCfg(ISymbol symbol, IOperation exceptTarget)
                {
                    return !owner.lva.Cfg.Blocks.SelectMany(x => x.OperationsAndBranchValue).ToExecutionOrder().Any(IsUsed);

                    bool IsUsed(IOperationWrapperSonar wrapper) =>
                        wrapper.Instance != exceptTarget
                        && wrapper.Instance.Kind == OperationKindEx.LocalReference
                        && ILocalReferenceOperationWrapper.FromOperation(wrapper.Instance).Local.Equals(symbol);
                }
            }
        }
    }
}
