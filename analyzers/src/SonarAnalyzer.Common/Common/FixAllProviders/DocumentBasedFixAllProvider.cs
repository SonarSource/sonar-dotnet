/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Common
{
    public class DocumentBasedFixAllProvider : FixAllProvider
    {
        #region Singleton implementation

        private DocumentBasedFixAllProvider()
        {
        }

        private static readonly Lazy<DocumentBasedFixAllProvider> Lazy = new(() => new DocumentBasedFixAllProvider());
        public static DocumentBasedFixAllProvider Instance => Lazy.Value;

        #endregion Singleton implementation

        private const string TitleSolutionPattern = "Fix all '{0}' in Solution";
        private const string TitleScopePattern = "Fix all '{0}' in '{1}'";
        private const string TitleFixAll = "Fix all '{0}'";

        private static string GetFixAllTitle(FixAllContext fixAllContext)
        {
            var diagnosticIds = fixAllContext.DiagnosticIds;
            var diagnosticId = string.Join(",", diagnosticIds.ToArray());

            switch (fixAllContext.Scope)
            {
                case FixAllScope.Document:
                    return string.Format(TitleScopePattern, diagnosticId, fixAllContext.Document.Name);

                case FixAllScope.Project:
                    return string.Format(TitleScopePattern, diagnosticId, fixAllContext.Project.Name);

                case FixAllScope.Solution:
                    return string.Format(TitleSolutionPattern, diagnosticId);

                default:
                    return TitleFixAll;
            }
        }

        public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
        {
            var title = GetFixAllTitle(fixAllContext);

            switch (fixAllContext.Scope)
            {
                case FixAllScope.Document:
                    return Task.FromResult(CodeAction.Create(title,
                        async ct => fixAllContext.Document.WithSyntaxRoot(
                            await GetFixedDocumentAsync(fixAllContext, fixAllContext.Document).ConfigureAwait(false))));

                case FixAllScope.Project:
                    return Task.FromResult(CodeAction.Create(title,
                        ct => GetFixedProjectAsync(fixAllContext, fixAllContext.Project)));

                case FixAllScope.Solution:
                    return Task.FromResult(CodeAction.Create(title,
                        ct => GetFixedSolutionAsync(fixAllContext)));

                default:
                    return Task.FromResult<CodeAction>(null);
            }
        }

        private static async Task<Solution> GetFixedSolutionAsync(FixAllContext fixAllContext)
        {
            var newSolution = fixAllContext.Solution;
            foreach (var projectId in newSolution.ProjectIds)
            {
                newSolution = await GetFixedProjectAsync(fixAllContext, newSolution.GetProject(projectId))
                    .ConfigureAwait(false);
            }
            return newSolution;
        }

        private static async Task<Solution> GetFixedProjectAsync(FixAllContext fixAllContext, Project project)
        {
            var solution = project.Solution;
            var newDocuments = project.Documents.ToDictionary(d => d.Id, d => GetFixedDocumentAsync(fixAllContext, d));
            await Task.WhenAll(newDocuments.Values).ConfigureAwait(false);
            foreach (var newDoc in newDocuments)
            {
                solution = solution.WithDocumentSyntaxRoot(newDoc.Key, newDoc.Value.Result);
            }
            return solution;
        }

        private static async Task<SyntaxNode> GetFixedDocumentAsync(FixAllContext fixAllContext, Document document)
        {
            var annotationKind = Guid.NewGuid().ToString();

            var diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
            var elementDiagnosticPairs = diagnostics
                .Select(d => new KeyValuePair<SyntaxNodeOrToken, Diagnostic>(GetReportedElement(d, root), d))
                .Where(n => !n.Key.IsMissing)
                .GroupBy(n => n.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);
            diagnostics = elementDiagnosticPairs.Values.ToImmutableArray(); // Continue with unique winners

            var diagnosticAnnotationPairs = new BidirectionalDictionary<Diagnostic, SyntaxAnnotation>();
            CreateAnnotationForDiagnostics(diagnostics, annotationKind, diagnosticAnnotationPairs);
            root = GetRootWithAnnotatedElements(root, elementDiagnosticPairs, diagnosticAnnotationPairs);

            var currentDocument = document.WithSyntaxRoot(root);
            var annotatedElements = root.GetAnnotatedNodesAndTokens(annotationKind).ToList();

            while (annotatedElements.Any())
            {
                var element = annotatedElements.First();
                var annotation = element.GetAnnotations(annotationKind).First();
                var diagnostic = diagnosticAnnotationPairs.GetByB(annotation);
                var location = root.GetAnnotatedNodesAndTokens(annotation).FirstOrDefault().GetLocation();
                if (location == null)
                {
                    // annotation is already removed from the tree
                    continue;
                }

                var newDiagnostic = Diagnostic.Create(
                    diagnostic.Descriptor,
                    location.EnsureMappedLocation(),
                    diagnostic.AdditionalLocations,
                    diagnostic.Properties);

                var fixes = new List<CodeAction>();
                var context = new CodeFixContext(currentDocument, newDiagnostic, (a, d) =>
                {
                    lock (fixes)
                    {
                        fixes.Add(a);
                    }
                }, fixAllContext.CancellationToken);
                await fixAllContext.CodeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

                var action = fixes.FirstOrDefault(fix => fix.EquivalenceKey == fixAllContext.CodeActionEquivalenceKey);
                if (action != null)
                {
                    var operations = await action.GetOperationsAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
                    var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
                    currentDocument = solution.GetDocument(document.Id);
                    root = await currentDocument.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
                }
                root = RemoveAnnotationIfExists(root, annotation);
                currentDocument = document.WithSyntaxRoot(root);
                annotatedElements = root.GetAnnotatedNodesAndTokens(annotationKind).ToList();
            }

            return await currentDocument.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
        }

        private static SyntaxNodeOrToken GetReportedElement(Diagnostic diagnostic, SyntaxNode root)
        {
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var exactMatch = token.Span == diagnostic.Location.SourceSpan;
            return exactMatch
                ? (SyntaxNodeOrToken)token
                : root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        }

        private static SyntaxNode RemoveAnnotationIfExists(SyntaxNode root, SyntaxAnnotation annotation)
        {
            var element = root.GetAnnotatedNodesAndTokens(annotation).FirstOrDefault();
            if (element == default)
            {
                return root;
            }

            if (element.IsNode)
            {
                var node = element.AsNode();
                return root.ReplaceNode(
                    node,
                    node.WithoutAnnotations(annotation));
            }

            var token = element.AsToken();
            return root.ReplaceToken(
                token,
                token.WithoutAnnotations(annotation));
        }

        private static SyntaxNode GetRootWithAnnotatedElements(SyntaxNode root,
            Dictionary<SyntaxNodeOrToken, Diagnostic> elementDiagnosticPairs,
            BidirectionalDictionary<Diagnostic, SyntaxAnnotation> diagnosticAnnotationPairs)
        {
            var nodes = elementDiagnosticPairs.Keys.Where(k => k.IsNode).Select(k => k.AsNode());
            var tokens = elementDiagnosticPairs.Keys.Where(k => k.IsToken).Select(k => k.AsToken());

            return root.ReplaceSyntax(
                nodes,
                (original, rewritten) =>
                {
                    var annotation = diagnosticAnnotationPairs.GetByA(elementDiagnosticPairs[original]);
                    return rewritten.WithAdditionalAnnotations(annotation);
                },
                tokens,
                (original, rewritten) =>
                {
                    var annotation = diagnosticAnnotationPairs.GetByA(elementDiagnosticPairs[original]);
                    return rewritten.WithAdditionalAnnotations(annotation);
                },
                null, null);
        }

        private static void CreateAnnotationForDiagnostics(System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics,
            string annotationKind,
            BidirectionalDictionary<Diagnostic, SyntaxAnnotation> diagnosticAnnotationPairs)
        {
            foreach (var diagnostic in diagnostics)
            {
                diagnosticAnnotationPairs.Add(diagnostic, new SyntaxAnnotation(annotationKind));
            }
        }
    }
}
