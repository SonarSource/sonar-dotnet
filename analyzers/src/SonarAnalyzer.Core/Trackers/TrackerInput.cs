/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

public class TrackerInput
{
    private readonly IAnalyzerConfiguration configuration;

    public DiagnosticDescriptor Rule { get; }
    public SonarAnalysisContext Context { get; }

    public TrackerInput(SonarAnalysisContext context, IAnalyzerConfiguration configuration, DiagnosticDescriptor rule)
    {
        Context = context;
        this.configuration = configuration;
        Rule = rule;
    }

    public bool IsEnabled(AnalyzerOptions options)
    {
        configuration.Initialize(options);
        return configuration.IsEnabled(Rule.Id);
    }
}
