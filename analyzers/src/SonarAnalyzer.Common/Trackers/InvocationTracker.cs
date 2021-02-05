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

namespace SonarAnalyzer.Helpers
{
    public abstract class InvocationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, InvocationContext>
        where TSyntaxKind : struct
    {
        public abstract Condition ArgumentAtIndexIsConstant(int index);
        public abstract Condition ArgumentAtIndexIsAny(int index, params string[] values);
        public abstract Condition MatchProperty(MemberDescriptor member);
        internal abstract object ConstArgumentForParameter(InvocationContext context, string parameterName);

        public Condition MatchMethod(params MemberDescriptor[] methods) =>
           context => MemberDescriptor.MatchesAny(context.MethodName, context.MethodSymbol, true, Language.NameComparison, methods);

        public Condition MethodNameIs(string methodName) =>
            context => context.MethodName == methodName;

        public Condition MethodIsStatic() =>
            context => context.MethodSymbol.Value != null
                       && context.MethodSymbol.Value.IsStatic;

        public Condition MethodIsExtension() =>
            context => context.MethodSymbol.Value != null
                       && context.MethodSymbol.Value.IsExtensionMethod;

        public Condition MethodHasParameters(int count) =>
            context => context.MethodSymbol.Value != null
                       && context.MethodSymbol.Value.Parameters.Length == count;

        public Condition IsInvalidBuilderInitialization<TInvocationSyntax>(BuilderPatternCondition<TInvocationSyntax> condition) where TInvocationSyntax : SyntaxNode =>
            condition.IsInvalidBuilderInitialization;

        internal Condition MethodReturnTypeIs(KnownType returnType) =>
            context => context.MethodSymbol.Value != null
                       && context.MethodSymbol.Value.ReturnType.DerivesFrom(returnType);

        internal Condition ArgumentIsBoolConstant(string parameterName, bool expectedValue) =>
            context => ConstArgumentForParameter(context, parameterName) is bool boolValue
                       && boolValue == expectedValue;

        protected override InvocationContext CreateContext(SyntaxNodeAnalysisContext context) =>
            Language.Syntax.NodeExpression(context.Node) is { } expression
            && Language.Syntax.NodeIdentifier(expression) is { } identifier
                ? new InvocationContext(context, identifier.ValueText) : null;
    }
}
