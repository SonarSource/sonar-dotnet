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

        public OperationExecutionOrder(IEnumerable<IOperation> operations) =>
            this.operations = operations;

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
                //FIXME: Validate this pro LVA, there's Reverse/Reverse logic
                foreach (var operation in owner.operations.Reverse())
                {
                    stack.Push(new StackItem(operation));
                }
            }

            public bool MoveNext()
            {
                while (stack.Any())
                {
                    //FIXME: Prev pro LVA
                    //FIXME: Next pro SE
                    if (stack.Peek().NextChild() is { } child)
                    {
                        stack.Push(new StackItem(child));
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

            public void Dispose() =>
                children.Dispose();
        }
    }
}
