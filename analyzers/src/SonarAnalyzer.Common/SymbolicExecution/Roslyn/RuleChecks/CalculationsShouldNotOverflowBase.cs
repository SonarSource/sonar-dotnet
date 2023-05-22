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

using System.Numerics;
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
        if (OverflowCandidateValue(context.State, operation)?.Constraint<NumberConstraint>() is { } number
            && Min(operation.Type) is { } typeMin
            && Max(operation.Type) is { } typeMax)
        {
            if (number.Max < typeMin)
            {
                ReportIssue(operation, MessageGuaranteed, MessageUnderflow, typeMin);
            }
            else if (number.Min > typeMax)
            {
                ReportIssue(operation, MessageGuaranteed, MessageOverflow, typeMax);
            }
            else if (number.Max > typeMax)
            {
                ReportIssue(operation, MessageLikely, MessageOverflow, typeMax);
            }
            else if (number.Min < typeMin)
            {
                ReportIssue(operation, MessageLikely, MessageUnderflow, typeMin);
            }
        }
        return context.State;
    }

    private static SymbolicValue OverflowCandidateValue(ProgramState state, IOperation operation) =>
        operation.Kind switch
        {
            OperationKindEx.Binary when CanOverflow(operation.ToBinary().OperatorKind) => state[operation],
            OperationKindEx.CompoundAssignment when CanOverflow(operation.ToCompoundAssignment().OperatorKind) => state[operation],
            OperationKindEx.Increment or OperationKindEx.Decrement when operation.ToIncrementOrDecrement().Target.TrackedSymbol() is { } symbol => state[symbol],
            _ => null
        };

    private static bool CanOverflow(BinaryOperatorKind kind) =>
        kind is BinaryOperatorKind.Add or BinaryOperatorKind.Subtract or BinaryOperatorKind.Multiply or BinaryOperatorKind.Power;

    private static BigInteger? Min(ITypeSymbol type) =>
        type switch
        {
            _ when type.Is(KnownType.System_SByte) => sbyte.MinValue,
            _ when type.Is(KnownType.System_Byte) => byte.MinValue,
            _ when type.Is(KnownType.System_Int16) => short.MinValue,
            _ when type.Is(KnownType.System_UInt16) => ushort.MinValue,
            _ when type.Is(KnownType.System_Int32) => int.MinValue,
            _ when type.Is(KnownType.System_UInt32) => uint.MinValue,
            _ when type.Is(KnownType.System_Int64) => long.MinValue,
            _ when type.Is(KnownType.System_UInt64) => ulong.MinValue,
            _ => null
        };

    private static BigInteger? Max(ITypeSymbol type)
        => type switch
        {
            _ when type.Is(KnownType.System_SByte) => sbyte.MaxValue,
            _ when type.Is(KnownType.System_Byte) => byte.MaxValue,
            _ when type.Is(KnownType.System_Int16) => short.MaxValue,
            _ when type.Is(KnownType.System_UInt16) => ushort.MaxValue,
            _ when type.Is(KnownType.System_Int32) => int.MaxValue,
            _ when type.Is(KnownType.System_UInt32) => uint.MaxValue,
            _ when type.Is(KnownType.System_Int64) => long.MaxValue,
            _ when type.Is(KnownType.System_UInt64) => ulong.MaxValue,
            _ => null
        };
}
