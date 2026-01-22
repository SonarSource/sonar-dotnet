/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Trackers;

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
        context => context.MethodSymbol.Value is { IsStatic: true };

    public Condition MethodIsExtension() =>
        context => context.MethodSymbol.Value is { IsExtensionMethod: true };

    public Condition MethodHasParameters(int count) =>
        context => context.MethodSymbol.Value is { } method
                    && method.Parameters.Length == count;

    public Condition IsInvalidBuilderInitialization<TInvocationSyntax>(BuilderPatternCondition<TSyntaxKind, TInvocationSyntax> condition) where TInvocationSyntax : SyntaxNode =>
        condition.IsInvalidBuilderInitialization;

    internal Condition MethodReturnTypeIs(KnownType returnType) =>
        context => context.MethodSymbol.Value is { } method
                    && method.ReturnType.DerivesFrom(returnType);

    internal Condition ArgumentIsBoolConstant(string parameterName, bool expectedValue) =>
        context => ConstArgumentForParameter(context, parameterName) is bool boolValue
                    && boolValue == expectedValue;

    internal Condition IsIHeadersDictionary() =>
        context => context.MethodSymbol.Value.ContainingType.TypeArguments is var typeArguments
                    && typeArguments.Length == 2
                    && typeArguments[0].Is(KnownType.System_String)
                    && typeArguments[1].Is(KnownType.Microsoft_Extensions_Primitives_StringValues);

    protected virtual SyntaxNode NodeExpression(SyntaxNode node) =>
        Language.Syntax.NodeExpression(node);

    protected override InvocationContext CreateContext(SonarSyntaxNodeReportingContext context) =>
        NodeExpression(context.Node) is { } expression && ExpectedExpressionIdentifier(expression) is { } identifier
            ? new InvocationContext(context, identifier.ValueText)
            : null;
}
