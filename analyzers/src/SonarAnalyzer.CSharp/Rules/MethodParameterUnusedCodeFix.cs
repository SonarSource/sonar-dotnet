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
using Microsoft.CodeAnalysis.FindSymbols;

namespace SonarAnalyzer.Rules.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class MethodParameterUnusedCodeFix : SonarCodeFix
{
    private const string Title = "Remove unused parameter";

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodParameterUnused.DiagnosticId);

    protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var parameter = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as ParameterSyntax;
        IEnumerable<SyntaxNode> arguments = [];

        if (!bool.Parse(diagnostic.Properties[MethodParameterUnused.IsRemovableKey]))
        {
            return;
        }

        if (parameter?.Parent.Parent is BaseMethodDeclarationSyntax methodDeclaration)
        {
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var parameterIndex = methodDeclaration.ParameterList.Parameters.IndexOf(parameter);

            if (parameterIndex >= 0 && model.GetDeclaredSymbol(methodDeclaration, context.CancellationToken) is { } methodSymbol)
            {
                var callers = await SymbolFinder.FindCallersAsync(
                    methodSymbol,
                    context.Document.Project.Solution,
                    ImmutableHashSet.Create(context.Document),
                    context.CancellationToken);

                arguments = callers
                    .SelectMany(x => x.Locations)
                    .Select(x => GetArgumentFromLocation(x, parameterIndex));
            }
        }

        context.RegisterCodeFix(
            Title,
            _ =>
            {
                var newRoot = root.RemoveNodes(
                    [parameter, ..arguments.Where(x => x is not null)],
                    SyntaxRemoveOptions.KeepLeadingTrivia | SyntaxRemoveOptions.AddElasticMarker);

                return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
            },
            context.Diagnostics);
    }

    private static SyntaxNode GetArgumentFromLocation(Location location, int parameterIndex) =>
        location.SourceTree.GetRoot().FindNode(location.SourceSpan, getInnermostNodeForTie: true) is { } node
        && GetIdentifierNameParent(node).ArgumentList() is { } argumentList
        && argumentList.Arguments.Count > parameterIndex
            ? argumentList.Arguments[parameterIndex]
            : null;

    private static SyntaxNode GetIdentifierNameParent(SyntaxNode node) =>
        node is IdentifierNameSyntax identifierName ? identifierName.Parent : node;
}
