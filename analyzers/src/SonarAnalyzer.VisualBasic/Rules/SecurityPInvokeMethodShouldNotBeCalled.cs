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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class SecurityPInvokeMethodShouldNotBeCalled : SecurityPInvokeMethodShouldNotBeCalledBase<SyntaxKind, InvocationExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override bool IsImportFromInteropDll(IMethodSymbol symbol, SemanticModel semanticModel) =>
            base.IsImportFromInteropDll(symbol, semanticModel)
            || (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is DeclareStatementSyntax declaration
                && IsInterop(declaration.LibraryName?.StringValue(semanticModel)));

        protected override string GetMethodName(ISymbol symbol, SemanticModel semanticModel) =>
            symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is DeclareStatementSyntax declaration
            && declaration.AliasName != null
                ? declaration.AliasName.StringValue(semanticModel)
                : symbol.Name;

        protected override IMethodSymbol MethodSymbolForInvalidInvocation(SyntaxNode syntaxNode, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(syntaxNode).Symbol is IMethodSymbol methodSymbol
            && GetMethodName(methodSymbol, semanticModel) is var methodName
            && InvalidMethods.Any(x => methodName.Equals(x, StringComparison.OrdinalIgnoreCase))
                ? methodSymbol
                : null;
    }
}
