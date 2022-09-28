/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

internal static class OperationDispatcher
{
    private static readonly Dictionary<OperationKind, IMultiProcessor> Branching = new()
    {
        { OperationKindEx.Binary, new Binary() },
        { OperationKindEx.Invocation, new Invocation() },
        { OperationKindEx.IsNull, new IsNull() },
        { OperationKindEx.IsPattern, new IsPattern() },
        { OperationKindEx.IsType, new IsType() }
    };

    private static readonly Dictionary<OperationKind, ISimpleProcessor> Simple = new()
    {
        { OperationKindEx.Argument, new Argument() },
        { OperationKindEx.ArrayCreation, new Creation() },
        { OperationKindEx.ArrayElementReference, new ArrayElementReference() },
        { OperationKindEx.AnonymousObjectCreation, new Creation() },
        { OperationKindEx.Await, new Await() },
        { OperationKindEx.Conversion, new Conversion() },
        { OperationKindEx.DeclarationPattern, new DeclarationPattern() },
        { OperationKindEx.DefaultValue, new DefaultValue() },
        { OperationKindEx.DelegateCreation, new Creation() },
        { OperationKindEx.DynamicObjectCreation, new Creation() },
        { OperationKindEx.EventReference, new EventReference() },
        { OperationKindEx.FieldReference, new FieldReference() },
        { OperationKindEx.FlowCapture, new FlowCapture() },
        { OperationKindEx.InstanceReference, new InstanceReference() },
        { OperationKindEx.LocalReference, new LocalReference() },
        { OperationKindEx.ObjectCreation, new Creation() },
        { OperationKindEx.ParameterReference, new ParameterReference() },
        { OperationKindEx.PropertyReference, new PropertyReference() },
        { OperationKindEx.RecursivePattern, new RecursivePattern() },
        { OperationKindEx.ReDimClause, new ReDimClause() },
        { OperationKindEx.SimpleAssignment, new Assignment() },
        { OperationKindEx.TypeParameterObjectCreation, new Creation() },
        { OperationKindEx.Unary, new Unary() }
    };

    public static IEnumerable<SymbolicContext> Process(SymbolicContext context)
    {
        if (Simple.TryGetValue(context.Operation.Instance.Kind, out var simple))            // Operations that return single state
        {
            context = context.WithState(simple.Process(context));
        }
        return Branching.TryGetValue(context.Operation.Instance.Kind, out var processor)    // Operations that can return multiple states
            ? processor.Process(context).Select(x => context.WithState(x))
            : new[] { context };
    }
}
