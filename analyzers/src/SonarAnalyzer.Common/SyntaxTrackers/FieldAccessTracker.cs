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

using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public delegate bool FieldAccessCondition(FieldAccessContext invocationContext);

    public abstract class FieldAccessTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, FieldAccessCondition>
        where TSyntaxKind : struct
    {
        public abstract FieldAccessCondition WhenRead();
        public abstract FieldAccessCondition MatchSet();
        public abstract FieldAccessCondition AssignedValueIsConstant();
        protected abstract bool IsIdentifierWithinMemberAccess(SyntaxNode expression);
        protected abstract string GetFieldName(SyntaxNode expression);

        private bool CaseInsensitiveComparison { get; }

        protected FieldAccessTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule, bool caseInsensitiveComparison = false) : base(analyzerConfiguration, rule) =>
            CaseInsensitiveComparison = caseInsensitiveComparison;

        public FieldAccessCondition MatchField(params MemberDescriptor[] fields) =>
            context => MemberDescriptor.MatchesAny(context.FieldName, context.InvokedFieldSymbol, false, CaseInsensitiveComparison, fields);

        protected override BaseContext IsTracked(SyntaxNode expression, SemanticModel semanticModel, FieldAccessCondition[] conditions, out Location location)
        {
            location = Location.None;

            // We register for both MemberAccess and IdentifierName and we want to
            // avoid raising two times for the same identifier.
            if (IsIdentifierWithinMemberAccess(expression))
            {
                return null;
            }

            var fieldName = GetFieldName(expression);
            if (fieldName == null)
            {
                return null;
            }

            var context = new FieldAccessContext(expression, fieldName, semanticModel);
            location = expression.GetLocation();
            return conditions.All(c => c(context)) ? context : null;
        }
    }
}
