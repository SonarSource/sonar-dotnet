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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BeginInvokePairedWithEndInvoke : BeginInvokePairedWithEndInvokeBase<SyntaxKind, InvocationExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override SyntaxKind InvocationExpressionKind => SyntaxKind.InvocationExpression;
        protected override string CallbackParameterName => "callback";
        protected override ISet<SyntaxKind> ParentDeclarationKinds { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.AnonymousMethodExpression,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.SimpleLambdaExpression,
        }.ToImmutableHashSet();

        protected override void VisitInvocation(EndInvokeContext context) =>
            new InvocationWalker(context).SafeVisit(context.Root);

        protected override bool IsInvalidCallback(SyntaxNode callbackArg, SemanticModel semanticModel)
        {
            if (FindCallback(callbackArg, semanticModel) is { } callback)
            {
                var callbackSemanticModel = callback.EnsureCorrectSemanticModelOrDefault(semanticModel);
                return callback is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.ToString().Equals(EndInvoke)
                    ? !(callbackSemanticModel.GetSymbolInfo(callback).Symbol is IMethodSymbol)
                    : Language.Syntax.IsNullLiteral(callback) || !IsParentDeclarationWithEndInvoke(callback, callbackSemanticModel);
            }

            return false;
        }

        /// <summary>
        /// This method is looking for the callback code which can be:
        /// - in a identifier initializer (like a lambda)
        /// - passed directly as a lambda argument
        /// - passed as a new delegate instantiation (and the code can be inside the method declaration)
        /// - a mix of the above.
        /// </summary>
        private static SyntaxNode FindCallback(SyntaxNode callbackArg, SemanticModel semanticModel)
        {
            var callback = callbackArg.RemoveParentheses();
            if (callback is IdentifierNameSyntax identifier)
            {
                callback = LookupIdentifierInitializer(identifier, semanticModel);
            }

            if (callback is ObjectCreationExpressionSyntax objectCreation)
            {
                callback = objectCreation.ArgumentList.Arguments.Count == 1 ? objectCreation.ArgumentList.Arguments.Single().Expression : null;
                if (callback != null && semanticModel.GetSymbolInfo(callback).Symbol is IMethodSymbol methodSymbol)
                {
                    callback = methodSymbol.ImplementationSyntax();
                }
            }

            return callback;
        }

        private static SyntaxNode LookupIdentifierInitializer(IdentifierNameSyntax identifier, SemanticModel semantic) =>
            semantic.GetSymbolInfo(identifier).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is VariableDeclaratorSyntax variableDeclarator
                && variableDeclarator.Initializer is EqualsValueClauseSyntax equalsValueClause
                ? equalsValueClause.Value.RemoveParentheses()
                : null;

        private class InvocationWalker : SafeCSharpSyntaxWalker
        {
            private readonly EndInvokeContext context;

            public InvocationWalker(EndInvokeContext context) =>
                this.context = context;

            public override void Visit(SyntaxNode node)
            {
                if (context.Visit(node))
                {
                    base.Visit(node);
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (context.VisitInvocationExpression(node))
                {
                    base.VisitInvocationExpression(node);
                }
            }
        }
    }
}
