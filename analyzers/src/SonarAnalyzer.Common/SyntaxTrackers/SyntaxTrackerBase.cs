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
    public delegate bool TrackingCondition<in TContext>(TContext trackingContext);

    public abstract class SyntaxTrackerBase<TSyntaxKind, TContext> : TrackerBase
        where TSyntaxKind : struct
        where TContext : SyntaxBaseContext
    {
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TSyntaxKind[] TrackedSyntaxKinds { get; }
        protected abstract SyntaxBaseContext CreateContext(SyntaxNode expression, SemanticModel semanticModel);

        protected SyntaxTrackerBase(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) : base(analyzerConfiguration, rule) { }

        public void Track(SonarAnalysisContext context, params TrackingCondition<TContext>[] conditions) =>
            Track(context, new object[0], conditions);

        public void Track(SonarAnalysisContext context, object[] diagnosticMessageArgs, params TrackingCondition<TContext>[] conditions)
        {
            context.RegisterCompilationStartAction(
              c =>
              {
                  if (IsEnabled(c.Options))
                  {
                      c.RegisterSyntaxNodeActionInNonGenerated(
                          GeneratedCodeRecognizer,
                          TrackAndReportIfNecessary,
                          TrackedSyntaxKinds);
                  }
              });

            void TrackAndReportIfNecessary(SyntaxNodeAnalysisContext c)
            {
                if (CreateContext(c.Node, c.SemanticModel) is { } trackingContext
                    && conditions.All(c => c((TContext)trackingContext))
                    && trackingContext.PrimaryLocation != null
                    && trackingContext.PrimaryLocation != Location.None)
                {
                    c.ReportDiagnosticWhenActive(
                        Diagnostic.Create(Rule,
                                          trackingContext.PrimaryLocation,
                                          trackingContext.SecondaryLocations.ToAdditionalLocations(),
                                          trackingContext.SecondaryLocations.ToProperties(),
                                          diagnosticMessageArgs));
                }
            }
        }
    }
}
