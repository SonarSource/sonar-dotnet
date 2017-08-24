/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution
{
    internal class InvocationVisitor
    {
        private const string EqualsLiteral = "Equals";
        private const string ReferenceEqualsLiteral = "ReferenceEquals";

        private readonly InvocationExpressionSyntax invocation;
        private readonly SemanticModel semanticModel;
        private readonly ProgramState programState;

        public InvocationVisitor(InvocationExpressionSyntax invocation, SemanticModel semanticModel,
            ProgramState programState)
        {
            this.invocation = invocation;
            this.semanticModel = semanticModel;
            this.programState = programState;
        }

        internal ProgramState ProcessInvocation()
        {
            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;

            var methodSymbol = symbol as IMethodSymbol;
            var invocationArgsCount = invocation.ArgumentList?.Arguments.Count ?? 0;

            if (IsInstanceEqualsCall(methodSymbol))
            {
                return HandleInstanceEqualsCall();
            }

            if (IsStaticEqualsCall(methodSymbol))
            {
                return HandleStaticEqualsCall();
            }

            if (IsReferenceEqualsCall(methodSymbol))
            {
                return HandleReferenceEqualsCall();
            }

            if (IsStringNullCheckMethod(methodSymbol))
            {
                return HandleStringNullCheckMethod();
            }

            if (invocation.IsNameof(semanticModel))
            {
                return HandleNameofExpression(invocationArgsCount);
            }

            return programState
                .PopValues(invocationArgsCount + 1)
                .PushValue(new SymbolicValue());
        }

        private ProgramState HandleNameofExpression(int argumentsCount)
        {
            // We want to handle the correct nameof(a) but also malformed ones likes nameof(), nameof(a, b)...
            // So we pop number of arguments + the nameof itself
            var newProgramState = programState
                .PopValues(argumentsCount)
                .PopValue();

            var nameof = new SymbolicValue();
            newProgramState = newProgramState.PushValue(nameof);
            return nameof.SetConstraint(ObjectConstraint.NotNull, newProgramState);
        }

        private ProgramState HandleStringNullCheckMethod()
        {
            SymbolicValue arg1;

            var newProgramState = programState
                .PopValue(out arg1)
                .PopValue();

            if (arg1.HasConstraint(ObjectConstraint.Null, newProgramState))
            {
                // Value is null, so the result of the call is true
                return newProgramState.PushValue(SymbolicValue.True);
            }

            return newProgramState.PushValue(new SymbolicValue());
        }

        private ProgramState HandleStaticEqualsCall()
        {
            SymbolicValue arg1;
            SymbolicValue arg2;

            var newProgramState = programState
                .PopValue(out arg1)
                .PopValue(out arg2)
                .PopValue();

            var equals = new ValueEqualsSymbolicValue(arg1, arg2);
            newProgramState = newProgramState.PushValue(equals);
            return SetConstraintOnValueEquals(equals, newProgramState);
        }

        private ProgramState HandleReferenceEqualsCall()
        {
            SymbolicValue arg1;
            SymbolicValue arg2;

            var newProgramState = programState
                .PopValue(out arg1)
                .PopValue(out arg2)
                .PopValue();

            return new ReferenceEqualsConstraintHandler(arg1, arg2,
                    invocation.ArgumentList.Arguments[0].Expression,
                    invocation.ArgumentList.Arguments[1].Expression,
                    newProgramState, semanticModel)
                .PushWithConstraint();
        }

        private ProgramState HandleInstanceEqualsCall()
        {
            SymbolicValue arg1;
            SymbolicValue expression;

            var newProgramState = programState
                .PopValue(out arg1)
                .PopValue(out expression);

            var memberAccess = expression as MemberAccessSymbolicValue;

            SymbolicValue arg2 = memberAccess != null
                ? memberAccess.MemberExpression
                : SymbolicValue.This;

            var equals = new ValueEqualsSymbolicValue(arg1, arg2);
            newProgramState = newProgramState.PushValue(equals);
            return SetConstraintOnValueEquals(equals, newProgramState);
        }

        private static readonly ImmutableHashSet<string> IsNullMethodNames = ImmutableHashSet.Create(
            nameof(string.IsNullOrEmpty),
            nameof(string.IsNullOrWhiteSpace));

        private static bool IsStringNullCheckMethod(IMethodSymbol methodSymbol)
        {
            return methodSymbol != null &&
                methodSymbol.ContainingType.Is(KnownType.System_String) &&
                methodSymbol.IsStatic &&
                IsNullMethodNames.Contains(methodSymbol.Name);
        }

        private static bool IsReferenceEqualsCall(IMethodSymbol methodSymbol)
        {
            return methodSymbol != null &&
                methodSymbol.ContainingType.Is(KnownType.System_Object) &&
                methodSymbol.Name == ReferenceEqualsLiteral;
        }

        private static bool IsInstanceEqualsCall(IMethodSymbol methodSymbol)
        {
            return methodSymbol != null &&
                methodSymbol.Name == EqualsLiteral &&
                !methodSymbol.IsStatic &&
                methodSymbol.Parameters.Length == 1;
        }

        private static bool IsStaticEqualsCall(IMethodSymbol methodSymbol)
        {
            return methodSymbol != null &&
                methodSymbol.ContainingType.Is(KnownType.System_Object) &&
                methodSymbol.IsStatic &&
                methodSymbol.Name == EqualsLiteral;
        }

        internal static ProgramState SetConstraintOnValueEquals(ValueEqualsSymbolicValue equals,
            ProgramState programState)
        {
            if (equals.LeftOperand == equals.RightOperand)
            {
                return equals.SetConstraint(BoolConstraint.True, programState);
            }

            return programState;
        }

        internal class ReferenceEqualsConstraintHandler
        {
            private readonly ExpressionSyntax expressionLeft;
            private readonly ExpressionSyntax expressionRight;
            private readonly ProgramState programState;
            private readonly SemanticModel semanticModel;
            private readonly SymbolicValue valueLeft;
            private readonly SymbolicValue valueRight;

            public ReferenceEqualsConstraintHandler(SymbolicValue valueLeft, SymbolicValue valueRight,
                ExpressionSyntax expressionLeft, ExpressionSyntax expressionRight,
                ProgramState programState, SemanticModel semanticModel)
            {
                this.valueLeft = valueLeft;
                this.valueRight = valueRight;
                this.expressionLeft = expressionLeft;
                this.expressionRight = expressionRight;
                this.programState = programState;
                this.semanticModel = semanticModel;
            }

            public ProgramState PushWithConstraint()
            {
                var refEquals = new ReferenceEqualsSymbolicValue(valueLeft, valueRight);
                var newProgramState = programState.PushValue(refEquals);
                return SetConstraint(refEquals, newProgramState);
            }

            private ProgramState SetConstraint(ReferenceEqualsSymbolicValue refEquals, ProgramState programState)
            {
                if (AreBothArgumentsNull())
                {
                    return refEquals.SetConstraint(BoolConstraint.True, programState);
                }

                if (IsAnyArgumentNonNullValueType() ||
                    ArgumentsHaveDifferentNullability())
                {
                    return refEquals.SetConstraint(BoolConstraint.False, programState);
                }

                if (valueLeft == valueRight)
                {
                    return refEquals.SetConstraint(BoolConstraint.True, programState);
                }

                return programState;
            }

            private bool ArgumentsHaveDifferentNullability()
            {
                return (valueLeft.HasConstraint(ObjectConstraint.Null, programState) &&
                    valueRight.HasConstraint(ObjectConstraint.NotNull, programState))
                    ||
                    (valueLeft.HasConstraint(ObjectConstraint.NotNull, programState) &&
                    valueRight.HasConstraint(ObjectConstraint.Null, programState));
            }

            private bool IsAnyArgumentNonNullValueType()
            {
                var type1 = semanticModel.GetTypeInfo(expressionLeft).Type;
                var type2 = semanticModel.GetTypeInfo(expressionRight).Type;

                if (type1 == null ||
                    type2 == null)
                {
                    return false;
                }

                return IsValueNotNull(valueLeft, type1, programState) ||
                    IsValueNotNull(valueRight, type2, programState);
            }

            private static bool IsValueNotNull(SymbolicValue arg, ITypeSymbol type, ProgramState programState)
            {
                return arg.HasConstraint(ObjectConstraint.NotNull, programState) &&
                    type.IsValueType;
            }

            private bool AreBothArgumentsNull()
            {
                return valueLeft.HasConstraint(ObjectConstraint.Null, programState) &&
                    valueRight.HasConstraint(ObjectConstraint.Null, programState);
            }
        }
    }
}
