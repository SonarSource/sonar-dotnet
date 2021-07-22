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

using System;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    public abstract class SonarDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string EnableParallelProcessing = "SONAR_DOTNET_ENABLE_PARALLEL_PROCESSING";
        protected virtual bool ParallelProcessingDisabled => IsParallelProcessingDisabled();

        protected abstract void Initialize(SonarAnalysisContext context);

        public sealed override void Initialize(AnalysisContext context)
        {
            if (!ParallelProcessingDisabled)
            {
                context.EnableConcurrentExecution();
            }
            Initialize(new SonarAnalysisContext(context, SupportedDiagnostics));
        }

        private static bool IsParallelProcessingDisabled()
        {
            var value = Environment.GetEnvironmentVariable(EnableParallelProcessing);

            if (value != null && bool.TryParse(value, out var result))
            {
                return !result;
            }
            return true;
        }
    }
}
