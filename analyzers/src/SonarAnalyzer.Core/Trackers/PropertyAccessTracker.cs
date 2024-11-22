/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

public abstract class PropertyAccessTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, PropertyAccessContext>
    where TSyntaxKind : struct
{
    public abstract object AssignedValue(PropertyAccessContext context);
    public abstract Condition MatchGetter();
    public abstract Condition MatchSetter();
    public abstract Condition AssignedValueIsConstant();
    protected abstract bool IsIdentifierWithinMemberAccess(SyntaxNode expression);

    public Condition MatchProperty(params MemberDescriptor[] properties) =>
        MatchProperty(false, properties);

    public Condition MatchProperty(bool checkOverridenProperties, params MemberDescriptor[] properties) =>
        context => MemberDescriptor.MatchesAny(context.PropertyName, context.PropertySymbol, checkOverridenProperties, Language.NameComparison, properties);

    protected override PropertyAccessContext CreateContext(SonarSyntaxNodeReportingContext context)
    {
        // We register for both MemberAccess and IdentifierName and we want to
        // avoid raising two times for the same identifier.
        if (IsIdentifierWithinMemberAccess(context.Node))
        {
            return null;
        }

        return Language.Syntax.NodeIdentifier(context.Node) is { } propertyIdentifier ? new PropertyAccessContext(context, propertyIdentifier.ValueText) : null;
    }
}
