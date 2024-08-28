/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Analyzers;

public abstract class TrackerHotspotDiagnosticAnalyzer<TSyntaxKind> : HotspotDiagnosticAnalyzer where TSyntaxKind : struct
{
    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
    protected abstract void Initialize(TrackerInput input);

    protected DiagnosticDescriptor Rule { get; }

    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected TrackerHotspotDiagnosticAnalyzer(IAnalyzerConfiguration configuration, string diagnosticId, string messageFormat) : base(configuration) =>
        Rule = Language.CreateDescriptor(diagnosticId, messageFormat);

    protected override void Initialize(SonarAnalysisContext context) =>
        Initialize(new TrackerInput(context, Configuration, Rule));
}
