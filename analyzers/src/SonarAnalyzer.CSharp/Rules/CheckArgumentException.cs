/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class CheckArgumentException : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3928";
        private const string MessageFormat = "{0}";
        private const string ParameterLessConstructorMessage = "Use a constructor overloads that allows a more meaningful exception message to be provided.";
        private const string ConstructorParametersInverted = "ArgumentException constructor arguments have been inverted.";
        private const string InvalidParameterName = "The parameter name '{0}' is not declared in the argument list.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<KnownType> ArgumentExceptionTypesToCheck =
            ImmutableArray.Create(
                KnownType.System_ArgumentException,
                KnownType.System_ArgumentNullException,
                KnownType.System_ArgumentOutOfRangeException,
                KnownType.System_DuplicateWaitObjectException);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForIssue, SyntaxKind.ObjectCreationExpression);

        private static void CheckForIssue(SyntaxNodeAnalysisContext analysisContext)
        {
            var objectCreationSyntax = (ObjectCreationExpressionSyntax)analysisContext.Node;
            var methodSymbol = analysisContext.SemanticModel.GetSymbolInfo(objectCreationSyntax).Symbol as IMethodSymbol;
            if (methodSymbol?.ContainingType == null || !methodSymbol.ContainingType.IsAny(ArgumentExceptionTypesToCheck))
            {
                return;
            }

            if (objectCreationSyntax.ArgumentList.Arguments.Count == 0)
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, objectCreationSyntax.GetLocation(), ParameterLessConstructorMessage));
                return;
            }

            var parameterNameValue = new Optional<object>();
            var messageValue = new Optional<object>();
            for (var i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var argumentExpression = objectCreationSyntax.ArgumentList.Arguments[i].Expression;
                if (methodSymbol.Parameters[i].MetadataName == "paramName" || methodSymbol.Parameters[i].MetadataName == "parameterName")
                {
                    parameterNameValue = analysisContext.SemanticModel.GetConstantValue(argumentExpression);
                }
                else if (methodSymbol.Parameters[i].MetadataName == "message")
                {
                    messageValue = analysisContext.SemanticModel.GetConstantValue(argumentExpression);
                }
                else
                {
                    // do nothing
                }
            }

            if (!parameterNameValue.HasValue)
            {
                // can't check non-constant strings OR argument is not set
                return;
            }

            var methodArgumentNames = GetMethodArgumentNames(objectCreationSyntax).ToHashSet();
            if (!methodArgumentNames.Contains(TakeOnlyBeforeDot(parameterNameValue)))
            {
                var message = messageValue.HasValue && messageValue.Value != null && methodArgumentNames.Contains(TakeOnlyBeforeDot(messageValue))
                    ? ConstructorParametersInverted
                    : string.Format(InvalidParameterName, parameterNameValue.Value);
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, objectCreationSyntax.GetLocation(), message));
            }
        }

        private static IEnumerable<string> GetMethodArgumentNames(ObjectCreationExpressionSyntax creationSyntax)
        {
            var node = creationSyntax.AncestorsAndSelf().FirstOrDefault(ancestor =>
                ancestor is SimpleLambdaExpressionSyntax
                || ancestor is ParenthesizedLambdaExpressionSyntax
                || ancestor is AccessorDeclarationSyntax
                || ancestor is BaseMethodDeclarationSyntax
                || ancestor is IndexerDeclarationSyntax
                || LocalFunctionStatementSyntaxWrapper.IsInstance(ancestor));

            if (node is SimpleLambdaExpressionSyntax simpleLambda)
            {
                return new[] { simpleLambda.Parameter.Identifier.ValueText };
            }
            else if (node is BaseMethodDeclarationSyntax method)
            {
                return IdentifierNames(method.ParameterList);
            }
            else if (node is ParenthesizedLambdaExpressionSyntax lambda)
            {
                return IdentifierNames(lambda.ParameterList);
            }
            else if (node is AccessorDeclarationSyntax accessor)
            {
                var arguments = new List<string>();
                if (node.FirstAncestorOrSelf<IndexerDeclarationSyntax>() is { } indexer)
                {
                    arguments.AddRange(IdentifierNames(indexer.ParameterList));
                }
                if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
                {
                    arguments.Add("value");
                }

                return arguments;
            }
            else if (node is IndexerDeclarationSyntax indexerDeclaration)
            {
                return IdentifierNames(indexerDeclaration.ParameterList);
            }
            else if (LocalFunctionStatementSyntaxWrapper.IsInstance(node))
            {
                return IdentifierNames(((LocalFunctionStatementSyntaxWrapper)node).ParameterList);
            }
            else
            {
                return Enumerable.Empty<string>();
            }

            static IEnumerable<string> IdentifierNames(BaseParameterListSyntax parameterList) =>
                parameterList.Parameters.Select(x => x.Identifier.ValueText);
        }

        private static string TakeOnlyBeforeDot(Optional<object> value) =>
            (value.Value as string)?.Split('.').FirstOrDefault();
    }
}
