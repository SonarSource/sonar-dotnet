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

public abstract class CalculationsShouldNotOverflowBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S3949";
    protected const string MessageFormat = "This calculation is {0} to {1} value of '{2}'.";
    private const string MessageLikely = "likely";
    private const string MessageGuaranteed = "guaranteed";
    private const string MessageUnderflow = "underflow the minimum";
    private const string MessageOverflow = "overflow the maximum";

    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (OverflowCandidateValue(context.State, operation)?.Constraint<NumberConstraint>() is { } number && Bounds(operation.Type) is { } bounds)
        {
            if (number.Max < bounds.Min)
            {
                ReportIssue(operation, MessageGuaranteed, MessageUnderflow, bounds.Min.ToString());
            }
            else if (number.Min > bounds.Max)
            {
                ReportIssue(operation, MessageGuaranteed, MessageOverflow, bounds.Max.ToString());
            }
            else if (number.Max > bounds.Max)
            {
                ReportIssue(operation, MessageLikely, MessageOverflow, bounds.Max.ToString());
            }
            else if (number.Min < bounds.Min)
            {
                ReportIssue(operation, MessageLikely, MessageUnderflow, bounds.Min.ToString());
            }
        }
        return context.State;
    }

    private static SymbolicValue OverflowCandidateValue(ProgramState state, IOperation operation) =>
        operation.Kind switch
        {
            OperationKindEx.Binary when CanOverflow(operation.ToBinary().OperatorKind) => state[operation],
            OperationKindEx.CompoundAssignment when CanOverflow(operation.ToCompoundAssignment().OperatorKind) => state[operation],
            OperationKindEx.Increment or OperationKindEx.Decrement when operation.ToIncrementOrDecrement().Target.TrackedSymbol(state) is { } symbol => state[symbol],
            _ => null
        };

    private static bool CanOverflow(BinaryOperatorKind kind) =>
        kind is BinaryOperatorKind.Add or BinaryOperatorKind.Subtract or BinaryOperatorKind.Multiply;

    private static NumberConstraint Bounds(ITypeSymbol type) =>
        type switch
        {
            _ when type.Is(KnownType.System_SByte) => NumberConstraint.From(sbyte.MinValue, sbyte.MaxValue),
            _ when type.Is(KnownType.System_Byte) => NumberConstraint.From(byte.MinValue, byte.MaxValue),
            _ when type.Is(KnownType.System_Int16) => NumberConstraint.From(short.MinValue, short.MaxValue),
            _ when type.Is(KnownType.System_UInt16) => NumberConstraint.From(ushort.MinValue, ushort.MaxValue),
            _ when type.Is(KnownType.System_Int32) => NumberConstraint.From(int.MinValue, int.MaxValue),
            _ when type.Is(KnownType.System_UInt32) => NumberConstraint.From(uint.MinValue, uint.MaxValue),
            _ when type.Is(KnownType.System_Int64) => NumberConstraint.From(long.MinValue, long.MaxValue),
            _ when type.Is(KnownType.System_UInt64) => NumberConstraint.From(ulong.MinValue, ulong.MaxValue),
            _ => null
        };
}
