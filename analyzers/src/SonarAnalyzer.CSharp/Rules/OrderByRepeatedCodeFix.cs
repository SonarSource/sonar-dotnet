/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class OrderByRepeatedCodeFix : SonarCodeFix
{
    internal const string Title = "Change 'OrderBy' to 'ThenBy'";

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OrderByRepeated.DiagnosticId);

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Only 'OrderBy'/'OrderByDescending' take a key selector that 'ThenBy' also accepts, so the fix is safe
        // for those alone. 'Order'/'OrderDescending' and query comprehension diagnostics are left without a fix.
        if (root.FindNode(diagnosticSpan) is IdentifierNameSyntax { Identifier.ValueText: nameof(Enumerable.OrderBy) or nameof(Enumerable.OrderByDescending) } syntaxNode)
        {
            context.RegisterCodeFix(Title, x => ChangeToThenByAsync(context.Document, syntaxNode, x), context.Diagnostics);
        }

        return Task.CompletedTask;
    }

    private static async Task<Document> ChangeToThenByAsync(Document document, SyntaxNode node, CancellationToken cancel)
    {
        var root = await document.GetSyntaxRootAsync(cancel).ConfigureAwait(false);
        var newRoot = root.ReplaceNode(node, SyntaxFactory.IdentifierName(nameof(Enumerable.ThenBy)));
        return document.WithSyntaxRoot(newRoot);
    }
}
