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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Helpers;

public abstract class SonarCodeFix : CodeFixProvider
{
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Document.SupportsSyntaxTree)
        {
            return;
        }

        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        /// This only disables code-fixes when different versions are loaded
        /// In case of analyzers, <see cref="SonarAnalysisContext.LegacyIsRegisteredActionEnabled"/> is sufficient, because Roslyn only
        /// creates a single instance from each assembly-version, so we can disable the VSIX analyzers
        /// In case of code fix providers Roslyn creates multiple instances of the code fix providers. Which means that
        /// we can only disable one of them if they are created from different assembly-versions.
        /// If the VSIX and the NuGet has the same version, then code fixes show up multiple times, this ticket will fix
        /// this problem: https://github.com/dotnet/roslyn/issues/4030
        if (SonarAnalysisContext.LegacyIsRegisteredActionEnabled(Enumerable.Empty<DiagnosticDescriptor>(), syntaxRoot.SyntaxTree))
        {
            await RegisterCodeFixesAsync(syntaxRoot, new(context)).ConfigureAwait(false);
        }
    }

    public override FixAllProvider GetFixAllProvider() =>
        DocumentBasedFixAllProvider.Instance;

    protected abstract Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context);
}
