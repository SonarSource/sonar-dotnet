/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CheckArgumentException : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3928";
        private const string MessageFormat = "{0}";
        private const string ParameterLessConstructorMessage = "Use a constructor overloads that allows a more meaningful exception message to be provided.";
        private const string ConstructorParametersInverted = "ArgumentException constructor arguments have been inverted.";
        private const string InvalidParameterName = "The parameter name '{0}' is not declared in the argument list.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<KnownType> ArgumentExceptionTypesToCheck =
            ImmutableArray.Create(
                KnownType.System_ArgumentException,
                KnownType.System_ArgumentNullException,
                KnownType.System_ArgumentOutOfRangeException,
                KnownType.System_DuplicateWaitObjectException);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(CheckForIssue, SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

        private static void CheckForIssue(SonarSyntaxNodeReportingContext analysisContext)
        {
            var objectCreation = ObjectCreationFactory.Create(analysisContext.Node);
            var methodSymbol = objectCreation.MethodSymbol(analysisContext.SemanticModel);
            if (methodSymbol?.ContainingType == null || !methodSymbol.ContainingType.IsAny(ArgumentExceptionTypesToCheck))
            {
                return;
            }

            if (objectCreation.ArgumentList == null || objectCreation.ArgumentList.Arguments.Count == 0)
            {
                analysisContext.ReportIssue(CreateDiagnostic(Rule, objectCreation.Expression.GetLocation(), ParameterLessConstructorMessage));
                return;
            }

            var parameterAndMessage = RetrieveParameterAndMessageArgumentValue(methodSymbol, objectCreation, analysisContext.SemanticModel);

            var constructorParameterArgument = parameterAndMessage.Item1;
            var constructorMessageArgument = parameterAndMessage.Item2;

            if (!constructorParameterArgument.HasValue)
            {
                // can't check non-constant strings OR argument is not set
                return;
            }

            var methodArgumentNames = GetMethodArgumentNames(objectCreation.Expression).ToHashSet();
            if (!methodArgumentNames.Contains(TakeOnlyBeforeDot(constructorParameterArgument)))
            {
                var message = constructorMessageArgument.HasValue && methodArgumentNames.Contains(TakeOnlyBeforeDot(constructorMessageArgument))
                    ? ConstructorParametersInverted
                    : string.Format(InvalidParameterName, constructorParameterArgument.Value);
                analysisContext.ReportIssue(CreateDiagnostic(Rule, objectCreation.Expression.GetLocation(), message));
            }
        }

        private static Tuple<Optional<object>, Optional<object>> RetrieveParameterAndMessageArgumentValue(IMethodSymbol methodSymbol, IObjectCreation objectCreation, SemanticModel semanticModel)
        {
            var parameterNameValue = default(Optional<object>);
            var messageValue = default(Optional<object>);
            for (var i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var argument = objectCreation.ArgumentList.Arguments[i];
                var argumentExpression = objectCreation.ArgumentList.Arguments[i].Expression;
                var argumentName = argument.NameColon != null
                                   ? argument.NameColon.Name.Identifier.ValueText
                                   : methodSymbol.Parameters[i].MetadataName;

                if (argumentName.Equals("paramName", StringComparison.Ordinal) || argumentName.Equals("parameterName", StringComparison.Ordinal))
                {
                    parameterNameValue = semanticModel.GetConstantValue(argumentExpression);
                }
                else if (argumentName.Equals("message", StringComparison.Ordinal))
                {
                    messageValue = semanticModel.GetConstantValue(argumentExpression);
                }
            }

            return new Tuple<Optional<object>, Optional<object>>(parameterNameValue, messageValue);
        }

        private static IEnumerable<string> GetMethodArgumentNames(SyntaxNode creationSyntax)
        {
            var node = creationSyntax.AncestorsAndSelf().FirstOrDefault(ancestor =>
                ancestor is SimpleLambdaExpressionSyntax
                    or ParenthesizedLambdaExpressionSyntax
                    or AccessorDeclarationSyntax
                    or BaseMethodDeclarationSyntax
                    or IndexerDeclarationSyntax
                    or PropertyDeclarationSyntax
                    or CompilationUnitSyntax
                || LocalFunctionStatementSyntaxWrapper.IsInstance(ancestor));

            var parameterList = node switch
            {
                SimpleLambdaExpressionSyntax simpleLambda => new[] { simpleLambda.Parameter.Identifier.ValueText },
                BaseMethodDeclarationSyntax method => IdentifierNames(method.ParameterList),
                ParenthesizedLambdaExpressionSyntax lambda => IdentifierNames(lambda.ParameterList),
                AccessorDeclarationSyntax accessor => AccessorIdentifierNames(accessor),
                IndexerDeclarationSyntax indexerDeclaration => IdentifierNames(indexerDeclaration.ParameterList),
                PropertyDeclarationSyntax propertyDeclaration => ParentParameterList(propertyDeclaration),
                CompilationUnitSyntax => new[] { "args" },
                { } when LocalFunctionStatementSyntaxWrapper.IsInstance(node) => IdentifierNames(((LocalFunctionStatementSyntaxWrapper)node).ParameterList),
                _ => Enumerable.Empty<string>()
            };

            return parameterList.Union(ParentParameterList(node));

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

            static IEnumerable<string> ParentParameterList(SyntaxNode node) =>
                node?.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault() is { } baseTypeAncestor
                && RecordDeclarationSyntaxWrapper.IsInstance(baseTypeAncestor)
                && ((RecordDeclarationSyntaxWrapper)baseTypeAncestor).ParameterList is { } recordParameterList
                    ? IdentifierNames(recordParameterList)
                    : Enumerable.Empty<string>();
        }

        private static string TakeOnlyBeforeDot(Optional<object> value) =>
            (value.Value as string)?.Split('.').FirstOrDefault();
    }
}
