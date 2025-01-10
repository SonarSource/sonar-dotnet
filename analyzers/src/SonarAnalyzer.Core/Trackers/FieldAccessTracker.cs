/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

public abstract class FieldAccessTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, FieldAccessContext>
    where TSyntaxKind : struct
{
    public abstract Condition WhenRead();
    public abstract Condition MatchSet();
    public abstract Condition AssignedValueIsConstant();
    protected abstract bool IsIdentifierWithinMemberAccess(SyntaxNode expression);

    public Condition MatchField(params MemberDescriptor[] fields) =>
        context => MemberDescriptor.MatchesAny(context.FieldName, context.InvokedFieldSymbol, false, Language.NameComparison, fields);

    protected override FieldAccessContext CreateContext(SonarSyntaxNodeReportingContext context)
    {
        // We register for both MemberAccess and IdentifierName and we want to avoid raising two times for the same identifier.
        if (IsIdentifierWithinMemberAccess(context.Node))
        {
            return null;
        }

        return Language.Syntax.NodeIdentifier(context.Node) is { } fieldIdentifier ? new FieldAccessContext(context, fieldIdentifier.ValueText) : null;
    }
}
