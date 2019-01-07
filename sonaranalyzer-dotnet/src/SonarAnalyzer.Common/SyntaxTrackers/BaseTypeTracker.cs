/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// Checker method called by <see cref="BaseClassTracker"/> to check whether
    /// an issue should be reported because of a type the class is inheriting from.
    /// </summary>
   public delegate bool BaseClassCondition(BaseTypeContext context, out Location issueLocation);

    /// <summary>
    /// Tracker class for rules that check the inheritance tree for e.g. disallowed base classes
    /// </summary>
    /// <typeparam name="TSyntaxKind"></typeparam>
    public abstract class BaseTypeTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected BaseTypeTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule)
            : base(analyzerConfiguration, rule)
        {
        }

        public void Track(SonarAnalysisContext context, params BaseClassCondition[] conditions)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsEnabled(c.Options))
                    {
                        c.RegisterSyntaxNodeActionInNonGenerated(
                            GeneratedCodeRecognizer,
                            TrackInheritance,
                            TrackedSyntaxKinds);
                    }
                });

            void TrackInheritance(SyntaxNodeAnalysisContext c)
            {
                Location issueLocation;
                if (IsTrackedRelationship(c.Node, c.SemanticModel, out issueLocation))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, issueLocation));
                }
            }

            bool IsTrackedRelationship(SyntaxNode contextNode, SemanticModel semanticModel, out Location issueLocation)
            {
                var baseTypeList = GetBaseTypeNodes(contextNode);
                if (baseTypeList == null || !baseTypeList.Any())
                {
                    issueLocation = Location.None;
                    return false;
                }

                var baseClassContext = new BaseTypeContext(contextNode, baseTypeList, semanticModel);

                // We can't pass the issueLocation to the lambda directly so we need a temporary variable
                Location locationToReport = null;
                if (conditions.All(c => c(baseClassContext, out locationToReport)))
                {
                    issueLocation = locationToReport;
                    return true;
                }
                issueLocation = Location.None;
                return false;
            }
        }

        /// <summary>
        /// Extract the list of type syntax nodes for the base types/interface types
        /// </summary>
        protected abstract IEnumerable<SyntaxNode> GetBaseTypeNodes(SyntaxNode contextNode);

        internal BaseClassCondition MatchSubclassesOf(params KnownType[] types)
        {
            var immutableTypes = types.ToImmutableArray();
            return (BaseTypeContext context, out Location issueLocation) =>
            {
                foreach (var baseTypeNode in context.AllBaseTypeNodes)
                {
                    if (context.SemanticModel.GetTypeInfo(baseTypeNode).Type.DerivesOrImplementsAny(immutableTypes))
                    {
                        issueLocation = baseTypeNode.GetLocation();
                        return true; // assume there won't be more than one matching node
                    }
                }

                issueLocation = null;
                return false;
            };
        }
    }
}
