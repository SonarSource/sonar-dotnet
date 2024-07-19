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

    private static readonly ISet<SyntaxKind> SideEffectExpression =
        new HashSet<SyntaxKind>
        {
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.AwaitExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.InvocationExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.PostDecrementExpression,
            SyntaxKind.PostIncrementExpression,
            SyntaxKind.PreDecrementExpression,
            SyntaxKind.PreIncrementExpression,
            SyntaxKind.RightShiftAssignmentExpression,
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKindEx.CoalesceAssignmentExpression,
            SyntaxKindEx.UnsignedRightShiftAssignmentExpression
        };

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodParameterUnused.DiagnosticId);

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var parameter = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as ParameterSyntax;


        if (!bool.Parse(diagnostic.Properties[MethodParameterUnused.IsRemovableKey]))
        {
            return Task.CompletedTask;
        }

        context.RegisterCodeFix(
            Title,
            async x =>
            {
                var newRoot = root.RemoveNodes(
                    [parameter, ..(await ArgumentsToRemoveAsync(context, parameter, x))],
                    SyntaxRemoveOptions.KeepLeadingTrivia | SyntaxRemoveOptions.AddElasticMarker);

                return context.Document.WithSyntaxRoot(newRoot);
            },
            context.Diagnostics);
        return Task.CompletedTask;
    }

    private static async Task<IEnumerable<SyntaxNode>> ArgumentsToRemoveAsync(SonarCodeFixContext context, ParameterSyntax parameter, CancellationToken cancel)
    {
        IEnumerable<SyntaxNode> arguments = [];

        if (parameter?.Parent.Parent is BaseMethodDeclarationSyntax methodDeclaration)
        {
            var model = await context.Document.GetSemanticModelAsync(cancel);

            if (model.GetDeclaredSymbol(methodDeclaration, cancel) is { } methodSymbol)
            {
                var callers = await SymbolFinder.FindCallersAsync(
                    methodSymbol,
                    context.Document.Project.Solution,
                    ImmutableHashSet.Create(context.Document),
                    cancel);

                arguments = callers
                    .SelectMany(x => x.Locations)
                    .Select(x => GetArgumentFromLocation(x, parameter))
                    .Where(x => x is not null && !x.DescendantNodes().Select(y => y.Kind()).Any(y => SideEffectExpression.Contains(y)));
            }
        }

        return arguments;
    }

    private static ArgumentSyntax GetArgumentFromLocation(Location location, ParameterSyntax parameter) =>
        location.SourceTree.GetRoot().FindNode(location.SourceSpan, getInnermostNodeForTie: true) is { } node
        && GetIdentifierNameParent(node).ArgumentList() is { } argumentList
            ? argumentList.Arguments.FirstOrDefault(x => ArgumentEqualsParameter(x, parameter))
            : null;

    private static bool ArgumentEqualsParameter(ArgumentSyntax argument, ParameterSyntax parameter) =>
        argument.NameIs(parameter.GetName()) || argument.GetArgumentIndex() == parameter.GetParameterIndex();

    private static SyntaxNode GetIdentifierNameParent(SyntaxNode node) =>
        node is IdentifierNameSyntax identifierName ? identifierName.Parent : node;
}
