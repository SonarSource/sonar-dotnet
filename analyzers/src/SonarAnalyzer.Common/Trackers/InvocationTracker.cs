/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers.Trackers
{
    public abstract class InvocationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, InvocationContext>
        where TSyntaxKind : struct
    {
        public abstract Condition ArgumentAtIndexIsConstant(int index);
        public abstract Condition ArgumentAtIndexIsAny(int index, params string[] values);
        public abstract Condition MatchProperty(MemberDescriptor member);
        internal abstract object ConstArgumentForParameter(InvocationContext context, string parameterName);
        protected abstract SyntaxToken? ExpectedExpressionIdentifier(SyntaxNode expression);

        public Condition MatchMethod(params MemberDescriptor[] methods) =>
           new Condition(context => MemberDescriptor.MatchesAny(context.MethodName, context.MethodSymbol, true, Language.NameComparison, methods));

        public Condition MethodNameIs(string methodName) =>
            new Condition(context => context.MethodName == methodName);

        public Condition MethodIsStatic() =>
            new Condition(context => context.MethodSymbol.Value != null
                       && context.MethodSymbol.Value.IsStatic);

        public Condition MethodIsExtension() =>
            new Condition(context => context.MethodSymbol.Value != null
                       && context.MethodSymbol.Value.IsExtensionMethod);

        public Condition MethodHasParameters(int count) =>
            new Condition(context => context.MethodSymbol.Value != null
                       && context.MethodSymbol.Value.Parameters.Length == count);

        public Condition IsInvalidBuilderInitialization<TInvocationSyntax>(BuilderPatternCondition<TSyntaxKind, TInvocationSyntax> condition) where TInvocationSyntax : SyntaxNode =>
            new Condition(condition.IsInvalidBuilderInitialization);

        public Condition ExceptWhen(Condition condition) => !condition;

        public Condition And(Condition condition1, Condition condition2) => condition1 & condition2;

        public Condition Or(Condition condition1, Condition condition2) => condition1 | condition2;

        public Condition Or(Condition condition1, Condition condition2, Condition condition3) =>
            condition1 | condition2 | condition3;

        internal Condition MethodReturnTypeIs(KnownType returnType) =>
            new Condition(context => context.MethodSymbol.Value != null
                       && context.MethodSymbol.Value.ReturnType.DerivesFrom(returnType));

        internal Condition ArgumentIsBoolConstant(string parameterName, bool expectedValue) =>
            new Condition(context => ConstArgumentForParameter(context, parameterName) is bool boolValue
                       && boolValue == expectedValue);

        internal Condition IsIHeadersDictionary() =>
           new Condition(context =>
            {
                const int argumentsNumber = 2;

                var containingType = context.MethodSymbol.Value.ContainingType;

                return containingType.TypeArguments.Length == argumentsNumber
                       && containingType.TypeArguments[0].Is(KnownType.System_String)
                       && containingType.TypeArguments[1].Is(KnownType.Microsoft_Extensions_Primitives_StringValues);
            });

        protected override InvocationContext CreateContext(SyntaxNodeAnalysisContext context) =>
            Language.Syntax.NodeExpression(context.Node) is { } expression
            && ExpectedExpressionIdentifier(expression) is { } identifier
                ? new InvocationContext(context, identifier.ValueText) : null;
    }
}
