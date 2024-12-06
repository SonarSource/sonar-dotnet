/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Core.Analyzers;

public abstract class TrackerHotspotDiagnosticAnalyzer<TSyntaxKind> : HotspotDiagnosticAnalyzer where TSyntaxKind : struct
{
    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
    protected abstract void Initialize(TrackerInput input);

    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    protected DiagnosticDescriptor Rule { get; }

    protected TrackerHotspotDiagnosticAnalyzer(IAnalyzerConfiguration configuration, string diagnosticId, string messageFormat) : base(configuration) =>
        Rule = Language.CreateDescriptor(diagnosticId, messageFormat);

    protected override void Initialize(SonarAnalysisContext context) =>
        Initialize(new TrackerInput(context, Configuration, Rule));
}
