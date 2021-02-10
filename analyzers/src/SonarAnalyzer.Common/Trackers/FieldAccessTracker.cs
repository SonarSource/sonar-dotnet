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
    public abstract class FieldAccessTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, FieldAccessContext>
        where TSyntaxKind : struct
    {
        public abstract Condition WhenRead();
        public abstract Condition MatchSet();
        public abstract Condition AssignedValueIsConstant();
        protected abstract bool IsIdentifierWithinMemberAccess(SyntaxNode expression);

        public Condition MatchField(params MemberDescriptor[] fields) =>
            context => MemberDescriptor.MatchesAny(context.FieldName, context.InvokedFieldSymbol, false, Language.NameComparison, fields);

        protected override FieldAccessContext CreateContext(SyntaxNodeAnalysisContext context)
        {
            // We register for both MemberAccess and IdentifierName and we want to avoid raising two times for the same identifier.
            if (IsIdentifierWithinMemberAccess(context.Node))
            {
                return null;
            }

            return Language.Syntax.NodeIdentifier(context.Node) is { } fieldIdentifier ? new FieldAccessContext(context, fieldIdentifier.ValueText) : null;
        }
    }
}
