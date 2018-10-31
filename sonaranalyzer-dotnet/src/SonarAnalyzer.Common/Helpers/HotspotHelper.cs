/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public abstract class HotspotHelper
    {
        private readonly IAnalyzerConfiguration analysisConfiguration;
        private readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics;

        public HotspotHelper(IAnalyzerConfiguration analysisConfiguration, ImmutableArray<DiagnosticDescriptor> supportedDiagnostics)
        {
            this.analysisConfiguration = analysisConfiguration;
            this.supportedDiagnostics = supportedDiagnostics;
        }

        protected bool IsEnabled(AnalyzerOptions options)
        {
            if (analysisConfiguration.EnabledRules == null)
            {
                analysisConfiguration.Read(options);
            }

            if (analysisConfiguration.EnabledRules == null)
            {
                return false;
            }

            return supportedDiagnostics.Any(d => analysisConfiguration.IsEnabled(d.Id));
        }

        public abstract void TrackMethodInvocations(SonarAnalysisContext context, DiagnosticDescriptor rule, params MethodSignature[] trackedMethods);

        public abstract void TrackPropertyAccess(SonarAnalysisContext context, DiagnosticDescriptor rule, params MethodSignature[] trackedMethods);
    }
}
