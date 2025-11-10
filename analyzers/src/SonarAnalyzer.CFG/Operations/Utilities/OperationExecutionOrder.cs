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

using System.Collections;

namespace SonarAnalyzer.CFG.Operations.Utilities;

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

        public bool MoveNext()
        {
            while (stack.Any())
            {
                if (owner.reverseOrder)
                {
                    var current = stack.Pop();
                    while (current.NextChild() is { } child)
                    {
                        stack.Push(new StackItem(child));
                    }
                    Current = current.DisposeEnumeratorAndReturnOperation();
                    return true;
                }
                else if (stack.Peek().NextChild() is { } child)
                {
                    stack.Push(new StackItem(child));
                }
                else
                {
                    Current = stack.Pop().DisposeEnumeratorAndReturnOperation();
                    return true;
                }
            }
            Current = default;
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

        private void Init()
        {
            // We need to push them to the stack in reversed order compared to reverseOrder argument
            foreach (var operation in owner.reverseOrder ? owner.operations : owner.operations.Reverse())
            {
                stack.Push(new StackItem(operation));
            }
        }
    }

    private sealed class StackItem : IDisposable
    {
        private readonly IOperationWrapperSonar operation;
        private readonly IEnumerator<IOperation> children;

        public StackItem(IOperation operation)
        {
            this.operation = operation.ToSonar();
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
