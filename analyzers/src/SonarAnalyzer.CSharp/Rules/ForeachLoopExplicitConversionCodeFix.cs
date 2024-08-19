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
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class ForeachLoopExplicitConversionCodeFix : SonarCodeFix
    {
        private const string Title = "Filter collection for the expected type";
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(ForeachLoopExplicitConversion.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var foreachSyntax = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<ForEachStatementSyntax>();
            if (foreachSyntax == null)
            {
                return Task.CompletedTask;
            }

            var semanticModel = context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
            var enumerableHelperType = semanticModel.Compilation.GetTypeByMetadataName(KnownType.System_Linq_Enumerable);

            if (enumerableHelperType != null)
            {
                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var newRoot = CalculateNewRoot(root, foreachSyntax, semanticModel);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }

        private static SyntaxNode CalculateNewRoot(SyntaxNode root, ForEachStatementSyntax foreachSyntax, SemanticModel semanticModel)
        {
            var collection = foreachSyntax.Expression;
            var typeName = foreachSyntax.Type.ToString();
            var invocationToAdd = GetOfTypeInvocation(typeName, collection);
            var namedTypes = semanticModel.LookupNamespacesAndTypes(foreachSyntax.SpanStart).OfType<INamedTypeSymbol>();
            var isUsingAlreadyThere = namedTypes.Any(KnownType.System_Linq_Enumerable.Matches);

            if (isUsingAlreadyThere)
            {
                return root
                    .ReplaceNode(collection, invocationToAdd)
                    .WithAdditionalAnnotations(Formatter.Annotation);
            }

            var usingDirectiveToAdd = SyntaxFactory.UsingDirective(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.IdentifierName("System"),
                    SyntaxFactory.IdentifierName("Linq")));

            var annotation = new SyntaxAnnotation("CollectionToChange");
            var newRoot = root.ReplaceNode(
                collection,
                collection.WithAdditionalAnnotations(annotation));

            var node = newRoot.GetAnnotatedNodes(annotation).First();

            var closestNamespaceWithUsing = node.AncestorsAndSelf()
                .FirstOrDefault(x => BaseNamespaceDeclarationSyntaxWrapper.IsInstance(x)
                                     && ((BaseNamespaceDeclarationSyntaxWrapper)x).Usings.Count > 0);

            if (closestNamespaceWithUsing != null)
            {
                var namespaceDeclarationWrapper = (BaseNamespaceDeclarationSyntaxWrapper)closestNamespaceWithUsing;
                var namespaceWithAdditionalUsing = namespaceDeclarationWrapper.WithUsings(namespaceDeclarationWrapper.Usings.Add(usingDirectiveToAdd));

                newRoot = newRoot.ReplaceNode(
                    namespaceDeclarationWrapper,
                    namespaceWithAdditionalUsing.SyntaxNode.WithAdditionalAnnotations(Formatter.Annotation));
            }
            else
            {
                var compilationUnit = node.FirstAncestorOrSelf<CompilationUnitSyntax>();
                newRoot = compilationUnit.AddUsings(usingDirectiveToAdd);
            }

            node = newRoot.GetAnnotatedNodes(annotation).First();
            return newRoot
                .ReplaceNode(node, invocationToAdd)
                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static InvocationExpressionSyntax GetOfTypeInvocation(string typeName, ExpressionSyntax collection)
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    collection,
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("OfType"),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList<TypeSyntax>().Add(
                                SyntaxFactory.IdentifierName(typeName))))));
        }
    }
}
