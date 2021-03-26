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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public abstract class TrackerHotspotDiagnosticAnalyzer<TSyntaxKind> : HotspotDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected DiagnosticDescriptor Rule { get; }

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract void Initialize(TrackerInput input);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected TrackerHotspotDiagnosticAnalyzer(IAnalyzerConfiguration configuration, string diagnosticId, string messageFormat) : base(configuration)
        {
            Rule = DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, messageFormat, Language.RspecResources);
            if (configuration == AnalyzerConfiguration.Hotspot)
            {
                Rule = Rule.WithNotConfigurable();
            }
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            Initialize(new TrackerInput(context, Configuration, Rule));
    }
}
