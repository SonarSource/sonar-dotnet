﻿/*
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

//FIXME: Cleanup
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    public partial class DeadStores : SonarDiagnosticAnalyzer
    {
        private class RoslynChecker : CheckerBase<ControlFlowGraph, BasicBlock>
        {
            private readonly RoslynLiveVariableAnalysis lva;

            public RoslynChecker(SyntaxNodeAnalysisContext context, RoslynLiveVariableAnalysis lva) : base(context, lva) =>
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
                    foreach (var operation in block.OperationsAndBranchValue.ToReversedExecutionOrder())
                    {
                        switch (operation.Instance.Kind)
                        {
                            case OperationKindEx.LocalReference:
                                ProcessParameterOrLocalReference(ILocalReferenceOperationWrapper.FromOperation(operation.Instance));
                                break;
                            case OperationKindEx.ParameterReference:
                                ProcessParameterOrLocalReference(IParameterReferenceOperationWrapper.FromOperation(operation.Instance));
                                break;
                            case OperationKindEx.SimpleAssignment:
                                ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper.FromOperation(operation.Instance));
                                break;
                            case OperationKindEx.CompoundAssignment:
                                ProcessCompoundAssignment(ICompoundAssignmentOperationWrapper.FromOperation(operation.Instance));
                                break;
                        }
                    }
                }

                private void ProcessParameterOrLocalReference(IOperationWrapper reference)
                {
                    if (owner.lva.ParameterOrLocalSymbol(reference.WrappedOperation) is { } symbol && IsSymbolRelevant(symbol))
                    {
                        if (RoslynLiveVariableAnalysis.IsOutArgument(reference.WrappedOperation))
                        {
                            liveOut.Remove(symbol);
                        }
                        else if (!reference.IsAssignmentTarget() && !reference.IsCompoundAssignmentTarget())
                        {
                            liveOut.Add(symbol);
                        }
                    }
                }

                private void ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper assignment)
                {
                    if (ProcessAssignment(assignment, assignment.Target) is { } localTarget)
                    {
                        liveOut.Remove(localTarget);
                    }
                }

                private void ProcessCompoundAssignment(ICompoundAssignmentOperationWrapper assignment) =>
                    ProcessAssignment(assignment, assignment.Target);

                private ISymbol ProcessAssignment(IOperationWrapper operation, IOperation target)
                {
                    if (owner.lva.ParameterOrLocalSymbol(target) is { } localTarget && IsSymbolRelevant(localTarget))
                    {
                        if (TargetRefKind() == RefKind.None
                            && !IsConst()
                            && !liveOut.Contains(localTarget)
                            && !IsAllowedInitialization()
                            && !IsMuted(target.Syntax))   // FIXME: Unmute?
                        {
                            ReportIssue(operation.WrappedOperation.Syntax.GetLocation(), localTarget);  // FIXME: Better overload?
                        }
                        return localTarget;
                    }
                    else
                    {
                        return null;
                    }

                    bool IsConst() =>
                        localTarget is ILocalSymbol local && local.IsConst;

                    RefKind TargetRefKind() =>
                        localTarget switch
                        {
                            ILocalSymbol local => local.RefKind(),
                            IParameterSymbol param => param.RefKind,
                            _ => default
                        };

                    bool IsAllowedInitialization() =>
                        operation.WrappedOperation.Syntax is VariableDeclaratorSyntax variableDeclarator
                        && variableDeclarator.Initializer != null
                        && IsAllowedInitializationValue(variableDeclarator.Initializer.Value);
                }

                // FIXME: Temporary duplicate
                private bool IsMuted(SyntaxNode node) =>
                    new MutedSyntaxWalker(SemanticModel, node).IsMuted();

            }
        }
    }
}
