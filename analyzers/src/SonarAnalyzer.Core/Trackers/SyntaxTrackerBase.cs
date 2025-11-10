/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

public abstract class SyntaxTrackerBase<TSyntaxKind, TContext> : TrackerBase<TSyntaxKind, TContext>
    where TSyntaxKind : struct
    where TContext : SyntaxBaseContext
{
    protected abstract TSyntaxKind[] TrackedSyntaxKinds { get; }
    protected abstract TContext CreateContext(SonarSyntaxNodeReportingContext context);

    public void Track(TrackerInput input, params Condition[] conditions) =>
        Track(input, [], conditions);

    public void Track(TrackerInput input, string[] diagnosticMessageArgs, params Condition[] conditions)
    {
        input.Context.RegisterCompilationStartAction(c =>
          {
              if (input.IsEnabled(c.Options))
              {
                  c.RegisterNodeAction(Language.GeneratedCodeRecognizer, TrackAndReportIfNecessary, TrackedSyntaxKinds);
              }
          });

        void TrackAndReportIfNecessary(SonarSyntaxNodeReportingContext c)
        {
            if (CreateContext(c) is { } trackingContext
                && Array.TrueForAll(conditions, x => x(trackingContext))
                && trackingContext.PrimaryLocation is not null
                && trackingContext.PrimaryLocation != Location.None)
            {
                c.ReportIssue(input.Rule, trackingContext.PrimaryLocation, trackingContext.SecondaryLocations, diagnosticMessageArgs);
            }
        }
    }

    public Condition ExceptWhen(Condition condition) =>
        x => !condition(x);

    public Condition And(Condition condition1, Condition condition2) =>
        x => condition1(x) && condition2(x);

    public Condition Or(Condition condition1, Condition condition2) =>
        x => condition1(x) || condition2(x);

    public Condition Or(Condition condition1, Condition condition2, Condition condition3) =>
        x => condition1(x) || condition2(x) || condition3(x);
}
