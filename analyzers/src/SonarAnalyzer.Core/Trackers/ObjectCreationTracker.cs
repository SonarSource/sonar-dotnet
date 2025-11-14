/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

public abstract class ObjectCreationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, ObjectCreationContext>
    where TSyntaxKind : struct
{
    public abstract Condition ArgumentAtIndexIsConst(int index);
    public abstract object ConstArgumentForParameter(ObjectCreationContext context, string parameterName);

    protected override TSyntaxKind[] TrackedSyntaxKinds => Language.SyntaxKind.ObjectCreationExpressions;

    internal Condition ArgumentIsBoolConstant(string parameterName, bool expectedValue) =>
        context => ConstArgumentForParameter(context, parameterName) is bool boolValue && boolValue == expectedValue;

    internal Condition ArgumentAtIndexIs(int index, KnownType type) =>
        context => context.InvokedConstructorSymbol.Value is { } constructor
            && constructor.Parameters.Length > index
            && constructor.Parameters[index].Type.Is(type);

    internal Condition WhenDerivesOrImplementsAny(params KnownType[] types) =>
        context => context.InvokedConstructorSymbol.Value is { } constructor
            && constructor.IsConstructor()
            && constructor.ContainingType.DerivesOrImplementsAny(types.ToImmutableArray());

    internal Condition MatchConstructor(params KnownType[] types) =>
        // We cannot do a syntax check first because a type name can be aliased with
        // a using Alias = Fully.Qualified.Name and we will generate false negative
        // for new Alias()
        context => context.InvokedConstructorSymbol.Value is { } constructor
            && constructor.IsConstructor()
            && constructor.ContainingType.IsAny(types);

    internal Condition WhenDerivesFrom(KnownType baseType) =>
        context => context.InvokedConstructorSymbol.Value is { } constructor
            && constructor.IsConstructor()
            && constructor.ContainingType.DerivesFrom(baseType);

    internal Condition WhenImplements(KnownType baseType) =>
        context => context.InvokedConstructorSymbol.Value is { } constructor
            && constructor.IsConstructor()
            && constructor.ContainingType.Implements(baseType);

    protected override ObjectCreationContext CreateContext(SonarSyntaxNodeReportingContext context) =>
        new ObjectCreationContext(context);
}
