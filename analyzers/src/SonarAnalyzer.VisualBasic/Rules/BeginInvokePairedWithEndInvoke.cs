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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class BeginInvokePairedWithEndInvoke : BeginInvokePairedWithEndInvokeBase<SyntaxKind, InvocationExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;
        protected override SyntaxKind InvocationExpressionKind { get; } = SyntaxKind.InvocationExpression;
        protected override string CallbackParameterName { get; } = "DelegateCallback";
        protected override ISet<SyntaxKind> ParentDeclarationKinds { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.CompilationUnit,
            // Types
            SyntaxKind.ClassBlock,
            SyntaxKind.StructureBlock,
            SyntaxKind.ModuleBlock,
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

        private class InvocationWalker : VisualBasicSyntaxWalker
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
