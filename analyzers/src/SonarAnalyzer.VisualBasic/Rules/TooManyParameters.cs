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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class TooManyParameters : TooManyParametersBase<SyntaxKind, ParameterListSyntax>
    {
        private static readonly ImmutableDictionary<SyntaxKind, string> NodeToDeclarationName = new Dictionary<SyntaxKind, string>
        {
            { SyntaxKind.SubNewStatement, "Constructor" },
            { SyntaxKind.FunctionStatement, "Function" },
            { SyntaxKind.SubStatement, "Sub" },
            { SyntaxKind.DelegateFunctionStatement, "Delegate" },
            { SyntaxKind.DelegateSubStatement, "Delegate" },
            { SyntaxKind.SubLambdaHeader, "Lambda" },
            { SyntaxKind.FunctionLambdaHeader, "Lambda" },
            { SyntaxKind.PropertyStatement, "Property" },
            { SyntaxKind.EventStatement, "Event" },
        }
        .ToImmutableDictionary();

        private static readonly SyntaxKind[] LambdaHeaders =
            [
                SyntaxKind.FunctionLambdaHeader,
                SyntaxKind.SubLambdaHeader
            ];

        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

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
            node.IsAnyKind(LambdaHeaders)
            || (NodeToDeclarationName.ContainsKey(node.Kind()) && VerifyCanBeChangedBySymbol(node, model));

        protected override int BaseParameterCount(SyntaxNode node, SemanticModel model) =>
            node.Parent is ConstructorBlockSyntax constructorBlock
                ? constructorBlock.Statements.Select(x => MyBaseNewParameterCount(x, model)).SingleOrDefault(x => x > 0)
                : 0;

        // Arguments forwarding a dependency-injected parameter are not counted, because dependency-injected parameters were already excluded.
        private static int MyBaseNewParameterCount(StatementSyntax statement, SemanticModel model) =>
            statement is ExpressionStatementSyntax expression
            && expression.Expression is InvocationExpressionSyntax invocation
            && invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Expression is MyBaseExpressionSyntax
            && memberAccess.Name.Identifier.Text.Equals("New", System.StringComparison.OrdinalIgnoreCase)
                ? invocation.ArgumentList?.Arguments.Count(x => !IsDependencyInjected(x.GetExpression(), model)) ?? 0   // A call without parentheses, as in MyBase.New, has no argument list
                : 0;

        private static bool IsDependencyInjected(ParameterSyntax parameter, SemanticModel model) =>
            parameter.AttributeLists.Count > 0    // Performance optimization: avoid the semantic lookup for the vast majority of parameters
            && model.GetDeclaredSymbol(parameter) is { } symbol
            && IsDependencyInjected(symbol);

        private static bool IsDependencyInjected(ExpressionSyntax expression, SemanticModel model) =>
            expression is not null   // An omitted argument, as in MyBase.New(, 42), has no expression
            && model.GetSymbolInfo(expression).Symbol is IParameterSymbol symbol
            && IsDependencyInjected(symbol);
    }
}
