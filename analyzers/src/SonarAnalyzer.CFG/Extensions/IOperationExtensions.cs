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

namespace SonarAnalyzer.Extensions
{
    public static class IOperationExtensions
    {
        // This method is taken from Roslyn implementation
        public static IEnumerable<IOperation> DescendantsAndSelf(this IOperation operation) =>
            Descendants(operation, true);

        // This method is taken from Roslyn implementation
        private static IEnumerable<IOperation> Descendants(IOperation operation, bool includeSelf)
        {
            if (operation == null)
            {
                yield break;
            }
            if (includeSelf)
            {
                yield return operation;
            }
            var stack = new Stack<IEnumerator<IOperation>>();
            stack.Push(new IOperationWrapperSonar(operation).Children.GetEnumerator());
            while (stack.Any())
            {
                var iterator = stack.Pop();
                if (!iterator.MoveNext())
                {
                    continue;
                }

                stack.Push(iterator);
                if (iterator.Current is { } current)
                {
                    yield return current;
                    stack.Push(new IOperationWrapperSonar(current).Children.GetEnumerator());
                }
            }
        }
    }
}
