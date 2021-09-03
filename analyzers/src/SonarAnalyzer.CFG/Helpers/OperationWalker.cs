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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG.Helpers
{
    public abstract class OperationWalker : OperationWalker<object>
    {
        protected abstract void VoidVisitOperation(IOperationWrapperSonar operation);

        protected override bool VisitOperation(IOperationWrapperSonar operation)
        {
            VoidVisitOperation(operation);
            return true;
        }
    }

    public abstract class OperationWalker<TResult>
    {
        protected TResult Result { get; set; }

        protected abstract bool VisitOperation(IOperationWrapperSonar operation);

        protected TResult Visit(IEnumerable<IOperation> operations)
        {
            var queue = new Queue<IOperation>();
            foreach (var operation in operations)
            {
                queue.Enqueue(operation);
                while (queue.Any())
                {
                    var wrapper = new IOperationWrapperSonar(queue.Dequeue());
                    if (!VisitOperation(wrapper))
                    {
                        return Result;
                    }

                    foreach (var child in wrapper.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return default;
        }
    }
}
