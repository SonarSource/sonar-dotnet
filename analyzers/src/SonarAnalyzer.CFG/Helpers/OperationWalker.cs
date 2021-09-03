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
using Microsoft.CodeAnalysis;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG.Helpers
{
    public abstract class OperationWalker : OperationWalker<object>
    {
        protected abstract void VoidVisitOperation(IOperation operation);

        protected override object VisitOperation(IOperation operation)
        {
            VoidVisitOperation(operation);
            return null;
        }
    }

    public abstract class OperationWalker<TResult> : OperationVisitor<TResult>
        where TResult : class
    {
        protected TResult Visit(IOperation operation)
        {
            if (operation == null)
            {
                return default;
            }
            var queue = new Queue<IOperation>();
            queue.Enqueue(operation);
            while (queue.Count != 0 && queue.Dequeue() is var elem)
            {
                if (VisitOperation(elem) is { } result)
                {
                    return result;
                }

                var wrapper = new IOperationWrapperSonar(elem);
                foreach (var child in wrapper.Children)
                {
                    queue.Enqueue(child);
                }
            }

            return default;
        }
    }
}
