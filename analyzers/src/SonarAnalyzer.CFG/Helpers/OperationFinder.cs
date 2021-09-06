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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Roslyn;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG.Helpers
{
    public abstract class OperationFinder<TResult>
    {
        protected abstract bool TryFindOperation(IOperationWrapperSonar operation, out TResult result);

        public bool TryFind(BasicBlock block, out TResult var) =>
            TryFind(block.OperationsAndBranchValue, out var);

        protected bool TryFind(IEnumerable<IOperation> operations, out TResult result)
        {
            var queue = new Queue<IOperation>();
            foreach (var operation in operations)
            {
                queue.Enqueue(operation);
                while (queue.Any())
                {
                    var wrapper = new IOperationWrapperSonar(queue.Dequeue());
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
}
