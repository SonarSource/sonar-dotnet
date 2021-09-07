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
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
//FIXME: using Microsoft.CodeAnalysis.CSharp;
//FIXME: using Microsoft.CodeAnalysis.CSharp.Syntax;
//FIXME:
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
//FIXME: using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG.LiveVariableAnalysis
{
    public sealed class RoslynLiveVariableAnalysis : LiveVariableAnalysisBase<ControlFlowGraph, BasicBlock>
    {
        private readonly ISymbol declaration;
        private readonly SemanticModel semanticModel;

        protected override BasicBlock ExitBlock => cfg.ExitBlock;

        //FIXME: What about LocalLifetime regions? Can we improve based on them?

        public RoslynLiveVariableAnalysis(ControlFlowGraph cfg, ISymbol declaration, SemanticModel semanticModel) : base(cfg)
        {
            this.declaration = declaration;
            this.semanticModel = semanticModel;
            Analyze();
        }

        protected override IEnumerable<BasicBlock> ReversedBlocks() =>
            cfg.Blocks.Reverse();

        protected override IEnumerable<BasicBlock> Successors(BasicBlock block) =>
            block.SuccessorBlocks;

        protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block) =>
            block.Predecessors.Select(x => x.Source);

        internal static bool IsOutArgument(IOperation operation) =>
            new IOperationWrapperSonar(operation) is var wrapped
            && IArgumentOperationWrapper.IsInstance(wrapped.Parent)
            && IArgumentOperationWrapper.FromOperation(wrapped.Parent).Parameter.RefKind == RefKind.Out;

        //internal static bool IsLocalScoped(ISymbol symbol, ISymbol declaration)
        //{
        //    return IsLocalOrParameterSymbol()
        //        && symbol.ContainingSymbol != null
        //        && symbol.ContainingSymbol.Equals(declaration);

        //    bool IsLocalOrParameterSymbol() =>
        //        (symbol is ILocalSymbol local && local.RefKind() == RefKind.None)
        //        || (symbol is IParameterSymbol parameter && parameter.RefKind == RefKind.None);
        //}

        protected override State ProcessBlock(BasicBlock block)
        {
            //FIXME: Ugly
            var ret = new State();
            ProcessBlockInternal(block, ret);

            // FIXME: Remove debug
            Console.WriteLine();
            Console.WriteLine($"Processing {block.Kind} #{block.Ordinal}");
            Console.WriteLine($"Assigned: " + string.Join(", ", ret.Assigned.Select(x => x.Name)));
            Console.WriteLine($"UsedBeforeAssigned: " + string.Join(", ", ret.UsedBeforeAssigned.Select(x => x.Name)));
            Console.WriteLine($"ProcessedLocalFunctions: " + string.Join(", ", ret.ProcessedLocalFunctions.Select(x => x.Name)));
            Console.WriteLine($"Captured: " + string.Join(", ", ret.Captured.Select(x => x.Name)));

            return ret;
        }

        //FIXME: Make it an extension
        private static IEnumerable<IOperationWrapperSonar> ToExecutionOrder(IEnumerable<IOperation> operations)
        {
            var stack = new Stack<StackItem>();
            try
            {
                foreach (var operation in operations.Reverse())
                {
                    stack.Push(new StackItem(operation));
                }
                while (stack.Any())
                {
                    if (stack.Peek().NextChild() is { } child)
                    {
                        stack.Push(new StackItem(child));
                    }
                    else
                    {
                        yield return stack.Pop().DisposeEnumeratorAndReturnOperation();
                    }
                }
            }
            finally
            {
                while (stack.Any())
                {
                    stack.Pop().Dispose();
                }
            }
        }

        private void ProcessBlockInternal(BasicBlock block, State state)
        {
            foreach (var operation in ToExecutionOrder(block.BranchValueAndOperations.Reverse()))
            {
                switch (operation.Instance.Kind)
                {
                    case OperationKindEx.LocalReference:
                        ProcessParameterOrLocalReference(state, ILocalReferenceOperationWrapper.FromOperation(operation.Instance));
                        break;
                    case OperationKindEx.ParameterReference:
                        ProcessParameterOrLocalReference(state, IParameterReferenceOperationWrapper.FromOperation(operation.Instance));
                        break;
                    case OperationKindEx.SimpleAssignment:
                        ProcessSimpleAssignment(state, ISimpleAssignmentOperationWrapper.FromOperation(operation.Instance));
                        break;

                        //FIXME: Something is still missing around here

                        //            case SyntaxKind.GenericName:
                        //                ProcessGenericName((GenericNameSyntax)instruction, state);
                        //                break;

                        //            case SyntaxKind.AnonymousMethodExpression:
                        //            case SyntaxKind.ParenthesizedLambdaExpression:
                        //            case SyntaxKind.SimpleLambdaExpression:
                        //            case SyntaxKind.QueryExpression:
                        //                CollectAllCapturedLocal(instruction, state);
                        //                break;
                }
            }
            //FIXME: Something is still missing around here

            //    // Keep alive the variables declared and used in the using statement until the UsingFinalizerBlock
            //    if (block is UsingEndBlock usingFinalizerBlock)
            //    {
            //        var disposableSymbols = usingFinalizerBlock.Identifiers
            //            .Select(i => semanticModel.GetDeclaredSymbol(i.Parent)
            //                        ?? semanticModel.GetSymbolInfo(i.Parent).Symbol)
            //            .WhereNotNull();
            //        foreach (var disposableSymbol in disposableSymbols)
            //        {
            //            state.UsedBeforeAssigned.Add(disposableSymbol);
            //        }
            //    }
        }

        private void ProcessParameterOrLocalReference(State state, IOperationWrapper reference)
        {
            var symbol = ParameterOrLocalSymbol(reference.WrappedOperation);
            Debug.Assert(symbol != null, "Only supported types should be passed to ParameterOrLocalSymbol");
            if (IsOutArgument(reference.WrappedOperation))
            {
                state.Assigned.Add(symbol);
                state.UsedBeforeAssigned.Remove(symbol);
            }
            else if (!IsAssignmentTarget())
            {
                state.UsedBeforeAssigned.Add(symbol);
            }

            bool IsAssignmentTarget() =>
                new IOperationWrapperSonar(reference.WrappedOperation).Parent is { } parent
                && parent.Kind == OperationKindEx.SimpleAssignment
                && ISimpleAssignmentOperationWrapper.FromOperation(parent).Target == reference.WrappedOperation;
        }

        private void ProcessSimpleAssignment(State state, ISimpleAssignmentOperationWrapper assignment)
        {
            if (ParameterOrLocalSymbol(assignment.Target) is { } localTarget)
            {
                state.Assigned.Add(localTarget);
                state.UsedBeforeAssigned.Remove(localTarget);
            }
        }

        //private void ProcessIdentifier(IdentifierNameSyntax identifier, State state)
        //{
        //    if (!identifier.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(semanticModel)
        //        && semanticModel.GetSymbolInfo(identifier).Symbol is { } symbol)
        //    {
        //        if (IsLocalScoped(symbol))
        //        {
        //            if (IsOutArgument(identifier))
        //            {
        //                state.Assigned.Add(symbol);
        //                state.UsedBeforeAssigned.Remove(symbol);
        //            }
        //            else if (!state.AssignmentLhs.Contains(identifier))
        //            {
        //                state.UsedBeforeAssigned.Add(symbol);
        //            }
        //        }

        //        if (symbol is IMethodSymbol { MethodKind: MethodKindEx.LocalFunction } method)
        //        {
        //            ProcessLocalFunction(symbol, state);
        //        }
        //    }
        //}

        //private void ProcessGenericName(GenericNameSyntax genericName, State state)
        //{
        //    if (!genericName.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(semanticModel)
        //        && semanticModel.GetSymbolInfo(genericName).Symbol is IMethodSymbol {MethodKind: MethodKindEx.LocalFunction } method)
        //    {
        //        ProcessLocalFunction(method, state);
        //    }
        //}

        //private void ProcessLocalFunction(ISymbol symbol, State state)
        //{
        //    if (!state.ProcessedLocalFunctions.Contains(symbol)
        //        && symbol.DeclaringSyntaxReferences.Length == 1
        //        && symbol.DeclaringSyntaxReferences.Single().GetSyntax() is { } node
        //        && (LocalFunctionStatementSyntaxWrapper)node is LocalFunctionStatementSyntaxWrapper function
        //        && CSharpControlFlowGraph.TryGet(function.Body ?? function.ExpressionBody as CSharpSyntaxNode, semanticModel, out var cfg))
        //    {
        //        state.ProcessedLocalFunctions.Add(symbol);
        //        foreach (var block in cfg.Blocks.Reverse())
        //        {
        //            ProcessBlockInternal(block, state);
        //        }
        //    }
        //}

        //private void CollectAllCapturedLocal(SyntaxNode instruction, State state)
        //{
        //    var allCapturedSymbols = instruction.DescendantNodes()
        //        .OfType<IdentifierNameSyntax>()
        //        .Select(i => semanticModel.GetSymbolInfo(i).Symbol)
        //        .Where(s => s != null && IsLocalScoped(s));

        //    // Collect captured locals
        //    // Read and write both affects liveness
        //    state.CapturedVariables.UnionWith(allCapturedSymbols);
        //}

        private static ISymbol ParameterOrLocalSymbol(IOperation operation) =>
            operation switch
            {
                var _ when IParameterReferenceOperationWrapper.IsInstance(operation) => IParameterReferenceOperationWrapper.FromOperation(operation).Parameter,
                var _ when ILocalReferenceOperationWrapper.IsInstance(operation) => ILocalReferenceOperationWrapper.FromOperation(operation).Local,
                _ => null
            };

        //private bool IsLocalScoped(ISymbol symbol) =>
        //    IsLocalScoped(symbol, declaration);

        private class StackItem : IDisposable
        {
            private readonly IOperationWrapperSonar operation;
            private readonly IEnumerator<IOperation> children;
            private bool isDisposed;

            public StackItem(IOperation operation)
            {
                this.operation = new IOperationWrapperSonar(operation);
                children = this.operation.Children.GetEnumerator();
            }

            public IOperation NextChild() =>
                children.MoveNext() ? children.Current : null;

            public IOperationWrapperSonar DisposeEnumeratorAndReturnOperation()
            {
                Dispose();
                return operation;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!isDisposed)
                {
                    children.Dispose();
                    isDisposed = true;
                }
            }

            public void Dispose() =>
                Dispose(true);
        }
    }
}
