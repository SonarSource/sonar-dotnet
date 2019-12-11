/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
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
            var symbol = this.semanticModel.GetSymbolInfo(this.invocation).Symbol;

            var methodSymbol = symbol as IMethodSymbol;
            var invocationArgsCount = this.invocation.ArgumentList?.Arguments.Count ?? 0;

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
            // ToDo: IsNullOrWhiteSpace is now treated as IsNull. Add the needed string constraints to treat IsNullOrWhiteSpace correctly
            if (IsStringNullOrWhiteSpaceCheckMethod(methodSymbol))
            {
                return HandleStringNullOrWhiteSpaceCheckMethod();
            }

            if (IsStringNullOrEmptyCheckMethod(methodSymbol))
            {
                return HandleStringNullOrEmptyCheckMethod();
            }

            if (methodSymbol != null &&
                ValidatesNotNull(methodSymbol, out var validatedArgumentIndex))
            {
                return HandleNullValidationMethod(validatedArgumentIndex, invocationArgsCount);
            }

            if (this.invocation.IsNameof(this.semanticModel))
            {
                return HandleNameofExpression();
            }

            return this.programState
                .PopValues(invocationArgsCount + 1)
                .PushValue(new SymbolicValue());
        }

        private ProgramState HandleNameofExpression()
        {
            // The nameof arguments are not on the stack, we just push the nameof result
            var nameof = new SymbolicValue();
            var newProgramState = this.programState.PushValue(nameof);
            return newProgramState.SetConstraint(nameof, ObjectConstraint.NotNull);
        }

        private ProgramState HandleStringNullOrWhiteSpaceCheckMethod()
        {
            var newProgramState = this.programState
                .PopValue(out var arg)
                .PopValue(out var isNullOrWhiteSpaceMethod);

            var refEquals = new ReferenceEqualsSymbolicValue(arg, isNullOrWhiteSpaceMethod);
            newProgramState = newProgramState.PushValue(refEquals);
            if (newProgramState.HasConstraint(arg, ObjectConstraint.Null) ||
                    newProgramState.HasConstraint(arg, StringConstraint.EmptyString) ||
                    newProgramState.HasConstraint(arg, StringConstraint.WhiteSpaceString))
            {
                newProgramState = newProgramState.SetConstraint(refEquals, BoolConstraint.True);
            }
            else if (newProgramState.HasConstraint(arg, StringConstraint.FullNotWhiteSpaceString))
            {
                newProgramState = newProgramState.SetConstraint(refEquals, BoolConstraint.False);
            }
            // This is reached when we don't know the result of isNullOrWhiteSpaceMethod
            // for example when there is a FullOrNullString constraint
            return newProgramState;
        }

        private ProgramState HandleStringNullOrEmptyCheckMethod()
        {
            var newProgramState = this.programState
                .PopValue(out var arg)
                .PopValue(out var isNullOrEmptySpaceMethod);

            var refEquals = new ReferenceEqualsSymbolicValue(arg, isNullOrEmptySpaceMethod);
            newProgramState = newProgramState.PushValue(refEquals);
            if (newProgramState.HasConstraint(arg, ObjectConstraint.Null) ||
                    newProgramState.HasConstraint(arg, StringConstraint.EmptyString))
            {
                newProgramState= newProgramState.SetConstraint(refEquals, BoolConstraint.True);
            }
            else if(newProgramState.HasConstraint(arg, StringConstraint.FullString) ||
                        newProgramState.HasConstraint(arg, StringConstraint.WhiteSpaceString) ||
                        newProgramState.HasConstraint(arg, StringConstraint.FullNotWhiteSpaceString))
            {
                newProgramState = newProgramState.SetConstraint(refEquals, BoolConstraint.False);
            }
            return newProgramState;
        }

        private ProgramState HandleStaticEqualsCall()
        {
            var newProgramState = this.programState
                .PopValue(out var arg1)
                .PopValue(out var arg2)
                .PopValue();

            var equals = new ValueEqualsSymbolicValue(arg1, arg2);
            newProgramState = newProgramState.PushValue(equals);
            return SetConstraintOnValueEquals(equals, newProgramState);
        }

        private ProgramState HandleReferenceEqualsCall()
        {
            var newProgramState = this.programState
                .PopValue(out var arg1)
                .PopValue(out var arg2)
                .PopValue();

            return new ReferenceEqualsConstraintHandler(arg1, arg2,
                    this.invocation.ArgumentList.Arguments[0].Expression,
                    this.invocation.ArgumentList.Arguments[1].Expression,
                    newProgramState, this.semanticModel)
                .PushWithConstraint();
        }

        private ProgramState HandleInstanceEqualsCall()
        {
            var newProgramState = this.programState
                .PopValue(out var arg1)
                .PopValue(out var expression);


            var arg2 = expression is MemberAccessSymbolicValue memberAccess ? memberAccess.MemberExpression
                : SymbolicValue.This;

            var equals = new ValueEqualsSymbolicValue(arg1, arg2);
            newProgramState = newProgramState.PushValue(equals);
            return SetConstraintOnValueEquals(equals, newProgramState);
        }

        private ProgramState HandleNullValidationMethod(int validatedArgumentIndex, int invocationArgsCount) =>
            this.programState
                .PopValues(invocationArgsCount - validatedArgumentIndex - 1)
                .PopValue(out var guardedArgumentValue)
                .PopValues(validatedArgumentIndex)
                .SetConstraint(guardedArgumentValue, ObjectConstraint.NotNull);

        private static bool IsStringStaticMethod(IMethodSymbol methodSymbol, string methodName) =>
             methodSymbol != null &&
             methodSymbol.ContainingType.Is(KnownType.System_String) &&
             methodSymbol.IsStatic &&
             methodName == methodSymbol.Name;


        private static bool IsStringNullOrWhiteSpaceCheckMethod(IMethodSymbol methodSymbol) =>
            IsStringStaticMethod(methodSymbol, nameof(string.IsNullOrWhiteSpace));
        

     
        private static bool IsStringNullOrEmptyCheckMethod(IMethodSymbol methodSymbol) =>
            IsStringStaticMethod(methodSymbol, nameof(string.IsNullOrEmpty));


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

        private static bool ValidatesNotNull(IMethodSymbol methodSymbol, out int validatedArgumentIndex)
        {
            validatedArgumentIndex = methodSymbol.Parameters.IndexOf(IsValidatedParameter);

            return validatedArgumentIndex >= 0;

            bool IsValidatedParameter(IParameterSymbol parameterSymbol) =>
                parameterSymbol.GetAttributes().Any(IsValidatedNotNullAttribute);

            bool IsValidatedNotNullAttribute(AttributeData attribute) =>
                "ValidatedNotNullAttribute".Equals(attribute.AttributeClass?.Name);
        }

        internal static ProgramState SetConstraintOnValueEquals(ValueEqualsSymbolicValue equals,
            ProgramState programState)
        {
            if (equals.LeftOperand == equals.RightOperand)
            {
                return programState.SetConstraint(equals, BoolConstraint.True);
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
                var refEquals = new ReferenceEqualsSymbolicValue(this.valueLeft, this.valueRight);
                var newProgramState = this.programState.PushValue(refEquals);
                return SetConstraint(refEquals, newProgramState);
            }

            private ProgramState SetConstraint(ReferenceEqualsSymbolicValue refEquals, ProgramState programState)
            {
                if (AreBothArgumentsNull())
                {
                    return programState.SetConstraint(refEquals, BoolConstraint.True);
                }

                if (IsAnyArgumentNonNullValueType() ||
                    ArgumentsHaveDifferentNullability())
                {
                    return programState.SetConstraint(refEquals, BoolConstraint.False);
                }

                if (this.valueLeft == this.valueRight)
                {
                    return programState.SetConstraint(refEquals, BoolConstraint.True);
                }

                return programState;
            }

            private bool ArgumentsHaveDifferentNullability()
            {
                return (this.programState.HasConstraint(this.valueLeft, ObjectConstraint.Null) &&
                    this.programState.HasConstraint(this.valueRight, ObjectConstraint.NotNull))
                    ||
                    (this.programState.HasConstraint(this.valueLeft, ObjectConstraint.NotNull) &&
                    this.programState.HasConstraint(this.valueRight, ObjectConstraint.Null));
            }

            private bool IsAnyArgumentNonNullValueType()
            {
                if (this.expressionRight == null ||
                    this.expressionLeft == null)
                {
                    return false;
                }

                var type1 = this.semanticModel.GetTypeInfo(this.expressionLeft).Type;
                var type2 = this.semanticModel.GetTypeInfo(this.expressionRight).Type;

                if (type1 == null ||
                    type2 == null)
                {
                    return false;
                }

                return IsValueNotNull(this.valueLeft, type1, this.programState) ||
                    IsValueNotNull(this.valueRight, type2, this.programState);
            }

            private static bool IsValueNotNull(SymbolicValue arg, ITypeSymbol type, ProgramState programState)
            {
                return programState.HasConstraint(arg, ObjectConstraint.NotNull) &&
                    type.IsValueType;
            }

            private bool AreBothArgumentsNull()
            {
                return this.programState.HasConstraint(this.valueLeft, ObjectConstraint.Null) &&
                    this.programState.HasConstraint(this.valueRight, ObjectConstraint.Null);
            }
        }
    }
}
