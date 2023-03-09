/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
        if (ReferenceOrDefault(context.Operation.Instance) is { } reference
            && context.HasConstraint(reference, ObjectConstraint.Null)
            && !IsSupressed(reference.Syntax)
            && SemanticModel.GetTypeInfo(reference.Syntax).Nullability().FlowState != NullableFlowState.NotNull)
        {
            ReportIssue(reference, reference.Syntax.ToString());
        }
        return context.State;
    }

    private static IOperation ReferenceOrDefault(IOperation instance) =>
        instance.Kind switch
        {
            OperationKindEx.Invocation => InvocationOrDefault(instance),
            OperationKindEx.PropertyReference => PropertyReferenceOrDefault(instance),
            OperationKindEx.Await => instance.ToAwait().Operation,
            OperationKindEx.ArrayElementReference => instance.ToArrayElementReference().ArrayReference,
            _ => null,
        };

    private static IOperation InvocationOrDefault(IOperation instance) =>
        instance.ToInvocation() is { TargetMethod: var method } invocation
        && !method.IsAny(KnownType.System_Nullable_T, nameof(Nullable<int>.GetValueOrDefault), nameof(Nullable<int>.Equals), nameof(Nullable<int>.ToString), nameof(Nullable<int>.GetHashCode))     // GetType raises, since it throws NRE
            ? invocation.Instance
            : null;

    private static IOperation PropertyReferenceOrDefault(IOperation instance) =>
        instance.ToPropertyReference() is { Property: var property } propertyReference
        && !(property.IsInType(KnownType.System_Nullable_T) && property.Name is nameof(Nullable<int>.HasValue) or nameof(Nullable<int>.Value))  // HasValue doesn't throw; Value is covered by S3655
            ? propertyReference.Instance
            : null;
}
