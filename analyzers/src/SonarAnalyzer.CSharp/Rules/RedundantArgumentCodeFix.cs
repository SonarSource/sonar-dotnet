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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using NodeAndSymbol = SonarAnalyzer.Core.Common.NodeAndSymbol<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax, Microsoft.CodeAnalysis.IParameterSymbol>;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantArgumentCodeFix : SonarCodeFix
    {
        internal const string TitleRemove = "Remove redundant arguments";
        internal const string TitleRemoveWithNameAdditions = "Remove redundant arguments with adding named arguments";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantArgument.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var invocation = GetInvocation(root, diagnostic.Location.SourceSpan);
            if (invocation == null)
            {
                return;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.Cancel).ConfigureAwait(false);
            var methodParameterLookup = new CSharpMethodParameterLookup(invocation, semanticModel);
            var argumentMappings = methodParameterLookup.GetAllArgumentParameterMappings().ToList();

            var methodSymbol = methodParameterLookup.MethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            var argumentsWithDefaultValues = new List<ArgumentSyntax>();
            var argumentsCanBeRemovedWithoutNamed = new List<ArgumentSyntax>();
            var canBeRemovedWithoutNamed = true;

            var reversedMappings = new List<NodeAndSymbol>(argumentMappings);
            reversedMappings.Reverse();
            foreach (var argumentMapping in reversedMappings)
            {
                var argument = argumentMapping.Node;

                if (RedundantArgument.ArgumentHasDefaultValue(argumentMapping, semanticModel))
                {
                    argumentsWithDefaultValues.Add(argument);

                    if (canBeRemovedWithoutNamed)
                    {
                        argumentsCanBeRemovedWithoutNamed.Add(argument);
                    }
                }
                else
                {
                    if (argument.NameColon == null)
                    {
                        canBeRemovedWithoutNamed = false;
                    }
                }
            }

            if (argumentsCanBeRemovedWithoutNamed.Any())
            {
                context.RegisterCodeFix(
                    TitleRemove,
                    c => RemoveArgumentsAsync(context.Document, argumentsCanBeRemovedWithoutNamed, c),
                    context.Diagnostics);
            }

            var cannotBeRemoved = argumentsWithDefaultValues.Except(argumentsCanBeRemovedWithoutNamed);
            if (cannotBeRemoved.Any())
            {
                context.RegisterCodeFix(
                    TitleRemoveWithNameAdditions,
                    c => RemoveArgumentsAndAddNecessaryNamesAsync(context.Document, invocation.ArgumentList, argumentMappings, argumentsWithDefaultValues, semanticModel, c),
                    context.Diagnostics);
            }
        }

        private static async Task<Document> RemoveArgumentsAndAddNecessaryNamesAsync(Document document,
                                                                                     ArgumentListSyntax argumentList,
                                                                                     IEnumerable<NodeAndSymbol> argumentMappings,
                                                                                     List<ArgumentSyntax> argumentsToRemove,
                                                                                     SemanticModel semanticModel,
                                                                                     CancellationToken cancel)
        {
            var root = await document.GetSyntaxRootAsync(cancel).ConfigureAwait(false);
            var newArgumentList = SyntaxFactory.ArgumentList();
            var alreadyRemovedOne = false;

            foreach (var argumentMapping in argumentMappings
                .Where(argumentMapping => !argumentMapping.Symbol.IsParams))
            {
                var argument = argumentMapping.Node;
                if (argumentsToRemove.Contains(argument))
                {
                    alreadyRemovedOne = true;
                    continue;
                }

                newArgumentList = AddArgument(newArgumentList, argumentMapping.Symbol.Name, argument, alreadyRemovedOne);
            }

            var paramsArguments = argumentMappings
                .Where(mapping => mapping.Symbol.IsParams)
                .ToList();

            if (paramsArguments.Any())
            {
                newArgumentList = AddParamsArguments(semanticModel, paramsArguments, newArgumentList);
            }

            var newRoot = root.ReplaceNode(argumentList, newArgumentList);
            return document.WithSyntaxRoot(newRoot);
        }

        private static ArgumentListSyntax AddArgument(ArgumentListSyntax argumentList, string parameterName,
            ArgumentSyntax argument, bool alreadyRemovedOne)
        {
            return alreadyRemovedOne
                ? argumentList.AddArguments(
                    SyntaxFactory.Argument(
                        SyntaxFactory.NameColon(SyntaxFactory.IdentifierName(parameterName)),
                        argument.RefOrOutKeyword,
                        argument.Expression))
                : argumentList.AddArguments(argument);
        }

        private static ArgumentListSyntax AddParamsArguments(SemanticModel semanticModel, IEnumerable<NodeAndSymbol> paramsArguments, ArgumentListSyntax argumentList)
        {
            var firstParamsMapping = paramsArguments.First();
            var firstParamsArgument = firstParamsMapping.Node;
            var paramsParameter = firstParamsMapping.Symbol;

            if (firstParamsArgument.NameColon != null)
            {
                return argumentList.AddArguments(firstParamsArgument);
            }

            if (paramsArguments.Count() == 1 &&
                paramsParameter.Type.Equals(
                    semanticModel.GetTypeInfo(firstParamsArgument.Expression).Type))
            {
                return argumentList.AddArguments(
                    SyntaxFactory.Argument(
                        SyntaxFactory.NameColon(
                            SyntaxFactory.IdentifierName(paramsParameter.Name)),
                        firstParamsArgument.RefOrOutKeyword,
                        firstParamsArgument.Expression));
            }

            return argumentList.AddArguments(
                SyntaxFactory.Argument(
                    SyntaxFactory.NameColon(
                        SyntaxFactory.IdentifierName(paramsParameter.Name)),
                    SyntaxFactory.Token(SyntaxKind.None),
                    SyntaxFactory.ImplicitArrayCreationExpression(
                        SyntaxFactory.InitializerExpression(
                            SyntaxKind.ArrayInitializerExpression,
                            SyntaxFactory.SeparatedList(paramsArguments.Select(arg => arg.Node.Expression))))));
        }

        private static async Task<Document> RemoveArgumentsAsync(Document document, IEnumerable<ArgumentSyntax> arguments, CancellationToken cancel)
        {
            var root = await document.GetSyntaxRootAsync(cancel).ConfigureAwait(false);
            var newRoot = root.RemoveNodes(arguments, SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.AddElasticMarker);
            return document.WithSyntaxRoot(newRoot);
        }

        private static InvocationExpressionSyntax GetInvocation(SyntaxNode root, TextSpan diagnosticSpan)
        {
            var argumentSyntax = root.FindNode(diagnosticSpan) as ArgumentSyntax;

            return argumentSyntax?.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        }
    }
}
