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

using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.CSharp.Rules;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class GenericReadonlyFieldPropertyAssignmentCodeFix : SonarCodeFix
{
    internal const string TitleRemove = "Remove assignment";
    internal const string TitleAddClassConstraint = "Add reference type constraint";
    private static readonly SyntaxAnnotation Annotation = new(nameof(GenericReadonlyFieldPropertyAssignmentCodeFix));

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(GenericReadonlyFieldPropertyAssignment.DiagnosticId);

    protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
        var memberExpression = node switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
            MemberBindingExpressionSyntax { Parent.Parent: ConditionalAccessExpressionSyntax conditionalAccess } => conditionalAccess.Expression,
            _ => null
        };
        if (memberExpression is null)
        {
            return;
        }

        var model = await context.Document.GetSemanticModelAsync(context.Cancel).ConfigureAwait(false);
        var fieldSymbol = (IFieldSymbol)model.GetSymbolInfo(memberExpression).Symbol;
        var typeParameterSymbol = (ITypeParameterSymbol)fieldSymbol.Type;
        var genericType = typeParameterSymbol.ContainingType;

        var classDeclarationTasks = genericType.DeclaringSyntaxReferences
            .Select(x => x.GetSyntaxAsync(context.Cancel))
            .ToList();

        var taskResults = await Task.WhenAll(classDeclarationTasks).ConfigureAwait(false);

        var classDeclarations = taskResults.OfType<ClassDeclarationSyntax>().ToList();

        if (classDeclarations.Any())
        {
            context.RegisterCodeFix(
                TitleAddClassConstraint,
                async x =>
                {
                    var currentSolution = context.Document.Project.Solution;
                    var mapping = DocumentIdClassDeclarationMapping(classDeclarations, currentSolution);

                    foreach (var classes in mapping)
                    {
                        var document = currentSolution.GetDocument(classes.Key);
                        var docRoot = await document.GetSyntaxRootAsync(context.Cancel).ConfigureAwait(false);
                        var newDocRoot = NewDocumentRoot(docRoot, typeParameterSymbol, classes);
                        currentSolution = currentSolution.WithDocumentSyntaxRoot(classes.Key, newDocRoot);
                    }

                    return currentSolution;
                },
                context.Diagnostics);
        }

        if (node is { Parent: ExpressionSyntax { Parent: StatementSyntax statement } })
        {
            context.RegisterCodeFix(
                TitleRemove,
                x =>
                {
                    var newRoot = root.RemoveNode(statement, SyntaxRemoveOptions.KeepNoTrivia);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }
    }

    private static MultiValueDictionary<DocumentId, ClassDeclarationSyntax> DocumentIdClassDeclarationMapping(IEnumerable<ClassDeclarationSyntax> classDeclarations, Solution currentSolution)
    {
        var mapping = new MultiValueDictionary<DocumentId, ClassDeclarationSyntax>();
        foreach (var classDeclaration in classDeclarations)
        {
            var documentId = currentSolution.GetDocument(classDeclaration.SyntaxTree).Id;
            mapping.AddWithKey(documentId, classDeclaration);
        }

        return mapping;
    }

    private static SyntaxNode NewDocumentRoot(SyntaxNode docRoot, ITypeParameterSymbol typeParameterSymbol, KeyValuePair<DocumentId, ICollection<ClassDeclarationSyntax>> classes)
    {
        var newDocRoot = docRoot.ReplaceNodes(classes.Value, (_, rewritten) => rewritten.WithAdditionalAnnotations(Annotation));
        var annotatedNodes = newDocRoot.GetAnnotatedNodes(Annotation).ToList();
        while (annotatedNodes.Any())
        {
            var classDeclaration = (ClassDeclarationSyntax)annotatedNodes[0];
            var constraintClauses = NewConstraintClause(classDeclaration.ConstraintClauses, typeParameterSymbol.Name);
            newDocRoot = newDocRoot.ReplaceNode(classDeclaration, classDeclaration.WithConstraintClauses(constraintClauses).WithoutAnnotations(Annotation));
            annotatedNodes = newDocRoot.GetAnnotatedNodes(Annotation).ToList();
        }

        return newDocRoot;
    }

    private static SyntaxList<TypeParameterConstraintClauseSyntax> NewConstraintClause(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, string typeParameterName)
    {
        var constraintList = SyntaxFactory.List<TypeParameterConstraintClauseSyntax>();
        foreach (var constraint in constraintClauses)
        {
            var currentConstraint = constraint;
            if (currentConstraint.Name.Identifier.ValueText == typeParameterName && !currentConstraint.Constraints.AnyOfKind(SyntaxKind.ClassConstraint))
            {
                currentConstraint = currentConstraint
                    .WithConstraints(currentConstraint.Constraints.Insert(0, SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint)))
                    .WithAdditionalAnnotations(Formatter.Annotation);
            }
            constraintList = constraintList.Add(currentConstraint);
        }
        return constraintList;
    }
}
