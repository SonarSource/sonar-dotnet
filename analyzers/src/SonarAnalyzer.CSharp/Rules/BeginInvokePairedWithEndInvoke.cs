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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class BeginInvokePairedWithEndInvoke : BeginInvokePairedWithEndInvokeBase
    {
        private const string BeginInvoke = "BeginInvoke";

        private static readonly ISet<SyntaxKind> ParentDeclarationKinds = new HashSet<SyntaxKind>
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

        public BeginInvokePairedWithEndInvoke() : base(RspecStrings.ResourceManager) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (invocation.Expression.ToStringContains(BeginInvoke)
                        && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                        && methodSymbol.Name == BeginInvoke
                        && IsDelegate(methodSymbol)
                        && new CSharpMethodParameterLookup(invocation.ArgumentList, methodSymbol).TryGetSyntax("callback", out var callbackArgs)
                        && !CallbackMayContainEndInvoke(callbackArgs.Single().Expression, c.SemanticModel)
                        && !ParentMethodContainsEndInvoke(invocation, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, ((SyntaxToken)invocation.GetMethodCallIdentifier()).GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression);

        private static bool ParentMethodContainsEndInvoke(SyntaxNode node, SemanticModel semantic)
        {
            var parentContext = node.AncestorsAndSelf().FirstOrDefault(ancestor => ParentDeclarationKinds.Contains(ancestor.Kind()));
            return ContainsEndInvoke(parentContext, semantic);
        }

        /// <summary>
        /// This method is looking for the callback code which can be:
        /// - in a identifier initializer (like a lambda)
        /// - passed directly as a lambda argument
        /// - passed as a new delegate instantiation (and the code can be inside the method declaration)
        /// - a mix of the above
        /// </summary>
        /// <returns>
        /// - false if callback code has been resolved and does not contain "EndInvoke",
        /// - true if callback code contains "EndInvoke" or callback code has not been resolved.
        /// </returns>
        private static bool CallbackMayContainEndInvoke(SyntaxNode callbackArg, SemanticModel semantic)
        {
            var callback = callbackArg.RemoveParentheses();
            if (callback.IsNullLiteral())
            {
                return false;
            }

            if (callback.IsKind(SyntaxKind.IdentifierName))
            {
                callback = LookupIdentifierInitializer((IdentifierNameSyntax)callback, semantic);
            }

            if (callback is ObjectCreationExpressionSyntax objectCreation)
            {
                callback = objectCreation.ArgumentList.Arguments.Count == 1 ? objectCreation.ArgumentList.Arguments.Single().Expression : null;
                if (callback != null && semantic.GetSymbolInfo(callback).Symbol is IMethodSymbol methodSymbol)
                {
                    callback = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                }
            }

            return callback == null
                || !ParentDeclarationKinds.Contains(callback.Kind())
                || ContainsEndInvoke(callback, semantic);
        }

        private static SyntaxNode LookupIdentifierInitializer(IdentifierNameSyntax identifier, SemanticModel semantic) =>
            semantic.GetSymbolInfo(identifier).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is VariableDeclaratorSyntax variableDeclarator
                && variableDeclarator.Initializer is EqualsValueClauseSyntax equalsValueClause
                ? equalsValueClause.Value.RemoveParentheses()
                : null;

        private static bool IsDelegate(IMethodSymbol methodSymbol) =>
            methodSymbol.ReceiverType.Is(TypeKind.Delegate);

        private static bool ContainsEndInvoke(SyntaxNode node, SemanticModel semanticModel) =>
            new InvocationWalker(node, semanticModel).Walk();

        private class InvocationWalker : CSharpSyntaxWalker
        {
            private static readonly SyntaxKind[] VisitOnlyOnParent = new[]
            {
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.SimpleLambdaExpression
            };

            private readonly SyntaxNode root;
            private readonly SemanticModel semanticModel;
            private bool containsEndInvoke;

            public InvocationWalker(SyntaxNode root, SemanticModel semanticModel)
            {
                this.root = root;
                this.semanticModel = semanticModel;
            }

            public bool Walk()
            {
                this.SafeVisit(root);
                return containsEndInvoke;
            }

            public override void Visit(SyntaxNode node)
            {
                if (!containsEndInvoke  // Stop visiting once we found it
                    && (node == root || !node.IsAnyKind(VisitOnlyOnParent)))
                {
                    base.Visit(node);
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.Name == "EndInvoke"
                    && IsDelegate(methodSymbol))
                {
                    containsEndInvoke = true;
                    return;
                }
                base.VisitInvocationExpression(node);
            }
        }
    }
}
