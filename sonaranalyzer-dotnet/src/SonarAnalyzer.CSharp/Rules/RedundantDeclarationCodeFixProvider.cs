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

using System;
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
    public sealed class RedundantDeclarationCodeFixProvider : SonarCodeFixProvider
    {
        internal const string TitleRedundantArraySize = "Remove redundant array size";
        internal const string TitleRedundantArrayType = "Remove redundant array type";
        internal const string TitleRedundantLambdaParameterType = "Remove redundant type declaration";
        internal const string TitleRedundantExplicitDelegate = "Remove redundant explicit delegate creation";
        internal const string TitleRedundantExplicitNullable = "Remove redundant explicit nullable creation";
        internal const string TitleRedundantObjectInitializer = "Remove redundant object initializer";
        internal const string TitleRedundantDelegateParameterList = "Remove redundant parameter list";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantDeclaration.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => DocumentBasedFixAllProvider.Instance;

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

            if (!Enum.TryParse(diagnostic.Properties[RedundantDeclaration.DiagnosticTypeKey], out RedundantDeclaration.RedundancyType diagnosticType))
            {
                return TaskHelper.CompletedTask;
            }

            if (TryGetAction(syntaxNode, root, diagnosticType, context.Document, out var action))
            {
                context.RegisterCodeFix(action, context.Diagnostics);
            }

            return TaskHelper.CompletedTask;
        }

        private static bool TryGetRedundantLambdaParameterAction(SyntaxNode syntaxNode, SyntaxNode root,
            Document document, out CodeAction action)
        {
            if (!(syntaxNode.Parent?.Parent is ParameterListSyntax parameterList))
            {
                action = null;
                return false;
            }

            action = CodeAction.Create(TitleRedundantLambdaParameterType, c =>
            {
                var newParameterList = parameterList.WithParameters(
                    SyntaxFactory.SeparatedList(parameterList.Parameters.Select(p =>
                        SyntaxFactory.Parameter(p.Identifier).WithTriviaFrom(p))));
                var newRoot = root.ReplaceNode(parameterList, newParameterList);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }, TitleRedundantLambdaParameterType);
            return true;
        }

        private static bool TryGetRedundantArraySizeAction(SyntaxNode syntaxNode, SyntaxNode root,
            Document document, out CodeAction action)
        {
            if (!(syntaxNode.Parent is ArrayRankSpecifierSyntax arrayRank))
            {
                action = null;
                return false;
            }

            action = CodeAction.Create(TitleRedundantArraySize, c =>
            {
                var newArrayRankSpecifier = arrayRank.WithSizes(
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(arrayRank.Sizes.Select(s =>
                        SyntaxFactory.OmittedArraySizeExpression())));
                var newRoot = root.ReplaceNode(arrayRank, newArrayRankSpecifier);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }, TitleRedundantArraySize);
            return true;
        }

        private static bool TryGetRedundantArrayTypeAction(SyntaxNode syntaxNode, SyntaxNode root,
            Document document, out CodeAction action)
        {
            var arrayTypeSyntax = syntaxNode as ArrayTypeSyntax ?? syntaxNode.Parent as ArrayTypeSyntax;

            if (!(arrayTypeSyntax?.Parent is ArrayCreationExpressionSyntax arrayCreation))
            {
                action = null;
                return false;
            }

            action = CodeAction.Create(TitleRedundantArrayType, c =>
            {
                var implicitArrayCreation = SyntaxFactory.ImplicitArrayCreationExpression(arrayCreation.Initializer);
                var newRoot = root.ReplaceNode(arrayCreation, implicitArrayCreation);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }, TitleRedundantArrayType);
            return true;
        }

        private static bool TryGetRedundantExplicitObjectCreationAction(SyntaxNode syntaxNode, SyntaxNode root,
            Document document, RedundantDeclaration.RedundancyType diagnosticType,  out CodeAction action)
        {
            var title = diagnosticType == RedundantDeclaration.RedundancyType.ExplicitDelegate
                ? TitleRedundantExplicitDelegate
                : TitleRedundantExplicitNullable;

            if (!(syntaxNode is ObjectCreationExpressionSyntax objectCreation))
            {
                action = null;
                return false;
            }

            var newExpression = objectCreation.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
            if (newExpression == null)
            {
                action = null;
                return false;
            }

            action = CodeAction.Create(title, c =>
            {
                newExpression = newExpression.WithTriviaFrom(objectCreation);
                var newRoot = root.ReplaceNode(objectCreation, newExpression);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }, title);
            return true;
        }

        private static bool TryGetRedundantObjectInitializerAction(SyntaxNode syntaxNode, SyntaxNode root,
            Document document, out CodeAction action)
        {
            if (!(syntaxNode.Parent is ObjectCreationExpressionSyntax objectCreation))
            {
                action = null;
                return false;
            }

            action = CodeAction.Create(TitleRedundantObjectInitializer, c =>
            {
                var newObjectCreation = objectCreation.WithInitializer(null).WithAdditionalAnnotations(Formatter.Annotation);
                var newRoot = root.ReplaceNode(objectCreation, newObjectCreation);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }, TitleRedundantObjectInitializer);
            return true;
        }

        private static bool TryGetRedundantParameterTypeAction(SyntaxNode syntaxNode, SyntaxNode root,
            Document document, out CodeAction action)
        {
            if (!(syntaxNode.Parent is AnonymousMethodExpressionSyntax anonymousMethod))
            {
                action = null;
                return false;
            }

            action = CodeAction.Create(TitleRedundantDelegateParameterList, c =>
            {
                var newAnonymousMethod = anonymousMethod.WithParameterList(null);
                var newRoot = root.ReplaceNode(anonymousMethod, newAnonymousMethod);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }, TitleRedundantDelegateParameterList);
            return true;
        }

        private static bool TryGetAction(SyntaxNode syntaxNode, SyntaxNode root, RedundantDeclaration.RedundancyType diagnosticType,
            Document document, out CodeAction action)
        {
            switch (diagnosticType)
            {
                case RedundantDeclaration.RedundancyType.LambdaParameterType:
                    return TryGetRedundantLambdaParameterAction(syntaxNode, root, document, out action);
                case RedundantDeclaration.RedundancyType.ArraySize:
                    return TryGetRedundantArraySizeAction(syntaxNode, root, document, out action);
                case RedundantDeclaration.RedundancyType.ArrayType:
                    return TryGetRedundantArrayTypeAction(syntaxNode, root, document, out action);
                case RedundantDeclaration.RedundancyType.ExplicitDelegate:
                case RedundantDeclaration.RedundancyType.ExplicitNullable:
                    return TryGetRedundantExplicitObjectCreationAction(syntaxNode, root, document, diagnosticType, out action);
                case RedundantDeclaration.RedundancyType.ObjectInitializer:
                    return TryGetRedundantObjectInitializerAction(syntaxNode, root, document, out action);
                case RedundantDeclaration.RedundancyType.DelegateParameterList:
                    return TryGetRedundantParameterTypeAction(syntaxNode, root, document, out action);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}

