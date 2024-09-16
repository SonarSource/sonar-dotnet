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

namespace SonarAnalyzer.Helpers.Trackers;

public abstract class InvocationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, InvocationContext>
    where TSyntaxKind : struct
{
    public abstract Condition ArgumentAtIndexIsStringConstant(int index);
    public abstract Condition ArgumentAtIndexIsAny(int index, params string[] values);
    public abstract Condition ArgumentAtIndexIs(int index, Func<SyntaxNode, SemanticModel, bool> predicate);
    public abstract Condition MatchProperty(MemberDescriptor member);
    public abstract object ConstArgumentForParameter(InvocationContext context, string parameterName);
    protected abstract SyntaxToken? ExpectedExpressionIdentifier(SyntaxNode expression);

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

    public Condition IsInvalidBuilderInitialization<TInvocationSyntax>(BuilderPatternCondition<TSyntaxKind, TInvocationSyntax> condition) where TInvocationSyntax : SyntaxNode =>
        condition.IsInvalidBuilderInitialization;

    internal Condition MethodReturnTypeIs(KnownType returnType) =>
        context => context.MethodSymbol.Value != null
                   && context.MethodSymbol.Value.ReturnType.DerivesFrom(returnType);

    internal Condition ArgumentIsBoolConstant(string parameterName, bool expectedValue) =>
        context => ConstArgumentForParameter(context, parameterName) is bool boolValue
                   && boolValue == expectedValue;

    internal Condition IsIHeadersDictionary() =>
        context =>
        {
            const int argumentsNumber = 2;

            var containingType = context.MethodSymbol.Value.ContainingType;

            return containingType.TypeArguments.Length == argumentsNumber
                   && containingType.TypeArguments[0].Is(KnownType.System_String)
                   && containingType.TypeArguments[1].Is(KnownType.Microsoft_Extensions_Primitives_StringValues);
        };

    protected override InvocationContext CreateContext(SonarSyntaxNodeReportingContext context) =>
        Language.Syntax.NodeExpression(context.Node) is { } expression
        && ExpectedExpressionIdentifier(expression) is { } identifier
            ? new InvocationContext(context, identifier.ValueText) : null;
}
