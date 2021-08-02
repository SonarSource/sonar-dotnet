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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class BeginInvokePairedWithEndInvoke : BeginInvokePairedWithEndInvokeBase<SyntaxKind, InvocationExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override SyntaxKind InvocationExpressionKind => SyntaxKind.InvocationExpression;
        protected override string CallbackParameterName => "callback";
        protected override ISet<SyntaxKind> ParentDeclarationKinds { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.AnonymousMethodExpression,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.CompilationUnit,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
        }.ToImmutableHashSet();

        protected override void VisitInvocation(EndInvokeContext context) =>
            new InvocationWalker(context).SafeVisit(context.Root);

        protected override bool IsInvalidCallback(SyntaxNode callbackArg, SemanticModel semanticModel)
        {
            if (FindCallback(callbackArg, semanticModel) is { } callback)
            {
                return callback is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.ToString().Equals(EndInvoke)
                    ? !(semanticModel.GetSymbolInfo(callback).Symbol is IMethodSymbol)
                    : Language.Syntax.IsNullLiteral(callback) || !IsParentDeclarationWithEndInvoke(callback, semanticModel);
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
                    callback = methodSymbol.PartialImplementationPart?.DeclaringSyntaxReferences.First().GetSyntax()
                        ?? methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                }
            }

            return callback;
        }

        private static SyntaxNode LookupIdentifierInitializer(IdentifierNameSyntax identifier, SemanticModel semantic) =>
            semantic.GetSymbolInfo(identifier).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is VariableDeclaratorSyntax variableDeclarator
                && variableDeclarator.Initializer is EqualsValueClauseSyntax equalsValueClause
                ? equalsValueClause.Value.RemoveParentheses()
                : null;

        private class InvocationWalker : CSharpSyntaxWalker
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
