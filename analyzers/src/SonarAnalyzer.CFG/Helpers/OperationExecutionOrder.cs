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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Helpers
{
    public class OperationExecutionOrder : IEnumerable<IOperationWrapperSonar>
    {
        private readonly IEnumerable<IOperation> operations;
        private readonly bool reverseOrder;

        public OperationExecutionOrder(IEnumerable<IOperation> operations, bool reverseOrder)
        {
            this.operations = operations;
            this.reverseOrder = reverseOrder;
        }

        public IEnumerator<IOperationWrapperSonar> GetEnumerator() =>
            new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private sealed class Enumerator : IEnumerator<IOperationWrapperSonar>
        {
            private readonly OperationExecutionOrder owner;
            private readonly Stack<StackItem> stack = new Stack<StackItem>();

            public IOperationWrapperSonar Current { get; private set; }
            object IEnumerator.Current => Current;

            public Enumerator(OperationExecutionOrder owner)
            {
                this.owner = owner;
                Init();
            }

            private void Init()
            {
                // We need to push them to the stack in reversed order compared to reverseOrder argument
                foreach (var operation in owner.reverseOrder ? owner.operations : owner.operations.Reverse())
                {
                    stack.Push(new StackItem(operation, owner.reverseOrder));
                }
            }

            public bool MoveNext()
            {
                while (stack.Any())
                {
                    if (owner.reverseOrder)
                    {
                        var current = stack.Pop();
                        while (current.NextChild() is { } child)
                        {
                            stack.Push(new StackItem(child, owner.reverseOrder));
                        }

                        Current = current.DisposeEnumeratorAndReturnOperation();
                        return true;
                    }
                    else if (!owner.reverseOrder && stack.Peek().NextChild() is { } child)
                    {
                        stack.Push(new StackItem(child, owner.reverseOrder));
                    }
                    else
                    {
                        Current = stack.Pop().DisposeEnumeratorAndReturnOperation();
                        return true;
                    }
                }
                Current = null;
                return false;
            }

            public void Reset()
            {
                Dispose();
                Init();
            }

            public void Dispose()
            {
                while (stack.Any())
                {
                    stack.Pop().Dispose();
                }
            }
        }

        private sealed class StackItem : IDisposable
        {
            private readonly IOperationWrapperSonar operation;
            private readonly IEnumerator<IOperation> children;

            public StackItem(IOperation operation, bool reversedOrder)
            {
                this.operation = new IOperationWrapperSonar(operation);
                children = //reversedOrder
                    //? this.operation.Children.Reverse().GetEnumerator()
                    //:
                    this.operation.Children.GetEnumerator();
            }

            public IOperation NextChild() =>
                children.MoveNext() ? children.Current : null;

            public IOperationWrapperSonar DisposeEnumeratorAndReturnOperation()
            {
                Dispose();
                return operation;
            }

            public void Dispose() =>
                children.Dispose();
        }
    }
}
