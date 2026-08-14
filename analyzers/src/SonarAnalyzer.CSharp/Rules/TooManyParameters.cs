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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TooManyParameters : TooManyParametersBase<SyntaxKind, ParameterListSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        private static readonly ImmutableDictionary<SyntaxKind, string> NodeToDeclarationName = new Dictionary<SyntaxKind, string>
        {
            { SyntaxKind.ConstructorDeclaration, "Constructor" },
            { SyntaxKind.StructDeclaration, "Constructor" },
            { SyntaxKind.ClassDeclaration, "Constructor" },
            { SyntaxKind.MethodDeclaration, "Method" },
            { SyntaxKind.DelegateDeclaration, "Delegate" },
            { SyntaxKind.AnonymousMethodExpression, "Delegate" },
            { SyntaxKind.ParenthesizedLambdaExpression, "Lambda" },
            { SyntaxKind.SimpleLambdaExpression, "Lambda" },
            { SyntaxKindEx.LocalFunctionStatement, "Local function" }
        }.ToImmutableDictionary();

        protected override string UserFriendlyNameForNode(SyntaxNode node) =>
            NodeToDeclarationName[node.Kind()];

        protected override bool TryGetParameterCountAboveMaximum(ParameterListSyntax parameterList, SemanticModel model, out int parameterCount)
        {
            if (parameterList.Parameters.Count <= Maximum)    // Performance optimization: filtering can only lower the count, so it cannot bring it above Maximum
            {
                parameterCount = 0;
                return false;
            }

            parameterCount = parameterList.Parameters.Count(x => !IsDependencyInjected(x, model));
            return parameterCount > Maximum;
        }

        protected override bool CanBeChanged(SyntaxNode node, SemanticModel model) =>
            NodeToDeclarationName.ContainsKey(node.Kind()) && VerifyCanBeChangedBySymbol(node, model);

        protected override int BaseParameterCount(SyntaxNode node, SemanticModel model) =>
            node switch
            {
                ConstructorDeclarationSyntax ctorDeclaration => CountArguments(ctorDeclaration.Initializer?.ArgumentList, model),
                ClassDeclarationSyntax classDeclaration => CountArguments(RetrieveBasePrimaryConstructorArguments(classDeclaration), model),
                _ => 0,
            };

        protected override bool IsExtern(SyntaxNode node) =>
            node is BaseMethodDeclarationSyntax { IsExtern: true };

        // Arguments forwarding a dependency-injected parameter are not counted, because dependency-injected parameters were already excluded.
        private static int CountArguments(ArgumentListSyntax argumentList, SemanticModel model) =>
            argumentList is null
                ? 0
                : argumentList.Arguments.Count(x => !IsDependencyInjected(x.Expression, model));

        private static bool IsDependencyInjected(ParameterSyntax parameter, SemanticModel model) =>
            parameter.AttributeLists.Count > 0    // Performance optimization: avoid the semantic lookup for the vast majority of parameters
            && model.GetDeclaredSymbol(parameter) is { } symbol
            && IsDependencyInjected(symbol);

        private static bool IsDependencyInjected(ExpressionSyntax expression, SemanticModel model) =>
            model.GetSymbolInfo(expression).Symbol is IParameterSymbol symbol
            && IsDependencyInjected(symbol);

        private static ArgumentListSyntax RetrieveBasePrimaryConstructorArguments(ClassDeclarationSyntax node)
        {
            var type = node.BaseList?.Types.FirstOrDefault();
            return PrimaryConstructorBaseTypeSyntaxWrapper.IsInstance(type)
                ? ((PrimaryConstructorBaseTypeSyntaxWrapper)type).ArgumentList
                : null;
        }
    }
}
