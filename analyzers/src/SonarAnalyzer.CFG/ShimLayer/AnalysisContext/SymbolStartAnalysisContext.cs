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

namespace SonarAnalyzer.ShimLayer.AnalysisContext;

public class SymbolStartAnalysisContext
{
    private readonly Action<Action<CodeBlockAnalysisContext>> registerCodeBlockAction;

    public SymbolStartAnalysisContext(
        CancellationToken cancellationToken,
        Compilation compilation,
        AnalyzerOptions options,
        ISymbol symbol,
        Action<Action<CodeBlockAnalysisContext>> registerCodeBlockAction)
    {
        CancellationToken = cancellationToken;
        Compilation = compilation;
        Options = options;
        Symbol = symbol;
        this.registerCodeBlockAction = registerCodeBlockAction;
    }

    public CancellationToken CancellationToken { get; }
    public Compilation Compilation { get; }
    public AnalyzerOptions Options { get; }
    public ISymbol Symbol { get; }

    public void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) => registerCodeBlockAction(action);
}
