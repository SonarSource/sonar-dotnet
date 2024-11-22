/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantDeclarationCodeFix : SonarCodeFix
    {
        internal const string TitleRedundantArraySize = "Remove redundant array size";
        internal const string TitleRedundantArrayType = "Remove redundant array type";
        internal const string TitleRedundantLambdaParameterType = "Remove redundant type declaration";
        internal const string TitleRedundantExplicitDelegate = "Remove redundant explicit delegate creation";
        internal const string TitleRedundantExplicitNullable = "Remove redundant explicit nullable creation";
        internal const string TitleRedundantObjectInitializer = "Remove redundant object initializer";
        internal const string TitleRedundantDelegateParameterList = "Remove redundant parameter list";
        internal const string TitleRedundantParameterName = "Use discard parameter";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantDeclaration.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

            if (!Enum.TryParse(diagnostic.Properties[RedundantDeclaration.DiagnosticTypeKey], out RedundantDeclaration.RedundancyType diagnosticType))
            {
                return Task.CompletedTask;
            }

            RegisterAction(syntaxNode, root, diagnosticType, context.Document, diagnostic.Properties, context);

            return Task.CompletedTask;
        }

        private static void RegisterRedundantLambdaParameterAction(SyntaxNode syntaxNode,
                                                                   SyntaxNode root,
                                                                   Document document,
                                                                   ImmutableDictionary<string, string> properties,
                                                                   SonarCodeFixContext context)
        {
            var parentExpression = syntaxNode.Parent?.Parent;
            if (parentExpression is ParenthesizedLambdaExpressionSyntax lambdaExpressionSyntax)
            {
                context.RegisterCodeFix(
                    TitleRedundantParameterName,
                    c =>
                    {
                        var parameterName = properties[RedundantDeclaration.ParameterNameKey];
                        var parameter = lambdaExpressionSyntax.ParameterList.Parameters.Single(parameter => parameter.Identifier.Text == parameterName);
                        var newRoot = root.ReplaceNode(parameter, SyntaxFactory.Parameter(SyntaxFactory.Identifier(SyntaxConstants.Discard)));

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }
            else if (parentExpression is ParameterListSyntax parameterList)
            {
                context.RegisterCodeFix(
                    TitleRedundantLambdaParameterType,
                    c =>
                    {
                        var newParameterList = parameterList.WithParameters(SyntaxFactory.SeparatedList(parameterList.Parameters.Select(p => SyntaxFactory.Parameter(p.Identifier).WithTriviaFrom(p))));
                        var newRoot = root.ReplaceNode(parameterList, newParameterList);

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }
        }

        private static void RegisterRedundantArraySizeAction(SyntaxNode syntaxNode, SyntaxNode root, Document document, SonarCodeFixContext context)
        {
            if (syntaxNode.Parent is not ArrayRankSpecifierSyntax arrayRank)
            {
                return;
            }

            context.RegisterCodeFix(
                TitleRedundantArraySize,
                c =>
                {
                    var newArrayRankSpecifier = arrayRank.WithSizes(
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(arrayRank.Sizes.Select(s =>
                            SyntaxFactory.OmittedArraySizeExpression())));
                    var newRoot = root.ReplaceNode(arrayRank, newArrayRankSpecifier);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }

        private static void RegisterRedundantArrayTypeAction(SyntaxNode syntaxNode, SyntaxNode root, Document document, SonarCodeFixContext context)
        {
            var arrayTypeSyntax = syntaxNode as ArrayTypeSyntax ?? syntaxNode.Parent as ArrayTypeSyntax;

            if (arrayTypeSyntax?.Parent is not ArrayCreationExpressionSyntax arrayCreation)
            {
                return;
            }

            context.RegisterCodeFix(
                TitleRedundantArrayType,
                c =>
                {
                    var implicitArrayCreation = SyntaxFactory.ImplicitArrayCreationExpression(arrayCreation.Initializer);
                    var newRoot = root.ReplaceNode(arrayCreation, implicitArrayCreation);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }

        private static void RegisterRedundantExplicitObjectCreationAction(SyntaxNode syntaxNode,
                                                                         SyntaxNode root,
                                                                         Document document,
                                                                         RedundantDeclaration.RedundancyType diagnosticType,
                                                                         SonarCodeFixContext context)
        {
            var title = diagnosticType == RedundantDeclaration.RedundancyType.ExplicitDelegate
                ? TitleRedundantExplicitDelegate
                : TitleRedundantExplicitNullable;

            if (syntaxNode is not ObjectCreationExpressionSyntax objectCreation)
            {
                return;
            }

            var newExpression = objectCreation.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
            if (newExpression == null)
            {
                return;
            }

            context.RegisterCodeFix(
                title,
                c =>
                {
                    newExpression = newExpression.WithTriviaFrom(objectCreation);
                    var newRoot = root.ReplaceNode(objectCreation, newExpression);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }

        private static void RegisterRedundantObjectInitializerAction(SyntaxNode syntaxNode, SyntaxNode root, Document document, SonarCodeFixContext context)
        {
            if (syntaxNode.Parent is not ObjectCreationExpressionSyntax objectCreation)
            {
                return;
            }

            context.RegisterCodeFix(
                TitleRedundantObjectInitializer,
                c =>
                {
                    var newObjectCreation = objectCreation.WithInitializer(null).WithAdditionalAnnotations(Formatter.Annotation);
                    var newRoot = root.ReplaceNode(objectCreation, newObjectCreation);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }

        private static void RegisterRedundantParameterTypeAction(SyntaxNode syntaxNode, SyntaxNode root, Document document, SonarCodeFixContext context)
        {
            if (syntaxNode.Parent is not AnonymousMethodExpressionSyntax anonymousMethod)
            {
                return;
            }

            context.RegisterCodeFix(
                TitleRedundantDelegateParameterList,
                c =>
                {
                    var newAnonymousMethod = anonymousMethod.WithParameterList(null);
                    var newRoot = root.ReplaceNode(anonymousMethod, newAnonymousMethod);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }

        private static void RegisterAction(SyntaxNode syntaxNode,
                                         SyntaxNode root,
                                         RedundantDeclaration.RedundancyType diagnosticType,
                                         Document document,
                                         ImmutableDictionary<string, string> properties,
                                         SonarCodeFixContext context)
        {
            switch (diagnosticType)
            {
                case RedundantDeclaration.RedundancyType.LambdaParameterType:
                    RegisterRedundantLambdaParameterAction(syntaxNode, root, document, properties, context);
                    break;
                case RedundantDeclaration.RedundancyType.ArraySize:
                    RegisterRedundantArraySizeAction(syntaxNode, root, document, context);
                    break;
                case RedundantDeclaration.RedundancyType.ArrayType:
                    RegisterRedundantArrayTypeAction(syntaxNode, root, document, context);
                    break;
                case RedundantDeclaration.RedundancyType.ExplicitDelegate:
                case RedundantDeclaration.RedundancyType.ExplicitNullable:
                    RegisterRedundantExplicitObjectCreationAction(syntaxNode, root, document, diagnosticType, context);
                    break;
                case RedundantDeclaration.RedundancyType.ObjectInitializer:
                    RegisterRedundantObjectInitializerAction(syntaxNode, root, document, context);
                    break;
                case RedundantDeclaration.RedundancyType.DelegateParameterList:
                    RegisterRedundantParameterTypeAction(syntaxNode, root, document, context);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
