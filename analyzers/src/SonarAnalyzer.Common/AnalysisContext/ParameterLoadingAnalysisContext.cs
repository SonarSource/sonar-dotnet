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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers;

public sealed class ParameterLoadingAnalysisContext : SonarAnalysisContextBase // FIXME: Refactor
{
    private readonly List<Action<CompilationStartAnalysisContext>> compilationStartActions = new();

    internal SonarAnalysisContext Context { get; }
    internal IEnumerable<Action<CompilationStartAnalysisContext>> CompilationStartActions => compilationStartActions;

    internal ParameterLoadingAnalysisContext(SonarAnalysisContext context) =>
        Context = context;

    internal void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) =>
        compilationStartActions.Add(action);    // only collect compilation start actions and call them later

    public override bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value) =>
        Context.TryGetValue(text, valueProvider, out value);
}
