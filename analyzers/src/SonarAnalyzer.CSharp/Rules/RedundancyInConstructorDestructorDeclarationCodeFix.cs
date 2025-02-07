/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundancyInConstructorDestructorDeclarationCodeFix : SonarCodeFix
    {
        internal const string TitleRemoveBaseCall = "Remove 'base()' call";
        internal const string TitleRemoveConstructor = "Remove constructor";
        internal const string TitleRemoveDestructor = "Remove destructor";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundancyInConstructorDestructorDeclaration.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);
            if (syntaxNode is ConstructorInitializerSyntax initializer)
            {
                RegisterActionForBaseCall(context, root, initializer);
                return Task.CompletedTask;
            }

            var method = syntaxNode.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();

            if (method is ConstructorDeclarationSyntax)
            {
                RegisterActionForConstructor(context, root, method);
                return Task.CompletedTask;
            }

            if (method is DestructorDeclarationSyntax)
            {
                RegisterActionForDestructor(context, root, method);
            }

            return Task.CompletedTask;
        }

        private static void RegisterActionForDestructor(SonarCodeFixContext context, SyntaxNode root, SyntaxNode method) =>
            context.RegisterCodeFix(TitleRemoveDestructor,
                                    c =>
                                    {
                                        var newRoot = root.RemoveNode(method, SyntaxRemoveOptions.KeepNoTrivia);
                                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                                    },
                                    context.Diagnostics);

        private static void RegisterActionForConstructor(SonarCodeFixContext context, SyntaxNode root, SyntaxNode method) =>
            context.RegisterCodeFix(TitleRemoveConstructor,
                                    c =>
                                    {
                                        var newRoot = root.RemoveNode(method, SyntaxRemoveOptions.KeepNoTrivia);
                                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                                    },
                                    context.Diagnostics);

        private static void RegisterActionForBaseCall(SonarCodeFixContext context, SyntaxNode root, SyntaxNode initializer)
        {
            if (!(initializer.Parent is ConstructorDeclarationSyntax constructor))
            {
                return;
            }

            context.RegisterCodeFix(TitleRemoveBaseCall,
                                    c =>
                                    {
                                        var newRoot = RemoveInitializer(root, constructor);
                                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                                    },
                                    context.Diagnostics);
        }

        private static SyntaxNode RemoveInitializer(SyntaxNode root, ConstructorDeclarationSyntax constructor)
        {
            var annotation = new SyntaxAnnotation();
            var ctor = constructor;
            var newRoot = root;
            newRoot = newRoot.ReplaceNode(ctor, ctor.WithAdditionalAnnotations(annotation));
            ctor = GetConstructor(newRoot, annotation);
            var initializer = ctor.Initializer;

            if (RedundantInheritanceListCodeFix.HasLineEnding(constructor.ParameterList))
            {
                newRoot = newRoot.RemoveNode(initializer, SyntaxRemoveOptions.KeepNoTrivia);
                ctor = GetConstructor(newRoot, annotation);

                if (ctor.Body is {HasLeadingTrivia: true})
                {
                    var lastTrivia = ctor.Body.GetLeadingTrivia().Last();
                    var newBody = lastTrivia.IsKind(SyntaxKind.EndOfLineTrivia)
                        ? ctor.Body.WithoutLeadingTrivia()
                        : ctor.Body.WithLeadingTrivia(lastTrivia);

                    newRoot = newRoot.ReplaceNode(ctor.Body, newBody);
                }
            }
            else
            {
                var trailingTrivia = SyntaxFactory.TriviaList();
                if (initializer.HasTrailingTrivia)
                {
                    trailingTrivia = initializer.GetTrailingTrivia();
                }
                newRoot = newRoot.RemoveNode(initializer, SyntaxRemoveOptions.KeepNoTrivia);
                ctor = GetConstructor(newRoot, annotation);

                if (ctor.Body is {HasLeadingTrivia: true})
                {
                    var lastTrivia = ctor.Body.GetLeadingTrivia().Last();
                    newRoot = newRoot.ReplaceNode(ctor.Body, ctor.Body.WithLeadingTrivia(trailingTrivia.Add(lastTrivia)));
                }
                else
                {
                    if (initializer.HasTrailingTrivia)
                    {
                        newRoot = newRoot.ReplaceNode(ctor.ParameterList, ctor.ParameterList.WithTrailingTrivia(trailingTrivia));
                    }
                }
            }

            ctor = GetConstructor(newRoot, annotation);
            return newRoot.ReplaceNode(ctor, ctor.WithoutAnnotations(annotation));
        }

        private static ConstructorDeclarationSyntax GetConstructor(SyntaxNode newRoot, SyntaxAnnotation annotation) =>
            (ConstructorDeclarationSyntax)newRoot.GetAnnotatedNodes(annotation).First();
    }
}
