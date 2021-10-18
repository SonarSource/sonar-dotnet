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
using SonarAnalyzer.Wrappers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class CheckArgumentException : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3928";
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
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForIssue, SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

        private static void CheckForIssue(SyntaxNodeAnalysisContext analysisContext)
        {
            var objectCreation = ObjectCreationFactory.Create(analysisContext.Node);
            var methodSymbol = objectCreation.MethodSymbol(analysisContext.SemanticModel);
            if (methodSymbol?.ContainingType == null || !methodSymbol.ContainingType.IsAny(ArgumentExceptionTypesToCheck))
            {
                return;
            }

            if (objectCreation.ArgumentList == null || objectCreation.ArgumentList.Arguments.Count == 0)
            {
                analysisContext.ReportIssue(Diagnostic.Create(Rule, objectCreation.Expression.GetLocation(), ParameterLessConstructorMessage));
                return;
            }

            var parameterNameValue = new Optional<object>();
            var messageValue = new Optional<object>();
            for (var i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var argumentExpression = objectCreation.ArgumentList.Arguments[i].Expression;
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

            var methodArgumentNames = GetMethodArgumentNames(objectCreation.Expression).ToHashSet();
            if (!methodArgumentNames.Contains(TakeOnlyBeforeDot(parameterNameValue)))
            {
                var message = messageValue.HasValue && messageValue.Value != null && methodArgumentNames.Contains(TakeOnlyBeforeDot(messageValue))
                    ? ConstructorParametersInverted
                    : string.Format(InvalidParameterName, parameterNameValue.Value);
                analysisContext.ReportIssue(Diagnostic.Create(Rule, objectCreation.Expression.GetLocation(), message));
            }
        }

        private static IEnumerable<string> GetMethodArgumentNames(SyntaxNode creationSyntax)
        {
            var node = creationSyntax.AncestorsAndSelf().FirstOrDefault(ancestor =>
                ancestor is SimpleLambdaExpressionSyntax
                || ancestor is ParenthesizedLambdaExpressionSyntax
                || ancestor is AccessorDeclarationSyntax
                || ancestor is BaseMethodDeclarationSyntax
                || ancestor is IndexerDeclarationSyntax
                || LocalFunctionStatementSyntaxWrapper.IsInstance(ancestor));

            return node switch
            {
                SimpleLambdaExpressionSyntax simpleLambda => new[] { simpleLambda.Parameter.Identifier.ValueText },
                BaseMethodDeclarationSyntax method => IdentifierNames(method.ParameterList),
                ParenthesizedLambdaExpressionSyntax lambda => IdentifierNames(lambda.ParameterList),
                AccessorDeclarationSyntax accessor => AccessorIdentifierNames(accessor),
                IndexerDeclarationSyntax indexerDeclaration => IdentifierNames(indexerDeclaration.ParameterList),
                { } when LocalFunctionStatementSyntaxWrapper.IsInstance(node) => IdentifierNames(((LocalFunctionStatementSyntaxWrapper)node).ParameterList),
                _ => Enumerable.Empty<string>()
            };

            static IEnumerable<string> IdentifierNames(BaseParameterListSyntax parameterList) =>
                    parameterList.Parameters.Select(x => x.Identifier.ValueText);

            static IEnumerable<string> AccessorIdentifierNames(AccessorDeclarationSyntax accessor)
            {
                var arguments = new List<string>();
                if (accessor.FirstAncestorOrSelf<IndexerDeclarationSyntax>() is { } indexer)
                {
                    arguments.AddRange(IdentifierNames(indexer.ParameterList));
                }
                if (accessor.IsAnyKind(SyntaxKind.SetAccessorDeclaration, SyntaxKindEx.InitAccessorDeclaration))
                {
                    if (accessor.Parent.Parent is PropertyDeclarationSyntax propertyDeclaration)
                    {
                        arguments.Add(propertyDeclaration.Identifier.Text);
                    }
                    arguments.Add("value");
                }
                return arguments;
            }
        }

        private static string TakeOnlyBeforeDot(Optional<object> value) =>
            (value.Value as string)?.Split('.').FirstOrDefault();
    }
}
