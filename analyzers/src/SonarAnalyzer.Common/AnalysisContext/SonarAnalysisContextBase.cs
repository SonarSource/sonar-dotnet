/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

namespace SonarAnalyzer;

public class SonarAnalysisContextBase
{
    //protected void Register<TContext>(Action<Action<TContext>> registrationAction,
    //                                  Action<TContext> registeredAction,
    //                                  Func<TContext, SyntaxTree> getSyntaxTree,
    //                                  Func<TContext, Compilation> getCompilation,
    //                                  Func<TContext, AnalyzerOptions> getAnalyzerOptions) =>
    //registrationAction(c =>
    //{
    //    // For each action registered on context we need to do some pre-processing before actually calling the rule.
    //    // First, we need to ensure the rule does apply to the current scope (main vs test source).
    //    // Second, we call an external delegate (set by SonarLint for VS) to ensure the rule should be run (usually
    //    // the decision is made on based on whether the project contains the analyzer as NuGet).
    //    var compilation = getCompilation(c);
    //    var isTestProject = IsTestProject(compilation, getAnalyzerOptions(c));

    //    if (IsAnalysisScopeMatching(compilation, isTestProject, IsScannerRun(getAnalyzerOptions(c)), supportedDiagnostics)
    //            && IsRegisteredActionEnabled(supportedDiagnostics, getSyntaxTree(c)))
    //    {
    //        registeredAction(c);
    //    }
    //});


}

public abstract class SonarAnalysisContextBase<TContext> : SonarAnalysisContextBase
{
    public abstract SyntaxTree Tree { get; }
    public abstract Compilation Compilation { get; }
    public abstract AnalyzerOptions Options { get; }

    public SonarAnalysisContext AnalysisContext { get; }
    public TContext Context { get; }

    protected SonarAnalysisContextBase(SonarAnalysisContext analysisContext, TContext context)
    {
        AnalysisContext = analysisContext ?? throw new ArgumentNullException(nameof(analysisContext));
        Context = context;
    }
}
