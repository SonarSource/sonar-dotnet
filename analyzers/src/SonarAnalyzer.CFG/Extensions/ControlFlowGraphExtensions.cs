/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.CFG.Extensions;

public static class ControlFlowGraphExtensions
{
    public static IEnumerable<IFlowAnonymousFunctionOperationWrapper> FlowAnonymousFunctionOperations(this ControlFlowGraph cfg) =>
        cfg.Blocks
           .SelectMany(x => x.OperationsAndBranchValue)
           .SelectMany(x => x.DescendantsAndSelf())
           .Where(IFlowAnonymousFunctionOperationWrapper.IsInstance)
           .Select(IFlowAnonymousFunctionOperationWrapper.FromOperation);

    // Similar to ControlFlowGraphExtensions.GetLocalFunctionControlFlowGraphInScope from Roslyn
    public static ControlFlowGraph FindLocalFunctionCfgInScope(this ControlFlowGraph cfg, IMethodSymbol localFunction, CancellationToken cancel)
    {
        var current = cfg;
        while (current is not null)
        {
            if (current.LocalFunctions.Contains(localFunction))
            {
                return current.GetLocalFunctionControlFlowGraph(localFunction, cancel);
            }
            current = current.Parent;
        }
        throw new ArgumentOutOfRangeException(nameof(localFunction));
    }

    public static ControlFlowGraph GetLocalFunctionControlFlowGraph(this ControlFlowGraph cfg, SyntaxNode localFunction, CancellationToken cancel) =>
        cfg.GetLocalFunctionControlFlowGraph(cfg.LocalFunctions.Single(x => x.DeclaringSyntaxReferences.Single().GetSyntax() == localFunction), cancel);
}
