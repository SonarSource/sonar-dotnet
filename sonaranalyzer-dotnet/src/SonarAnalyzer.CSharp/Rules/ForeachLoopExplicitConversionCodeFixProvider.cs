/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class ForeachLoopExplicitConversionCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Filter collection for the expected type";
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(ForeachLoopExplicitConversion.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => DocumentBasedFixAllProvider.Instance;

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var foreachSyntax = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<ForEachStatementSyntax>();
            if (foreachSyntax == null)
            {
                return TaskHelper.CompletedTask;
            }

            var semanticModel = context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
            var enumerableHelperType = semanticModel.Compilation.GetTypeByMetadataName(ofTypeExtensionClass);

            if (enumerableHelperType != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        Title,
                        c =>
                        {
                            var newRoot = CalculateNewRoot(root, foreachSyntax, semanticModel);
                            return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                        }),
                    context.Diagnostics);
            }

            return TaskHelper.CompletedTask;
        }

        private const string ofTypeExtensionClass = "System.Linq.Enumerable";

        private static SyntaxNode CalculateNewRoot(SyntaxNode root, ForEachStatementSyntax foreachSyntax, SemanticModel semanticModel)
        {
            var collection = foreachSyntax.Expression;
            var typeName = foreachSyntax.Type.ToString();
            var invocationToAdd = GetOfTypeInvocation(typeName, collection);
            var namedTypes = semanticModel.LookupNamespacesAndTypes(foreachSyntax.SpanStart).OfType<INamedTypeSymbol>();
            var isUsingAlreadyThere = namedTypes.Any(nt => nt.ToDisplayString() == ofTypeExtensionClass);

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
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault(n => n.Usings.Count > 0);

            if (closestNamespaceWithUsing != null)
            {
                newRoot = newRoot.ReplaceNode(
                    closestNamespaceWithUsing,
                    closestNamespaceWithUsing.AddUsings(usingDirectiveToAdd))
                    .WithAdditionalAnnotations(Formatter.Annotation);
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

