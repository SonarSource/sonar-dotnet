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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public delegate bool PropertyAccessCondition(PropertyAccessContext invocationContext);

    public abstract class PropertyAccessTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind>
        where TSyntaxKind : struct
    {
        private bool CaseInsensitiveComparison { get; }

        public abstract object AssignedValue(PropertyAccessContext context);
        public abstract PropertyAccessCondition MatchGetter();
        public abstract PropertyAccessCondition MatchSetter();
        public abstract PropertyAccessCondition AssignedValueIsConstant();
        protected abstract bool IsIdentifierWithinMemberAccess(SyntaxNode expression);
        protected abstract string GetPropertyName(SyntaxNode expression);

        protected PropertyAccessTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule, bool caseInsensitiveComparison = false) : base(analyzerConfiguration, rule) =>
            CaseInsensitiveComparison = caseInsensitiveComparison;

        public void Track(SonarAnalysisContext context, params PropertyAccessCondition[] conditions) =>
            Track(context, new object[0], conditions);

        public void Track(SonarAnalysisContext context, object[] diagnosticMessageArgs,  params PropertyAccessCondition[] conditions)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsEnabled(c.Options))
                    {
                        c.RegisterSyntaxNodeActionInNonGenerated(
                            GeneratedCodeRecognizer,
                            TrackMemberAccess,
                            TrackedSyntaxKinds);
                    }
                });

            void TrackMemberAccess(SyntaxNodeAnalysisContext c)
            {
                if (IsTrackedProperty(c.Node, c.SemanticModel))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, c.Node.GetLocation(), diagnosticMessageArgs));
                }
            }

            bool IsTrackedProperty(SyntaxNode expression, SemanticModel semanticModel)
            {
                // We register for both MemberAccess and IdentifierName and we want to
                // avoid raising two times for the same identifier.
                if (IsIdentifierWithinMemberAccess(expression))
                {
                    return false;
                }

                var propertyName = GetPropertyName(expression);
                if (propertyName == null)
                {
                    return false;
                }

                var conditionContext = new PropertyAccessContext(expression, propertyName, semanticModel);
                return conditions.All(c => c(conditionContext));
            }
        }

        public PropertyAccessCondition MatchProperty(params MemberDescriptor[] properties) =>
            context => MemberDescriptor.MatchesAny(context.PropertyName, context.PropertySymbol, false, CaseInsensitiveComparison, properties);
    }
}
