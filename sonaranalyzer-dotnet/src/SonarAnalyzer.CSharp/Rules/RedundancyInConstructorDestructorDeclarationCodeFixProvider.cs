/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundancyInConstructorDestructorDeclarationCodeFixProvider : SonarCodeFixProvider
    {
        internal const string TitleRemoveBaseCall = "Remove 'base()' call";
        internal const string TitleRemoveConstructor = "Remove constructor";
        internal const string TitleRemoveDestructor = "Remove destructor";

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(RedundancyInConstructorDestructorDeclaration.DiagnosticId);
            }
        }
        public override FixAllProvider GetFixAllProvider() => DocumentBasedFixAllProvider.Instance;

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);
            if (syntaxNode is ConstructorInitializerSyntax initializer)
            {
                RegisterActionForBaseCall(context, root, initializer);
                return TaskHelper.CompletedTask;
            }

            var method = syntaxNode.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();

            if (method is ConstructorDeclarationSyntax)
            {
                RegisterActionForConstructor(context, root, method);
                return TaskHelper.CompletedTask;
            }

            if (method is DestructorDeclarationSyntax)
            {
                RegisterActionForDestructor(context, root, method);
            }

            return TaskHelper.CompletedTask;
        }

        private static void RegisterActionForDestructor(CodeFixContext context, SyntaxNode root, BaseMethodDeclarationSyntax method)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    TitleRemoveDestructor,
                    c =>
                    {
                        var newRoot = root.RemoveNode(
                            method,
                            SyntaxRemoveOptions.KeepNoTrivia);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    TitleRemoveDestructor),
                context.Diagnostics);
        }

        private static void RegisterActionForConstructor(CodeFixContext context, SyntaxNode root, BaseMethodDeclarationSyntax method)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    TitleRemoveConstructor,
                    c =>
                    {
                        var newRoot = root.RemoveNode(
                            method,
                            SyntaxRemoveOptions.KeepNoTrivia);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    TitleRemoveConstructor),
                context.Diagnostics);
        }

        private static void RegisterActionForBaseCall(CodeFixContext context, SyntaxNode root, ConstructorInitializerSyntax initializer)
        {
            if (!(initializer.Parent is ConstructorDeclarationSyntax constructor))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    TitleRemoveBaseCall,
                    c =>
                    {
                        var newRoot = RemoveInitializer(root, constructor);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    TitleRemoveBaseCall),
                context.Diagnostics);
        }

        public static SyntaxNode RemoveInitializer(SyntaxNode root, ConstructorDeclarationSyntax constructor)
        {
            var annotation = new SyntaxAnnotation();
            var ctor = constructor;
            var newRoot = root;
            newRoot = newRoot.ReplaceNode(ctor, ctor.WithAdditionalAnnotations(annotation));
            ctor = GetConstructor(newRoot, annotation);
            var initializer = ctor.Initializer;

            if (RedundantInheritanceListCodeFixProvider.HasLineEnding(constructor.ParameterList))
            {
                newRoot = newRoot.RemoveNode(initializer, SyntaxRemoveOptions.KeepNoTrivia);
                ctor = GetConstructor(newRoot, annotation);

                if (ctor.Body != null &&
                    ctor.Body.HasLeadingTrivia)
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

                if (ctor.Body != null &&
                    ctor.Body.HasLeadingTrivia)
                {
                    var lastTrivia = ctor.Body.GetLeadingTrivia().Last();
                    newRoot = newRoot.ReplaceNode(
                        ctor.Body,
                        ctor.Body.WithLeadingTrivia(trailingTrivia.Add(lastTrivia)));
                }
                else
                {
                    if (initializer.HasTrailingTrivia)
                    {
                        newRoot = newRoot.ReplaceNode(ctor, ctor.WithTrailingTrivia(trailingTrivia));
                    }
                }
            }

            ctor = GetConstructor(newRoot, annotation);
            return newRoot.ReplaceNode(ctor, ctor.WithoutAnnotations(annotation));
        }

        private static ConstructorDeclarationSyntax GetConstructor(SyntaxNode newRoot, SyntaxAnnotation annotation)
        {
            return (ConstructorDeclarationSyntax)newRoot.GetAnnotatedNodes(annotation).First();
        }
    }
}

