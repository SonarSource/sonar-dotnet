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

using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public abstract class AdditionalFileTypeHotspotAnalyzer : HotspotDiagnosticAnalyzer
    {
        protected FilesToAnalyzeProvider FilesToAnalyzeProvider { get; private set; }

        protected AdditionalFileTypeHotspotAnalyzer(IAnalyzerConfiguration configuration) : base(configuration) { }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            var analysisContext = new AdditionalCompilationStartActionAnalysisContext(context);
            Initialize(analysisContext);

            context.RegisterCompilationStartAction(
                cac =>
                {
                    var projectConfigReader = new ProjectConfigReader(cac.Options);
                    FilesToAnalyzeProvider = new FilesToAnalyzeProvider(projectConfigReader.FilesToAnalyzePath);
                    foreach (var compilationStartActions in analysisContext.CompilationStartActions)
                    {
                        compilationStartActions(cac);
                    }
                });
        }

        protected abstract void Initialize(AdditionalCompilationStartActionAnalysisContext context);
    }
}
