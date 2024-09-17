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

namespace SonarAnalyzer.Core.Trackers;

public abstract class ElementAccessTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, ElementAccessContext>
    where TSyntaxKind : struct
{
    public abstract object AssignedValue(ElementAccessContext context);
    public abstract Condition ArgumentAtIndexEquals(int index, string value);
    public abstract Condition MatchSetter();
    public abstract Condition MatchProperty(MemberDescriptor member);

    internal Condition ArgumentAtIndexIs(int index, params KnownType[] types) =>
        context => context.InvokedPropertySymbol.Value is { } property
            && property.Parameters.Length > index
            && property.Parameters[0].Type.DerivesOrImplements(types[index]);

    internal Condition MatchIndexerIn(params KnownType[] types) =>
        context => context.InvokedPropertySymbol.Value is { } property
            && property.ContainingType.DerivesOrImplementsAny(types.ToImmutableArray());

    protected override ElementAccessContext CreateContext(SonarSyntaxNodeReportingContext context) =>
        new(context);
}
