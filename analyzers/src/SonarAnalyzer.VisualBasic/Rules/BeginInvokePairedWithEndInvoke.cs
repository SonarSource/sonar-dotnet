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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class BeginInvokePairedWithEndInvoke : BeginInvokePairedWithEndInvokeBase<SyntaxKind, InvocationExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;
        protected override SyntaxKind InvocationExpressionKind { get; } = SyntaxKind.InvocationExpression;
        protected override string CallbackParameterName { get; } = "DelegateCallback";
        protected override ISet<SyntaxKind> ParentDeclarationKinds { get; } = new HashSet<SyntaxKind>
        {
            // Methods
            SyntaxKind.ConstructorBlock,
            SyntaxKind.FunctionBlock,
            SyntaxKind.SubBlock,
            SyntaxKind.OperatorBlock,
            // Properties
            SyntaxKind.AddHandlerAccessorBlock,
            SyntaxKind.GetAccessorBlock,
            SyntaxKind.RaiseEventAccessorBlock,
            SyntaxKind.RemoveHandlerAccessorBlock,
            SyntaxKind.SetAccessorBlock,
            // Lambdas
            SyntaxKind.MultiLineFunctionLambdaExpression,
            SyntaxKind.MultiLineSubLambdaExpression,
            SyntaxKind.SingleLineFunctionLambdaExpression,
            SyntaxKind.SingleLineSubLambdaExpression
        }.ToImmutableHashSet();

        protected override void VisitInvocation(EndInvokeContext context) =>
            new InvocationWalker(context).SafeVisit(context.Root);

        protected override bool IsInvalidCallback(SyntaxNode callbackArg, SemanticModel semanticModel) =>
            FindCallback(callbackArg, semanticModel) is { } callback
            && (Language.Syntax.IsNullLiteral(callback) || !IsParentDeclarationWithEndInvoke(callback, semanticModel));

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
                callback = objectCreation.ArgumentList.Arguments.Count == 1 ? objectCreation.ArgumentList.Arguments.Single().GetExpression() : null;
            }

            if (callback is UnaryExpressionSyntax addressOf
                && callback.IsKind(SyntaxKind.AddressOfExpression)
                && semanticModel.GetSymbolInfo(addressOf.Operand).Symbol is IMethodSymbol methodSymbol)
            {
                callback = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().Parent;
            }

            return callback;
        }

        private static SyntaxNode LookupIdentifierInitializer(IdentifierNameSyntax identifier, SemanticModel semantic) =>
            semantic.GetSymbolInfo(identifier).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is ModifiedIdentifierSyntax modifiedIdentifier
            && modifiedIdentifier.Parent is VariableDeclaratorSyntax variableDeclarator
                ? variableDeclarator.Initializer?.Value.RemoveParentheses() ?? (variableDeclarator.AsClause as AsNewClauseSyntax)?.NewExpression
                : null;

        private class InvocationWalker : SafeVisualBasicSyntaxWalker
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
