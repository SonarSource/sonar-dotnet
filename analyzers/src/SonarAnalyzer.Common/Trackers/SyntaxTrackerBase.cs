/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Helpers
{
    public abstract class SyntaxTrackerBase<TSyntaxKind, TContext> : TrackerBase<TSyntaxKind, TContext>
        where TSyntaxKind : struct
        where TContext : SyntaxBaseContext
    {
        protected abstract TSyntaxKind[] TrackedSyntaxKinds { get; }
        protected abstract TContext CreateContext(SonarSyntaxNodeReportingContext context);

        public void Track(TrackerInput input, params Condition[] conditions) =>
            Track(input, Array.Empty<object>(), conditions);

        public void Track(TrackerInput input, object[] diagnosticMessageArgs, params Condition[] conditions)
        {
            input.Context.RegisterCompilationStartAction(c =>
              {
                  if (input.IsEnabled(c.Options))
                  {
                      c.RegisterNodeAction(
                          Language.GeneratedCodeRecognizer,
                          TrackAndReportIfNecessary,
                          TrackedSyntaxKinds);
                  }
              });

            void TrackAndReportIfNecessary(SonarSyntaxNodeReportingContext c)
            {
                if (CreateContext(c) is { } trackingContext
                    && conditions.All(c => c(trackingContext))
                    && trackingContext.PrimaryLocation != null
                    && trackingContext.PrimaryLocation != Location.None)
                {
                    c.ReportIssue(
                        Diagnostic.Create(input.Rule,
                                          trackingContext.PrimaryLocation.EnsureMappedLocation(),
                                          trackingContext.SecondaryLocations.ToAdditionalLocations(),
                                          trackingContext.SecondaryLocations.ToProperties(),
                                          diagnosticMessageArgs));
                }
            }
        }
    }
}
