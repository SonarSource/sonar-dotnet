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

namespace SonarAnalyzer.CSharp.Core.Wrappers;

public static class MethodDeclarationFactory
{
    public static IMethodDeclaration Create(SyntaxNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        else if (LocalFunctionStatementSyntaxWrapper.IsInstance(node))
        {
            return new LocalFunctionStatementAdapter((LocalFunctionStatementSyntaxWrapper)node);
        }
        else if (node is MethodDeclarationSyntax method)
        {
            return new MethodDeclarationSyntaxAdapter(method);
        }
        else
        {
            throw new InvalidOperationException("Unexpected type: " + node.GetType().Name);
        }
    }

    private sealed class LocalFunctionStatementAdapter : IMethodDeclaration
    {
        private readonly LocalFunctionStatementSyntaxWrapper syntaxWrapper;

        public BlockSyntax Body => syntaxWrapper.Body;
        public ArrowExpressionClauseSyntax ExpressionBody => syntaxWrapper.ExpressionBody;
        public SyntaxToken Identifier => syntaxWrapper.Identifier;
        public ParameterListSyntax ParameterList => syntaxWrapper.ParameterList;
        public TypeParameterListSyntax TypeParameterList => syntaxWrapper.TypeParameterList;
        public bool HasImplementation => Body is not null || ExpressionBody is not null;
        public bool IsLocal => true;
        public TypeSyntax ReturnType => syntaxWrapper.ReturnType;

        public LocalFunctionStatementAdapter(LocalFunctionStatementSyntaxWrapper syntaxWrapper) =>
            this.syntaxWrapper = syntaxWrapper;
    }

    private sealed class MethodDeclarationSyntaxAdapter : IMethodDeclaration
    {
        private readonly MethodDeclarationSyntax declarationSyntax;

        public BlockSyntax Body => declarationSyntax.Body;
        public ArrowExpressionClauseSyntax ExpressionBody => declarationSyntax.ExpressionBody;
        public SyntaxToken Identifier => declarationSyntax.Identifier;
        public ParameterListSyntax ParameterList => declarationSyntax.ParameterList;
        public TypeParameterListSyntax TypeParameterList => declarationSyntax.TypeParameterList;
        public bool HasImplementation => Body is not null || ExpressionBody is not null;
        public bool IsLocal => false;
        public TypeSyntax ReturnType => declarationSyntax.ReturnType;

        public MethodDeclarationSyntaxAdapter(MethodDeclarationSyntax declarationSyntax) =>
            this.declarationSyntax = declarationSyntax;
    }
}
