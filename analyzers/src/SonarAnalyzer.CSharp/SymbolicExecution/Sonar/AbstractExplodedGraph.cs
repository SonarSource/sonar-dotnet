﻿/*
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

using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Sonar
{
    internal abstract class AbstractExplodedGraph
    {
        internal const int MaxStepCount = 1500;
        internal const int MaxInternalStateCount = 10000;
        private const int MaxProgramPointExecutionCount = 2;

        private readonly Dictionary<ProgramPoint, ProgramPoint> programPoints = new Dictionary<ProgramPoint, ProgramPoint>();
        private readonly Queue<ExplodedGraphNode> workList = new Queue<ExplodedGraphNode>();
        private readonly HashSet<ExplodedGraphNode> nodesAlreadyInGraph = new HashSet<ExplodedGraphNode>();

        private readonly IControlFlowGraph cfg;
        private readonly ISymbol declaration;
        private readonly IEnumerable<IParameterSymbol> declarationParameters;
        private readonly IEnumerable<IParameterSymbol> nonInDeclarationParameters;
        private readonly SonarCSharpLiveVariableAnalysis lva;

        protected readonly ICollection<ExplodedGraphCheck> explodedGraphChecks = new List<ExplodedGraphCheck>();

        internal SemanticModel SemanticModel { get; }

        public event EventHandler ExplorationEnded;

        public event EventHandler MaxStepCountReached;

        public event EventHandler MaxInternalStateCountReached;

        public event EventHandler<InstructionProcessedEventArgs> InstructionProcessed;

        public event EventHandler<VisitCountExceedLimitEventArgs> ProgramPointVisitCountExceedLimit;

        public event EventHandler ExitBlockReached;

        public event EventHandler<ConditionEvaluatedEventArgs> ConditionEvaluated;

        protected AbstractExplodedGraph(IControlFlowGraph cfg, ISymbol declaration, SemanticModel semanticModel, SonarCSharpLiveVariableAnalysis lva)
        {
            this.cfg = cfg;
            this.declaration = declaration;
            this.lva = lva;

            SemanticModel = semanticModel;

            this.declarationParameters = declaration.GetParameters();
            this.nonInDeclarationParameters = this.declarationParameters.Where(p => p.RefKind != RefKind.None);
        }

        public void Walk()
        {
            var steps = 0;

            EnqueueStartNode();

            while (this.workList.Any())
            {
                if (steps >= MaxStepCount)
                {
                    OnMaxStepCountReached();
                    return;
                }

                steps++;
                var node = this.workList.Dequeue();
                var programPoint = node.ProgramPoint;

                if (programPoint.Block is ExitBlock)
                {
                    OnExitBlockReached();
                    continue;
                }

                try
                {
                    if (programPoint.Offset < programPoint.Block.Instructions.Count)
                    {
                        VisitInstruction(node);
                        continue;
                    }

                    if (programPoint.Block is BinaryBranchBlock binaryBranchBlock)
                    {
                        VisitBinaryBranch(binaryBranchBlock, node);
                        continue;
                    }

                    if (programPoint.Block is BinaryBranchingSimpleBlock singleSuccessorBinaryBranchBlock)
                    {
                        // Right operand of logical && and ||
                        VisitSingleSuccessorBinaryBranch(singleSuccessorBinaryBranchBlock, node);
                        continue;
                    }

                    if (programPoint.Block is SimpleBlock simpleBlock)
                    {
                        VisitSimpleBlock(simpleBlock, node);
                        continue;
                    }

                    if (programPoint.Block is BranchBlock branchBlock)
                    {
                        // switch:
                        VisitBranchBlock(branchBlock, node);
                    }
                }
                catch (TooManyInternalStatesException)
                {
                    OnMaxInternalStateCountReached();
                    return;
                }
            }

            OnExplorationEnded();
        }

        internal void AddExplodedGraphCheck<T>(T check)
            where T : ExplodedGraphCheck
        {
            var matchingCheck = this.explodedGraphChecks.OfType<T>().SingleOrDefault();
            if (matchingCheck == null)
            {
                this.explodedGraphChecks.Add(check);
            }
            else
            {
                this.explodedGraphChecks.Remove(matchingCheck);
                this.explodedGraphChecks.Add(check);
            }
        }

        #region OnEvent*

        private void OnExplorationEnded()
        {
            ExplorationEnded?.Invoke(this, EventArgs.Empty);
        }

        private void OnMaxStepCountReached()
        {
            MaxStepCountReached?.Invoke(this, EventArgs.Empty);
        }

        private void OnMaxInternalStateCountReached()
        {
            MaxInternalStateCountReached?.Invoke(this, EventArgs.Empty);
        }

        private void OnExitBlockReached()
        {
            ExitBlockReached?.Invoke(this, EventArgs.Empty);
        }

        private void OnProgramPointVisitCountExceedLimit(ProgramPoint programPoint, ProgramState programState)
        {
            ProgramPointVisitCountExceedLimit?.Invoke(this, new VisitCountExceedLimitEventArgs
            {
                Limit = MaxProgramPointExecutionCount,
                ProgramPoint = programPoint,
                ProgramState = programState
            });
        }

        protected void OnInstructionProcessed(SyntaxNode instruction, ProgramPoint programPoint, ProgramState programState)
        {
            InstructionProcessed?.Invoke(this, new InstructionProcessedEventArgs
            {
                Instruction = instruction,
                ProgramPoint = programPoint,
                ProgramState = programState
            });
        }

        protected void OnConditionEvaluated(SyntaxNode condition, bool evaluationValue)
        {
            ConditionEvaluated?.Invoke(this, new ConditionEvaluatedEventArgs
            {
                Condition = condition,
                EvaluationValue = evaluationValue
            });
        }

        #endregion OnEvent*

        #region Visit*

        protected abstract void VisitBinaryBranch(BinaryBranchBlock binaryBranchBlock, ExplodedGraphNode node);

        protected abstract void VisitInstruction(ExplodedGraphNode node);

        protected virtual void VisitBranchBlock(BranchBlock branchBlock, ExplodedGraphNode node)
        {
            var newProgramState = node.ProgramState;
            if (IsValueConsumingStatement(branchBlock.BranchingNode))
            {
                newProgramState = newProgramState.PopValue();
            }
            newProgramState = CleanStateAfterBlock(newProgramState, branchBlock);
            EnqueueAllSuccessors(branchBlock, newProgramState);
        }

        protected virtual void VisitSingleSuccessorBinaryBranch(BinaryBranchingSimpleBlock block, ExplodedGraphNode node)
        {
            var programState = node.ProgramState.PopValue(out var sv);

            foreach (var newProgramState in sv.TrySetConstraint(BoolConstraint.True, programState))
            {
                OnConditionEvaluated(block.BranchingInstruction, evaluationValue: true);
                var nps = newProgramState.PushValue(SymbolicValue.True);
                EnqueueNewNode(new ProgramPoint(block.SuccessorBlock), nps);
            }

            foreach (var newProgramState in sv.TrySetConstraint(BoolConstraint.False, programState))
            {
                OnConditionEvaluated(block.BranchingInstruction, evaluationValue: false);
                var nps = newProgramState.PushValue(SymbolicValue.False);
                EnqueueNewNode(new ProgramPoint(block.SuccessorBlock), nps);
            }
        }

        protected virtual void VisitSimpleBlock(SimpleBlock block, ExplodedGraphNode node)
        {
            var newProgramState = CleanStateAfterBlock(node.ProgramState, block);

            if (block is JumpBlock jumpBlock &&
                IsValueConsumingStatement(jumpBlock.JumpNode))
            {
                newProgramState = newProgramState.PopValue();
            }

            EnqueueAllSuccessors(block, newProgramState);
        }

        protected abstract bool IsValueConsumingStatement(SyntaxNode jumpNode);

        protected ProgramState CleanStateAfterBlock(ProgramState programState, Block block)
        {
            // We do not clean state when entering a using block statement, as some variables declared with "using" keywords
            // may still be implicitly disposed later, even if they are not referenced anymore. State will still be cleaned
            // in the "VisitSimpleBlock" of UsingEndBlock
            if (block is JumpBlock jumpBlock &&
                jumpBlock.SuccessorBlock is UsingEndBlock)
            {
               return programState;
            }

            var liveVariables = lva.LiveOut(block).Union(nonInDeclarationParameters); // LVA excludes out and ref parameters

            // ToDo: Remove the IFieldSymbol check when SLVS-1136 is fixed
            return programState.RemoveSymbols(
                symbol => !(symbol is IFieldSymbol) && !liveVariables.Contains(symbol));
        }

        internal bool IsSymbolTracked(ISymbol symbol)
        {
            if (symbol == null ||
                this.lva.CapturedVariables.Contains(symbol)) // Captured variables are not locally scoped, they are compiled to class fields
            {
                return false;
            }

            if (IsFieldSymbol(symbol))
            {
                return true;
            }

            if (!(symbol is ILocalSymbol local) &&
                !(symbol is IParameterSymbol parameter)) // No filter for ref/out
            {
                return false;
            }

            // Could be either ILocalSymbol or IParameterSymbol so let's use symbol
            return symbol.ContainingSymbol != null &&
                symbol.ContainingSymbol.Equals(this.declaration);
        }

        protected bool IsFieldSymbol(ISymbol symbol)
        {

            return symbol is IFieldSymbol field &&
                (field.IsConst ||
                this.declaration.ContainingType
                    .GetSelfAndBaseTypes()
                    .Contains(field.ContainingType));
        }

        protected abstract Block GetForEachExitBlock(Block block);

        #endregion Visit*

        #region Enqueue exploded graph node

        private void EnqueueStartNode()
        {
            var initialProgramState = new ProgramState();
            foreach (var parameter in this.declarationParameters)
            {
                var sv = SymbolicValue.Create(parameter.Type);
                initialProgramState = initialProgramState.StoreSymbolicValue(parameter, sv);
                initialProgramState = SetNonNullConstraintIfValueType(parameter, sv, initialProgramState);
            }

            EnqueueNewNode(new ProgramPoint(this.cfg.EntryBlock), initialProgramState);
        }

        protected void EnqueueAllSuccessors(Block block, ProgramState newProgramState)
        {
            foreach (var successorBlock in block.SuccessorBlocks)
            {
                EnqueueNewNode(new ProgramPoint(successorBlock), newProgramState);
            }
        }

        protected void EnqueueNewNode(ProgramPoint programPoint, ProgramState programState)
        {
            if (programState == null)
            {
                return;
            }

            var pos = programPoint;
            if (this.programPoints.ContainsKey(programPoint))
            {
                pos = this.programPoints[programPoint];
            }
            else
            {
                this.programPoints[pos] = pos;
            }

            ExplodedGraphNode newNode = null;
            if (programState.GetVisitedCount(pos) >= MaxProgramPointExecutionCount)
            {
                OnProgramPointVisitCountExceedLimit(pos, programState);

                // reached the max number of visit by program point, in the case of ForEach blocks, we take the foreach loop false branch with current program state,
                // if it is not a foreach loop, newNode will be null and we will stop exploring the path
                if (GetForEachExitBlock(programPoint.Block) is Block forEachExitBlock)
                {
                    newNode = new ExplodedGraphNode(new ProgramPoint(forEachExitBlock), programState);
                }
            }
            else
            {
                newNode = new ExplodedGraphNode(pos, programState.AddVisit(pos));
            }

            if (newNode != null && this.nodesAlreadyInGraph.Add(newNode))
            {
                this.workList.Enqueue(newNode);
            }
        }

        #endregion Enqueue exploded graph node

        protected ProgramState SetNewSymbolicValueIfTracked(ISymbol symbol, SymbolicValue symbolicValue, ProgramState programState)
        {
            return IsSymbolTracked(symbol)
                ? programState.StoreSymbolicValue(symbol, symbolicValue)
                : programState;
        }

        private static bool IsPointer(ITypeSymbol typeSymbol)
        {
            return typeSymbol?.TypeKind == TypeKind.Pointer;
        }

        private static bool IsValueTypeWithOverloadedNullCompatibleOpEquals(ITypeSymbol type)
        {
            if (type == null ||
                !type.IsValueType)
            {
                return false;
            }

            return type.GetMembers("op_Equality")
                .OfType<IMethodSymbol>()
                .Any(m => m.Parameters.Any(p => IsNullCompatibleType(p.Type)));
        }

        private static bool IsNullCompatibleType(ITypeSymbol type)
        {
            if (type == null)
            {
                return false;
            }

            return !type.IsValueType ||
                type.OriginalDefinition.Is(KnownType.System_Nullable_T);
        }

        protected static ProgramState SetNonNullConstraintIfValueType(ITypeSymbol typeSymbol,
            SymbolicValue symbolicValue, ProgramState programState)
        {
            var isDefinitelyNotNull = !programState.HasConstraint(symbolicValue, ObjectConstraint.NotNull) &&
                IsNonNullableValueType(typeSymbol) &&
                !IsValueTypeWithOverloadedNullCompatibleOpEquals(typeSymbol) &&
                !IsPointer(typeSymbol);

            return isDefinitelyNotNull
                ? programState.SetConstraint(symbolicValue, ObjectConstraint.NotNull)
                : programState;
        }

        protected static ProgramState SetNonNullConstraintIfValueType(ISymbol symbol, SymbolicValue symbolicValue, ProgramState programState)
        {
            return SetNonNullConstraintIfValueType(symbol.GetSymbolType(), symbolicValue, programState);
        }

        protected ProgramState SetNonNullConstraintIfValueType(SyntaxNode node, SymbolicValue symbolicValue, ProgramState programState)
        {
            return SetNonNullConstraintIfValueType(SemanticModel.GetTypeInfo(node).Type, symbolicValue, programState);
        }

        private static bool IsNonNullableValueType(ITypeSymbol type)
        {
            return type != null &&
                type.IsValueType &&
                !type.OriginalDefinition.Is(KnownType.System_Nullable_T);
        }
    }
}
