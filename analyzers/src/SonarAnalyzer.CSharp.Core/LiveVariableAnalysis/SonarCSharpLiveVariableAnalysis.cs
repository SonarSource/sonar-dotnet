/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.CSharp.Core.LiveVariableAnalysis;

public sealed class SonarCSharpLiveVariableAnalysis : LiveVariableAnalysisBase<IControlFlowGraph, Block>
{
    private readonly SemanticModel model;

    protected override Block ExitBlock => Cfg.ExitBlock;

    public SonarCSharpLiveVariableAnalysis(IControlFlowGraph controlFlowGraph, ISymbol originalDeclaration, SemanticModel model, CancellationToken cancel)
        : base(controlFlowGraph, originalDeclaration, cancel)
    {
        this.model = model;
        Analyze();
    }

    public override bool IsLocal(ISymbol symbol)
    {
        return IsLocalOrParameterSymbol()
            && symbol.ContainingSymbol is not null
            && symbol.ContainingSymbol.Equals(originalDeclaration);

        bool IsLocalOrParameterSymbol() =>
            (symbol is ILocalSymbol local && local.RefKind() == RefKind.None)
            || (symbol is IParameterSymbol parameter && parameter.RefKind == RefKind.None);
    }

    public static bool IsOutArgument(IdentifierNameSyntax identifier) =>
        identifier.GetFirstNonParenthesizedParent() is ArgumentSyntax argument && argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword);

    protected override IEnumerable<Block> ReversedBlocks() =>
        Cfg.Blocks.Reverse();

    protected override IEnumerable<Block> Successors(Block block) =>
        block.SuccessorBlocks;

    protected override IEnumerable<Block> Predecessors(Block block) =>
        block.PredecessorBlocks;

    protected override State ProcessBlock(Block block)
    {
        var ret = new SonarState(this, model);
        ret.ProcessBlock(block);
        return ret;
    }

    private sealed class SonarState : State
    {
        private readonly SonarCSharpLiveVariableAnalysis owner;
        private readonly SemanticModel model;

        public ISet<SyntaxNode> AssignmentLhs { get; } = new HashSet<SyntaxNode>();

        public SonarState(SonarCSharpLiveVariableAnalysis owner, SemanticModel model)
        {
            this.owner = owner;
            this.model = model;
        }

        public void ProcessBlock(Block block)
        {
            foreach (var instruction in block.Instructions.Reverse())
            {
                switch (instruction.Kind())
                {
                    case SyntaxKind.IdentifierName:
                        ProcessIdentifier((IdentifierNameSyntax)instruction);
                        break;

                    case SyntaxKind.GenericName:
                        ProcessGenericName((GenericNameSyntax)instruction);
                        break;

                    case SyntaxKind.SimpleAssignmentExpression:
                        ProcessSimpleAssignment((AssignmentExpressionSyntax)instruction);
                        break;

                    case SyntaxKind.VariableDeclarator:
                        ProcessVariableDeclarator((VariableDeclaratorSyntax)instruction);
                        break;
                    case SyntaxKind.AnonymousMethodExpression:
                    case SyntaxKind.ParenthesizedLambdaExpression:
                    case SyntaxKind.SimpleLambdaExpression:
                    case SyntaxKind.QueryExpression:
                        CollectAllCapturedLocal(instruction);
                        break;
                }
            }

            if (block.Instructions.Any())
            {
                return;
            }

            // Variable declaration in a foreach statement is not a VariableDeclarator, so handling it separately:
            if (block is BinaryBranchBlock foreachBlock && foreachBlock.BranchingNode.IsKind(SyntaxKind.ForEachStatement))
            {
                var foreachNode = (ForEachStatementSyntax)foreachBlock.BranchingNode;
                ProcessVariableInForeach(foreachNode);
            }

            // Keep alive the variables declared and used in the using statement until the UsingFinalizerBlock
            if (block is UsingEndBlock usingFinalizerBlock)
            {
                var disposableSymbols = usingFinalizerBlock.Identifiers
                    .Select(x => model.GetDeclaredSymbol(x.Parent) ?? model.GetSymbolInfo(x.Parent).Symbol)
                    .WhereNotNull();
                foreach (var disposableSymbol in disposableSymbols)
                {
                    UsedBeforeAssigned.Add(disposableSymbol);
                }
            }
        }

        private void ProcessVariableInForeach(ForEachStatementSyntax foreachNode)
        {
            if (model.GetDeclaredSymbol(foreachNode) is { } symbol)
            {
                Assigned.Add(symbol);
                UsedBeforeAssigned.Remove(symbol);
            }
        }

        private void ProcessVariableDeclarator(VariableDeclaratorSyntax instruction)
        {
            if (model.GetDeclaredSymbol(instruction) is { } symbol)
            {
                Assigned.Add(symbol);
                UsedBeforeAssigned.Remove(symbol);
            }
        }

        private void ProcessSimpleAssignment(AssignmentExpressionSyntax assignment)
        {
            var left = assignment.Left.RemoveParentheses();
            if (left.IsKind(SyntaxKind.IdentifierName)
                && model.GetSymbolInfo(left).Symbol is { } symbol
                && owner.IsLocal(symbol))
            {
                AssignmentLhs.Add(left);
                Assigned.Add(symbol);
                UsedBeforeAssigned.Remove(symbol);
            }
        }

        private void ProcessIdentifier(IdentifierNameSyntax identifier)
        {
            if (!identifier.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(model)
                && model.GetSymbolInfo(identifier).Symbol is { } symbol)
            {
                if (owner.IsLocal(symbol))
                {
                    if (IsOutArgument(identifier))
                    {
                        Assigned.Add(symbol);
                        UsedBeforeAssigned.Remove(symbol);
                    }
                    else if (!AssignmentLhs.Contains(identifier))
                    {
                        UsedBeforeAssigned.Add(symbol);
                    }
                }

                if (symbol is IMethodSymbol { MethodKind: MethodKindEx.LocalFunction })
                {
                    ProcessLocalFunction(symbol);
                }
            }
        }

        private void ProcessGenericName(GenericNameSyntax genericName)
        {
            if (!genericName.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(model)
                && model.GetSymbolInfo(genericName).Symbol is IMethodSymbol { MethodKind: MethodKindEx.LocalFunction } method)
            {
                ProcessLocalFunction(method);
            }
        }

        private void ProcessLocalFunction(ISymbol symbol)
        {
            if (!ProcessedLocalFunctions.Contains(symbol)
                && symbol.DeclaringSyntaxReferences.Length == 1
                && symbol.DeclaringSyntaxReferences.Single().GetSyntax() is { } node
                && LocalFunctionStatementSyntaxWrapper.IsInstance(node)
                && CSharpControlFlowGraph.TryGet((LocalFunctionStatementSyntaxWrapper)node, model, out var cfg))
            {
                ProcessedLocalFunctions.Add(symbol);
                foreach (var block in cfg.Blocks.Reverse())
                {
                    ProcessBlock(block);
                }
            }
        }

        private void CollectAllCapturedLocal(SyntaxNode instruction)
        {
            var allCapturedSymbols = instruction.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(x => model.GetSymbolInfo(x).Symbol)
                .Where(owner.IsLocal);

            // Collect captured locals
            // Read and write both affects liveness
            Captured.UnionWith(allCapturedSymbols);
        }
    }
}
