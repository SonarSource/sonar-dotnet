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

public abstract class EmptyNullableValueAccessBase : SymbolicRuleCheck
{
    public const string DiagnosticId = "S3655";

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var operationInstance = context.Operation.Instance;
        if (operationInstance.AsPropertyReference() is { } reference
            && reference.Property.Name == nameof(Nullable<int>.Value)
            && reference.Instance is { } instance
            && instance.Type.IsNullableValueType()
            && context.State.HasConstraint(instance, ObjectConstraint.Null)
            && FlowState(reference.Instance) != NullableFlowState.NotNull)
        {
            ReportIssue(instance, instance.Syntax.ToString());
        }
        else if (operationInstance.AsConversion() is { } conversion
            && conversion.Operand.Type.IsNullableValueType()
            && conversion.Type.IsNonNullableValueType()
            && context.State.HasConstraint(conversion.Operand, ObjectConstraint.Null)
            && FlowState(conversion.Operand) != NullableFlowState.NotNull)
        {
            ReportIssue(conversion.Operand, conversion.Operand.Syntax.ToString());
        }

        return context.State;

        NullableFlowState FlowState(IOperation reference) =>
            SemanticModel.GetTypeInfo(reference.Syntax).Nullability().FlowState;
    }
}
