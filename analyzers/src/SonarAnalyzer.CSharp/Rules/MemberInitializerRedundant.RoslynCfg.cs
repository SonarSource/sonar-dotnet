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

namespace SonarAnalyzer.Rules.CSharp
{
    public sealed partial class MemberInitializerRedundant
    {
        private class RoslynChecker : CfgAllPathValidator
        {
            private readonly ISymbol memberToCheck;
            private readonly CancellationToken cancel;
            private readonly ControlFlowGraph cfg;

            public RoslynChecker(ControlFlowGraph cfg, ISymbol memberToCheck, CancellationToken cancel) : base(cfg)
            {
                this.cfg = cfg;
                this.memberToCheck = memberToCheck;
                this.cancel = cancel;
            }

            // Returns true if the block contains assignment before access
            protected override bool IsValid(BasicBlock block) =>
                ProcessBlock(block, cfg, false);

            // Returns true if the block contains access before assignment
            protected override bool IsInvalid(BasicBlock block) =>
                ProcessBlock(block, cfg, true);

            private bool ProcessBlock(BasicBlock block, ControlFlowGraph controlFlowGraph, bool checkReadBeforeWrite)
            {
                foreach (var operation in block.OperationsAndBranchValue.Reverse().ToReversedExecutionOrder())
                {
                    if (checkReadBeforeWrite && operation.Instance.Kind == OperationKindEx.FlowAnonymousFunction)
                    {
                        var anonymousFunction = IFlowAnonymousFunctionOperationWrapper.FromOperation(operation.Instance);
                        var anonymousFunctionCfg = controlFlowGraph.GetAnonymousFunctionControlFlowGraph(anonymousFunction, cancel);
                        if (anonymousFunctionCfg.Blocks.Any(x => ProcessBlock(x, anonymousFunctionCfg, checkReadBeforeWrite)))
                        {
                            return true;
                        }
                    }
                    else if (memberToCheck.Equals(MemberSymbol(operation.Instance)))
                    {
                        return IsReadOrWrite(operation, checkReadBeforeWrite);
                    }
                }
                return false;
            }

            private bool IsReadOrWrite(IOperationWrapperSonar child, bool checkReadBeforeWrite)
            {
                if (child.Instance.IsOutArgumentReference())
                {
                    // it is out argument - that means that this is write
                    return !checkReadBeforeWrite;
                }

                var isWrite = child.Parent is { Kind: OperationKindEx.SimpleAssignment } parent && ISimpleAssignmentOperationWrapper.FromOperation(parent).Target == child.Instance;
                return checkReadBeforeWrite ^ isWrite;
            }

            private static ISymbol MemberSymbol(IOperation operation) =>
                operation.Kind switch
                {
                    OperationKindEx.FieldReference when IFieldReferenceOperationWrapper.FromOperation(operation) is var fieldReference && InstanceReferencesThis(fieldReference.Instance) =>
                        fieldReference.Field,
                    OperationKindEx.PropertyReference when IPropertyReferenceOperationWrapper.FromOperation(operation) is var propertyReference && InstanceReferencesThis(propertyReference.Instance) =>
                        propertyReference.Property,
                    OperationKindEx.EventReference when IEventReferenceOperationWrapper.FromOperation(operation) is var eventReference && InstanceReferencesThis(eventReference.Instance) =>
                        eventReference.Member,
                    _ => null
                };

            private static bool InstanceReferencesThis(IOperation instance) =>
                instance == null || instance.IsAnyKind(OperationKindEx.FlowCaptureReference, OperationKindEx.InstanceReference);
        }
    }
}
