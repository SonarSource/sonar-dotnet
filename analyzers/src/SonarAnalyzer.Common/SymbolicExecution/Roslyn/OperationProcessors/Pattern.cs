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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal class Pattern : BranchingProcessor<IIsPatternOperationWrapper>
{
    protected override Func<IOperation, IIsPatternOperationWrapper> Convert => IIsPatternOperationWrapper.FromOperation;

    public static ProgramState Process(SymbolicContext context, IRecursivePatternOperationWrapper recursive) =>
        ProcessDeclaration(context, recursive.DeclaredSymbol, true);

    public static ProgramState Process(SymbolicContext context, IDeclarationPatternOperationWrapper declaration) =>
        ProcessDeclaration(context, declaration.DeclaredSymbol, !declaration.MatchesNull);  // "... is var ..." should not set NotNull

    protected override SymbolicConstraint BoolConstraintFromOperation(SymbolicContext context, IIsPatternOperationWrapper operation) =>
        BoolContraintFromConstant(context.State, operation) ?? BoolConstraintFromPattern(context.State, operation);

    protected override ProgramState LearnBranchingConstraint(ProgramState state, IIsPatternOperationWrapper operation, bool falseBranch) =>
        state.ResolveCapture(operation.Value).TrackedSymbol() is { } testedSymbol
        && LearnBranchingConstraint(state, operation.Pattern, falseBranch) is { } constraint
            ? state.SetSymbolConstraint(testedSymbol, constraint)
            : state;

    private static SymbolicConstraint LearnBranchingConstraint(ProgramState state, IPatternOperationWrapper pattern, bool falseBranch)
    {
        return pattern.WrappedOperation.Kind switch
        {
            OperationKindEx.ConstantPattern => ConstraintFromConstantPattern(state, As(IConstantPatternOperationWrapper.FromOperation), falseBranch, pattern.InputType.IsReferenceType),
            OperationKindEx.DeclarationPattern => ConstraintFromDeclarationPattern(As(IDeclarationPatternOperationWrapper.FromOperation), falseBranch),
            OperationKindEx.NegatedPattern => LearnBranchingConstraint(state, As(INegatedPatternOperationWrapper.FromOperation).Pattern, !falseBranch),
            OperationKindEx.RecursivePattern => ConstraintFromRecursivePattern(As(IRecursivePatternOperationWrapper.FromOperation), falseBranch),
            OperationKindEx.TypePattern => ObjectConstraint.NotNull.ApplyOpposite(falseBranch),
            _ => null
        };

        T As<T>(Func<IOperation, T> fromOperation) =>
            fromOperation(pattern.WrappedOperation);
    }

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

    private static ObjectConstraint ConstraintFromRecursivePattern(IRecursivePatternOperationWrapper recursive, bool falseBranch) =>
        recursive.InputType.IsReferenceType
            ? falseBranch switch
            {
                true => RecursivePatternAlwaysMatchesAnyNotNull(recursive) ? ObjectConstraint.Null : null,
                _ => ObjectConstraint.NotNull
            }
            : null;

    private static SymbolicConstraint ConstraintFromDeclarationPattern(IDeclarationPatternOperationWrapper declaration, bool falseBranch)
    {
        if (declaration.MatchesNull || !declaration.InputType.IsReferenceType)
        {
            return null;
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

    private static ProgramState ProcessDeclaration(SymbolicContext context, ISymbol declaredSymbol, bool setNotNull)
    {
        if (declaredSymbol == null)
        {
            return context.State;
        }
        else
        {
            var state = context.Operation.Parent.AsIsPattern() is { } parentIsPattern && parentIsPattern.Value.TrackedSymbol() is { } sourceSymbol
                ? context.State.SetSymbolValue(declaredSymbol, context.State[sourceSymbol])  // ToDo: MMF-2563 should define relation between tested and declared symbol
                : context.State;
            return setNotNull
                ? state.SetSymbolConstraint(declaredSymbol, ObjectConstraint.NotNull)
                : state;
        }
    }

    private static BoolConstraint BoolContraintFromConstant(ProgramState state, IIsPatternOperationWrapper isPattern) =>
        state[isPattern.Value] is { } value
        && isPattern.Pattern.WrappedOperation.Kind == OperationKindEx.ConstantPattern
        && value.Constraint<BoolConstraint>() is { } valueConstraint
        && IConstantPatternOperationWrapper.FromOperation(isPattern.Pattern.WrappedOperation).Value.ConstantValue.Value is bool boolConstant
            ? BoolConstraint.From(valueConstraint == BoolConstraint.From(boolConstant))
            : null; // We cannot take conclusive decision

    private static SymbolicConstraint BoolConstraintFromPattern(ProgramState state, IIsPatternOperationWrapper isPattern) =>
        state[isPattern.Value] is { } value
        && value.Constraint<ObjectConstraint>() is { } valueConstraint
        && BoolConstraintFromPattern(state, valueConstraint, isPattern.Pattern) is { } newConstraint
            ? newConstraint
            : null;

    private static SymbolicConstraint BoolConstraintFromPattern(ProgramState state, ObjectConstraint valueConstraint, IPatternOperationWrapper pattern)
    {
        return pattern.WrappedOperation.Kind switch
        {
            OperationKindEx.ConstantPattern when state[As(IConstantPatternOperationWrapper.FromOperation).Value]?.HasConstraint(ObjectConstraint.Null) is true =>
                BoolConstraint.From(valueConstraint == ObjectConstraint.Null),
            OperationKindEx.RecursivePattern => BoolConstraintFromRecursivePattern(valueConstraint, As(IRecursivePatternOperationWrapper.FromOperation)),
            OperationKindEx.DeclarationPattern => BoolConstraintFromDeclarationPattern(valueConstraint, As(IDeclarationPatternOperationWrapper.FromOperation)),
            OperationKindEx.TypePattern when
                As(ITypePatternOperationWrapper.FromOperation) is var type
                && type.InputType.DerivesOrImplements(type.NarrowedType) => BoolConstraint.From(valueConstraint == ObjectConstraint.NotNull),
            OperationKindEx.NegatedPattern => BoolConstraintFromPattern(state, valueConstraint, As(INegatedPatternOperationWrapper.FromOperation).Pattern)?.Opposite,
            OperationKindEx.DiscardPattern => BoolConstraint.True,
            OperationKindEx.BinaryPattern => BoolConstraintFromBinaryPattern(state, valueConstraint, As(IBinaryPatternOperationWrapper.FromOperation)),
            _ => null,
        };

        T As<T>(Func<IOperation, T> fromOperation) =>
             fromOperation(pattern.WrappedOperation);
    }

    private static BoolConstraint BoolConstraintFromDeclarationPattern(ObjectConstraint valueConstraint, IDeclarationPatternOperationWrapper declaration) =>
        declaration switch
        {
            { MatchesNull: true } => BoolConstraint.True,
            _ when valueConstraint == ObjectConstraint.Null => BoolConstraint.False,
            _ when declaration.InputType.DerivesOrImplements(declaration.NarrowedType) => BoolConstraint.From(valueConstraint == ObjectConstraint.NotNull),
            _ => null,
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

    private static BoolConstraint BoolConstraintFromBinaryPattern(ProgramState state, ObjectConstraint valueConstraint, IBinaryPatternOperationWrapper binaryPattern)
    {
        var left = BoolConstraintFromPattern(state, valueConstraint, binaryPattern.LeftPattern);
        var right = BoolConstraintFromPattern(state, valueConstraint, binaryPattern.RightPattern);
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
}
