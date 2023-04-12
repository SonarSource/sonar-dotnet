/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

internal static class OperationDispatcher
{
    private static readonly Dictionary<OperationKind, IMultiProcessor> Branching = new()
    {
        { OperationKindEx.Binary, new Binary() },
        { OperationKindEx.Invocation, new Invocation() },
        { OperationKindEx.IsNull, new IsNull() },
        { OperationKindEx.IsPattern, new IsPattern() },
        { OperationKindEx.IsType, new IsType() },
        { OperationKindEx.PropertyReference, new PropertyReference() },
        { OperationKindEx.TupleBinary, new TupleBinary() },
    };

    private static readonly Dictionary<OperationKind, ISimpleProcessor> Simple = new()
    {
        { OperationKindEx.Argument, new Argument() },
        { OperationKindEx.ArrayCreation, new ArrayCreation() },
        { OperationKindEx.ArrayElementReference, new ArrayElementReference() },
        { OperationKindEx.AnonymousObjectCreation, new NotNullOperation() },
        { OperationKindEx.Await, new Await() },
        { OperationKindEx.CaughtException, new NotNullOperation() },
        { OperationKindEx.CompoundAssignment, new CompoundAssignment() },
        { OperationKindEx.Conversion, new Conversion() },
        { OperationKindEx.DeclarationPattern, new DeclarationPattern() },
        { OperationKindEx.DeconstructionAssignment, new DeconstructionAssignment() },
        { OperationKindEx.Decrement, new IncrementOrDecrement() },
        { OperationKindEx.DefaultValue, new DefaultValue() },
        { OperationKindEx.DelegateCreation, new NotNullOperation() },
        { OperationKindEx.DynamicObjectCreation, new NotNullOperation() },
        { OperationKindEx.EventReference, new EventReference() },
        { OperationKindEx.FieldReference, new FieldReference() },
        { OperationKindEx.FlowCapture, new FlowCapture() },
        { OperationKindEx.FlowCaptureReference, new FlowCaptureReference() },
        { OperationKindEx.Increment, new IncrementOrDecrement() },
        { OperationKindEx.InstanceReference, new InstanceReference() },
        { OperationKindEx.LocalReference, new LocalReference() },
        { OperationKindEx.MethodReference, new MethodReference() },
        { OperationKindEx.ObjectCreation, new ObjectCreation() },
        { OperationKindEx.ParameterReference, new ParameterReference() },
        { OperationKindEx.RecursivePattern, new RecursivePattern() },
        { OperationKindEx.ReDimClause, new ReDimClause() },
        { OperationKindEx.SimpleAssignment, new Assignment() },
        { OperationKindEx.StaticLocalInitializationSemaphore, new StaticLocalInitializationSemaphore() },
        { OperationKindEx.TypeOf, new NotNullOperation() },
        { OperationKindEx.TypeParameterObjectCreation, new NotNullOperation() },
        { OperationKindEx.Unary, new Unary() }
    };

    public static SymbolicContexts Process(SymbolicContext context)
    {
        if (Simple.TryGetValue(context.Operation.Instance.Kind, out var simple))            // Operations that return single state
        {
            return new(context.WithState(simple.Process(context)));
        }
        else if (Branching.TryGetValue(context.Operation.Instance.Kind, out var processor)) // Operations that can return multiple states
        {
            var states = processor.Process(context);
            var result = new SymbolicContexts();
            foreach (var state in states)
            {
                result += new SymbolicContexts(context.WithState(state));
            }
            return result;
        }
        else
        {
            return new(context);
        }
    }
}
