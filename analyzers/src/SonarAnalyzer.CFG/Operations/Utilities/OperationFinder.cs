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

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.CFG.Operations.Utilities;

public abstract class OperationFinder<TResult>
{
    protected abstract bool TryFindOperation(IOperationWrapperSonar operation, out TResult result);

    public bool TryFind(BasicBlock block, out TResult result) =>
        TryFind(block.OperationsAndBranchValue, out result);

    protected bool TryFind(IEnumerable<IOperation> operations, out TResult result)
    {
        var queue = new Queue<IOperation>();
        foreach (var operation in operations)
        {
            queue.Enqueue(operation);
            while (queue.Any())
            {
                var wrapper = queue.Dequeue().ToSonar();
                if (TryFindOperation(wrapper, out result))
                {
                    return true;
                }

                foreach (var child in wrapper.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        result = default;
        return false;
    }
}
