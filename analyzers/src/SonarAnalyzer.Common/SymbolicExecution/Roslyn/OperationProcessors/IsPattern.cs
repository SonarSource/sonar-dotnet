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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class IsPattern : BranchingProcessor<IIsPatternOperationWrapper>
{
    protected override IIsPatternOperationWrapper Convert(IOperation operation) =>
        IIsPatternOperationWrapper.FromOperation(operation);

    protected override SymbolicConstraint BoolConstraintFromOperation(ProgramState state, IIsPatternOperationWrapper operation) =>
        BoolConstraintFromConstant(state, operation) ?? BoolConstraintFromPattern(state, state[operation.Value], operation.Pattern);

    protected override ProgramState LearnBranchingConstraint(ProgramState state, IIsPatternOperationWrapper operation, bool falseBranch) =>
        operation.Value.TrackedSymbol(state) is { } testedSymbol
        && LearnBranchingConstraint(
            state,
            operation.Pattern,
            falseBranch,
            state[testedSymbol]?.HasConstraint<ObjectConstraint>() is true,
            state.Constraint<NumberConstraint>(testedSymbol))
        is { } constraint
            ? state.SetSymbolConstraint(testedSymbol, constraint)
            : state;

    private static SymbolicConstraint LearnBranchingConstraint(ProgramState state, IPatternOperationWrapper pattern, bool falseBranch, bool hasObjectConstraint, NumberConstraint numberConstraint)
    {
        return pattern.WrappedOperation.Kind switch
        {
            OperationKindEx.ConstantPattern => ConstraintFromConstantPattern(state, As(IConstantPatternOperationWrapper.FromOperation), falseBranch, pattern.InputType.IsReferenceType),
            OperationKindEx.DeclarationPattern => ConstraintFromDeclarationPattern(As(IDeclarationPatternOperationWrapper.FromOperation), falseBranch, hasObjectConstraint),
            OperationKindEx.NegatedPattern => LearnBranchingConstraint(state, As(INegatedPatternOperationWrapper.FromOperation).Pattern, !falseBranch, hasObjectConstraint, numberConstraint),
            OperationKindEx.RecursivePattern => ConstraintFromRecursivePattern(As(IRecursivePatternOperationWrapper.FromOperation), falseBranch, hasObjectConstraint),
            OperationKindEx.TypePattern => ObjectConstraint.NotNull.ApplyOpposite(falseBranch),
            OperationKindEx.RelationalPattern => NumberConstraintFromRelationalPattern(state, As(IRelationalPatternOperationWrapper.FromOperation), falseBranch, numberConstraint),
            _ => null
        };

        T As<T>(Func<IOperation, T> fromOperation) =>
            fromOperation(pattern.WrappedOperation);
    }

    private static NumberConstraint NumberConstraintFromRelationalPattern(ProgramState state, IRelationalPatternOperationWrapper relational, bool falseBranch, NumberConstraint existingNumber) =>
        state.Constraint<NumberConstraint>(relational.Value) is { } comparedNumber
            ? relational.OperatorKind.ApplyOpposite(falseBranch).NumberConstraintFromRelationalOperator(existingNumber, comparedNumber)
            : null;

    private static SymbolicConstraint ConstraintFromConstantPattern(ProgramState state, IConstantPatternOperationWrapper constant, bool falseBranch, bool isReferenceType)
    {
        if (state[constant.Value] is { } value)
        {
            if (value.Constraint<BoolConstraint>() is { } boolConstraint && !falseBranch)   // Cannot use opposite on booleans. If it is not "true", it could be null, false or any other type
            {
                return boolConstraint;
            }
            else if (value.Constraint<ObjectConstraint>() is { } objectConstraint)
            {
                return objectConstraint.ApplyOpposite(falseBranch);
            }
        }
        return isReferenceType ? ObjectConstraint.NotNull.ApplyOpposite(falseBranch) : null;    // "obj is 42" => "obj" is NotNull and obj.ToString() is safe. We don't have this for bool.
    }

    private static ObjectConstraint ConstraintFromRecursivePattern(IRecursivePatternOperationWrapper recursive, bool falseBranch, bool hasObjectConstraint)
    {
        if (hasObjectConstraint)
        {
            return null;    // Don't learn if we already know the answer
        }
        else if (falseBranch)
        {
            return RecursivePatternAlwaysMatchesAnyNotNull(recursive) ? ObjectConstraint.Null : null;
        }
        else
        {
            return ObjectConstraint.NotNull;
        }
    }

    private static SymbolicConstraint ConstraintFromDeclarationPattern(IDeclarationPatternOperationWrapper declaration, bool falseBranch, bool hasObjectConstraint)
    {
        if (declaration.MatchesNull || !declaration.InputType.IsReferenceType || hasObjectConstraint)
        {
            return null;    // Don't learn if we can't, or we already know the answer
        }
        else if (falseBranch && declaration.InputType.DerivesOrImplements(declaration.MatchedType)) // For "str is object o", we're sure that it's Null in "else" branch
        {
            return ObjectConstraint.Null;
        }
        else
        {
            return ObjectConstraint.NotNull.ApplyOpposite(falseBranch);
        }
    }

    private static BoolConstraint BoolConstraintFromConstant(ProgramState state, IIsPatternOperationWrapper isPattern)
    {
        if (state[isPattern.Value] is { } value
            && isPattern.Pattern.WrappedOperation.Kind == OperationKindEx.ConstantPattern
            && IConstantPatternOperationWrapper.FromOperation(isPattern.Pattern.WrappedOperation).Value is var constantPattern
            && constantPattern.ConstantValue.Value is { } constant)
        {
            if (constant is bool boolConstantConstraint)
            {
                if (value.Constraint<BoolConstraint>() is { } valueBoolConstraint)
                {
                    return BoolConstraint.From(valueBoolConstraint == BoolConstraint.From(boolConstantConstraint));
                }
                else if (value.HasConstraint(ObjectConstraint.Null))
                {
                    return BoolConstraint.False;
                }
            }
            else if (state.Constraint<NumberConstraint>(constantPattern) is { } constantNumberConstraint
                && value.Constraint<NumberConstraint>() is { } valueNumberConstraint)
            {
                if (constantNumberConstraint.IsSingleValue && valueNumberConstraint.IsSingleValue)
                {
                    return BoolConstraint.From(constantNumberConstraint.Min == valueNumberConstraint.Min);
                }
                else if (!valueNumberConstraint.Overlaps(constantNumberConstraint))
                {
                    return BoolConstraint.False;
                }
            }
        }
        return null; // We cannot take conclusive decision
    }

    private static SymbolicConstraint BoolConstraintFromPattern(ProgramState state, SymbolicValue value, IPatternOperationWrapper pattern)
    {
        var patternOperation = pattern.WrappedOperation;
        if (patternOperation.Kind is OperationKindEx.DiscardPattern
            || patternOperation.AsDeclarationPattern() is { MatchesNull: true })
        {
            return BoolConstraint.True;
        }
        else if (value?.Constraint<ObjectConstraint>() is { } valueConstraint)
        {
            return patternOperation.Kind switch
            {
                OperationKindEx.ConstantPattern when state[patternOperation.ToConstantPattern().Value]?.HasConstraint(ObjectConstraint.Null) is true =>
                    BoolConstraint.From(valueConstraint == ObjectConstraint.Null),
                OperationKindEx.NegatedPattern => BoolConstraintFromPattern(state, value, patternOperation.ToNegatedPattern().Pattern)?.Opposite,
                OperationKindEx.RecursivePattern => BoolConstraintFromRecursivePattern(valueConstraint, patternOperation.ToRecursivePattern()),
                OperationKindEx.DeclarationPattern => BoolConstraintFromDeclarationPattern(valueConstraint, patternOperation.ToDeclarationPattern()),
                OperationKindEx.TypePattern => BoolConstraintFromTypePattern(valueConstraint, patternOperation.ToTypePattern()),
                OperationKindEx.BinaryPattern => BoolConstraintFromBinaryPattern(state, value, patternOperation.ToBinaryPattern()),
                OperationKindEx.RelationalPattern => BoolConstraintFromRelationalPattern(value, patternOperation.ToRelationalPattern()),
                _ => null
            };
        }
        else
        {
            return null;
        }
    }

    private static BoolConstraint BoolConstraintFromDeclarationPattern(ObjectConstraint valueConstraint, IDeclarationPatternOperationWrapper declaration) =>
        declaration switch
        {
            _ when valueConstraint == ObjectConstraint.Null => BoolConstraint.False,
            _ when declaration.InputType.DerivesOrImplements(declaration.NarrowedType) => BoolConstraint.From(valueConstraint == ObjectConstraint.NotNull),
            _ => null,
        };

    private static BoolConstraint BoolConstraintFromTypePattern(ObjectConstraint valueConstraint, ITypePatternOperationWrapper type) =>
        type switch
        {
            _ when valueConstraint == ObjectConstraint.Null => BoolConstraint.False,
            _ when type.InputType.DerivesOrImplements(type.NarrowedType) => BoolConstraint.From(valueConstraint == ObjectConstraint.NotNull),
            _ => null
        };

    private static BoolConstraint BoolConstraintFromRecursivePattern(ObjectConstraint valueConstraint, IRecursivePatternOperationWrapper recursive) =>
        recursive switch
        {
            _ when valueConstraint == ObjectConstraint.Null => BoolConstraint.False,
            _ when RecursivePatternAlwaysMatchesAnyNotNull(recursive) => BoolConstraint.True,
            _ => null,
        };

    private static bool RecursivePatternAlwaysMatchesAnyNotNull(IRecursivePatternOperationWrapper recursive) =>
        recursive.InputType.DerivesOrImplements(recursive.MatchedType)
        && (recursive is { PropertySubpatterns.Length: 0, DeconstructionSubpatterns.Length: 0 } || SubPatternsAlwaysMatches(recursive));

    private static bool SubPatternsAlwaysMatches(IRecursivePatternOperationWrapper recursivePattern) =>
        recursivePattern.PropertySubpatterns
            .Select(x => IPropertySubpatternOperationWrapper.FromOperation(x).Pattern)
            .Concat(recursivePattern.DeconstructionSubpatterns.Select(x => IPatternOperationWrapper.FromOperation(x)))
            .All(x => x is { WrappedOperation.Kind: OperationKindEx.DiscardPattern }
                || (x.WrappedOperation.Kind == OperationKindEx.DeclarationPattern && IDeclarationPatternOperationWrapper.FromOperation(x.WrappedOperation).MatchesNull));

    private static BoolConstraint BoolConstraintFromBinaryPattern(ProgramState state, SymbolicValue value, IBinaryPatternOperationWrapper binaryPattern)
    {
        var left = BoolConstraintFromPattern(state, value, binaryPattern.LeftPattern);
        var right = BoolConstraintFromPattern(state, value, binaryPattern.RightPattern);
        return binaryPattern.OperatorKind switch
        {
            BinaryOperatorKind.And => CombineAnd(),
            BinaryOperatorKind.Or => CombineOr(),
            _ => null,
        };

        BoolConstraint CombineAnd()
        {
            if (left == BoolConstraint.True && right == BoolConstraint.True)
            {
                return BoolConstraint.True;
            }
            else if (left == BoolConstraint.False || right == BoolConstraint.False)
            {
                return BoolConstraint.False;
            }
            else
            {
                return null;
            }
        }

        BoolConstraint CombineOr()
        {
            if (left == BoolConstraint.True || right == BoolConstraint.True)
            {
                return BoolConstraint.True;
            }
            else if (left == BoolConstraint.False && right == BoolConstraint.False)
            {
                return BoolConstraint.False;
            }
            else
            {
                return null;
            }
        }
    }

    private static SymbolicConstraint BoolConstraintFromRelationalPattern(SymbolicValue value, IRelationalPatternOperationWrapper pattern)
    {
        if (value.HasConstraint(ObjectConstraint.Null))
        {
            return BoolConstraint.False;
        }
        else if (value.Constraint<NumberConstraint>() is { } valueNumber
            && NumberConstraint.From(pattern.Value.ConstantValue.Value) is { } patternNumber)
        {
            return pattern.OperatorKind.BoolConstraintFromNumberConstraints(valueNumber, patternNumber);
        }
        else
        {
            return null;
        }
    }
 }
