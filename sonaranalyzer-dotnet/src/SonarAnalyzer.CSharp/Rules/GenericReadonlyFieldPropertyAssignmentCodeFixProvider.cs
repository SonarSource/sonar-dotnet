/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Collections.Generic;
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
    public sealed class GenericReadonlyFieldPropertyAssignmentCodeFixProvider : SonarCodeFixProvider
    {
        internal const string TitleRemove = "Remove assignment";
        internal const string TitleAddClassConstraint = "Add reference type constraint";
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(GenericReadonlyFieldPropertyAssignment.DiagnosticId);
            }
        }

        private static readonly SyntaxAnnotation annotation = new SyntaxAnnotation(nameof(GenericReadonlyFieldPropertyAssignmentCodeFixProvider));

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var memberAccess = (MemberAccessExpressionSyntax)root.FindNode(diagnosticSpan);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var fieldSymbol = (IFieldSymbol)semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
            var typeParameterSymbol = (ITypeParameterSymbol)fieldSymbol.Type;
            var genericType = typeParameterSymbol.ContainingType;

            var classDeclarationTasks = genericType.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntaxAsync(context.CancellationToken))
                .ToList();

            await Task.WhenAll(classDeclarationTasks).ConfigureAwait(false);

            var classDeclarations = classDeclarationTasks
                .Select(task => task.Result as ClassDeclarationSyntax)
                .WhereNotNull()
                .ToList();

            if (classDeclarations.Any())
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        TitleAddClassConstraint,
                        async c =>
                        {
                            var currentSolution = context.Document.Project.Solution;
                            var mapping = GetDocumentIdClassDeclarationMapping(classDeclarations, currentSolution);

                            foreach (var classes in mapping)
                            {
                                var document = currentSolution.GetDocument(classes.Key);
                                var docRoot = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                                var newDocRoot = GetNewDocumentRoot(docRoot, typeParameterSymbol, classes);
                                currentSolution = currentSolution.WithDocumentSyntaxRoot(classes.Key, newDocRoot);
                            }

                            return currentSolution;
                        }),
                    context.Diagnostics);
            }

            var expression = memberAccess.Parent as ExpressionSyntax;
            if (!(expression?.Parent is StatementSyntax statement))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    TitleRemove,
                    c =>
                    {
                        var newRoot = root.RemoveNode(statement, SyntaxRemoveOptions.KeepNoTrivia);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);
        }

        private static MultiValueDictionary<DocumentId, ClassDeclarationSyntax> GetDocumentIdClassDeclarationMapping(
            IEnumerable<ClassDeclarationSyntax> classDeclarations, Solution currentSolution)
        {
            var mapping = new MultiValueDictionary<DocumentId, ClassDeclarationSyntax>();
            foreach (var classDeclaration in classDeclarations)
            {
                var documentId = currentSolution.GetDocument(classDeclaration.SyntaxTree).Id;
                mapping.AddWithKey(documentId, classDeclaration);
            }

            return mapping;
        }

        private static SyntaxNode GetNewDocumentRoot(SyntaxNode docRoot, ITypeParameterSymbol typeParameterSymbol,
            KeyValuePair<DocumentId, ICollection<ClassDeclarationSyntax>> classes)
        {
            var newDocRoot = docRoot.ReplaceNodes(classes.Value, (original, rewritten) => original.WithAdditionalAnnotations(annotation));
            var annotatedNodes = newDocRoot.GetAnnotatedNodes(annotation).ToList();
            while (annotatedNodes.Any())
            {
                var classDeclaration = (ClassDeclarationSyntax)annotatedNodes.First();
                var constraintClauses = GetNewConstraintClause(classDeclaration.ConstraintClauses, typeParameterSymbol.Name);
                newDocRoot = newDocRoot.ReplaceNode(
                    classDeclaration,
                    classDeclaration
                        .WithConstraintClauses(constraintClauses)
                        .WithoutAnnotations(annotation));
                annotatedNodes = newDocRoot.GetAnnotatedNodes(annotation).ToList();
            }

            return newDocRoot;
        }

        private static SyntaxList<TypeParameterConstraintClauseSyntax> GetNewConstraintClause(
            SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, string typeParameterName)
        {
            if (!constraintClauses.Any())
            {
                return constraintClauses;
            }

            var constraintList = SyntaxFactory.List<TypeParameterConstraintClauseSyntax>();

            foreach (var constraint in constraintClauses)
            {
                var currentConstraint = constraint;
                if (constraint.Name.Identifier.ValueText == typeParameterName)
                {
                    currentConstraint = currentConstraint
                        .WithConstraints(
                            currentConstraint.Constraints.Insert(0,
                                SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint)))
                        .WithAdditionalAnnotations(Formatter.Annotation);
                }
                constraintList = constraintList.Add(currentConstraint);
            }

            return constraintList;
        }
    }
}

