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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class NullPointerDereferenceBase : SymbolicRuleCheck
{
    internal const string DiagnosticId = "S2259";

    protected virtual bool IsSupressed(SyntaxNode node) => false;

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        if (NullDereferenceCandidate(context.Operation.Instance) is { } reference
            && context.State.HasConstraint(reference, ObjectConstraint.Null)
            && !IsSupressed(reference.Syntax)
            && SemanticModel.GetTypeInfo(reference.Syntax).Nullability().FlowState != NullableFlowState.NotNull)
        {
            ReportIssue(reference, reference.Syntax.ToString());
        }
        return context.State;
    }

    private static IOperation NullDereferenceCandidate(IOperation operation) =>
        operation.Kind switch
        {
            OperationKindEx.Invocation => NullInstanceCandidate(operation.ToInvocation()),
            OperationKindEx.FieldReference => operation.ToFieldReference().Instance,
            OperationKindEx.PropertyReference => NullInstanceCandidate(operation.ToPropertyReference()),
            OperationKindEx.EventReference => operation.ToEventReference().Instance,
            OperationKindEx.Await => operation.ToAwait().Operation,
            OperationKindEx.ArrayElementReference => operation.ToArrayElementReference().ArrayReference,
            OperationKindEx.MethodReference => operation.ToMethodReference().Instance,
            _ => null,
        };

    private static IOperation NullInstanceCandidate(IInvocationOperationWrapper operation) =>
        operation.TargetMethod.ContainingType.IsNullableValueType()
        && operation.TargetMethod.Name != nameof(Nullable<int>.GetType) // All methods on Nullable but .GetType() are safe to call
            ? null
            : operation.Instance;

    private static IOperation NullInstanceCandidate(IPropertyReferenceOperationWrapper operation) =>
        operation.Property.ContainingType.IsNullableValueType() // HasValue doesn't throw; Value is covered by S3655
            ? null
            : operation.Instance;
}
