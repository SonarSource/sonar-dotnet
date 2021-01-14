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
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public abstract class PropertyAccessTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, PropertyAccessContext>
        where TSyntaxKind : struct
    {
        public abstract object AssignedValue(PropertyAccessContext context);
        public abstract Condition MatchGetter();
        public abstract Condition MatchSetter();
        public abstract Condition AssignedValueIsConstant();
        protected abstract bool IsIdentifierWithinMemberAccess(SyntaxNode expression);
        protected abstract string GetPropertyName(SyntaxNode expression);

        private bool CaseInsensitiveComparison { get; }

        protected PropertyAccessTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule, bool caseInsensitiveComparison = false) : base(analyzerConfiguration, rule) =>
           CaseInsensitiveComparison = caseInsensitiveComparison;

        public Condition MatchProperty(params MemberDescriptor[] properties) =>
            context => MemberDescriptor.MatchesAny(context.PropertyName, context.PropertySymbol, false, CaseInsensitiveComparison, properties);

        protected override PropertyAccessContext CreateContext(SyntaxNodeAnalysisContext context)
        {
            // We register for both MemberAccess and IdentifierName and we want to
            // avoid raising two times for the same identifier.
            if (IsIdentifierWithinMemberAccess(context.Node))
            {
                return null;
            }

            return GetPropertyName(context.Node) is string propertyName ? new PropertyAccessContext(context.Node, propertyName, context.SemanticModel) : null;
        }
    }
}
