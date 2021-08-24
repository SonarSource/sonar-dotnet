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
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class BasicBlock
    {
        private static readonly PropertyInfo OperationsProperty;

        private readonly Lazy<ImmutableArray<IOperation>> operations;

        public ImmutableArray<IOperation> Operations => operations.Value;

        static BasicBlock()
        {
            if (RoslynHelper.FlowAnalysisType("BasicBlock") is { } type)
            {
                OperationsProperty = type.GetProperty(nameof(Operations));
            }
        }

        public BasicBlock(object instance)
        {
            _ = instance ?? throw new ArgumentNullException(nameof(instance));
            operations = RoslynHelper.ReadImmutableArray(OperationsProperty, instance, x => (IOperation)x);
        }
    }
}
