/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using SonarAnalyzer.Helpers.CSharp;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class BeginInvokePairedWithEndInvoke : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4583";
        private const string MessageFormat = "Pair this \"BeginInvoke\" with an \"EndInvoke\".";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

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

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var semantic = c.SemanticModel;
                    var methodSymbol = GetMethodSymbol(invocation, semantic);
                    if (methodSymbol?.Name == "BeginInvoke" && IsDelegate(methodSymbol))
                    {
                        var callbackArg = GetCallbackArg(invocation);
                        if (callbackArg != null &&
                            (callbackArg.IsNullLiteral() || !CallbackMayContainEndInvoke(callbackArg, semantic)) &&
                            !ParentMethodContainsEndInvoke(invocation, semantic))
                        {
                            var location = ((SyntaxToken)invocation.GetMethodCallIdentifier()).GetLocation();
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location));
                        }
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private static bool ParentMethodContainsEndInvoke(SyntaxNode node, SemanticModel semantic)
        {
            var parentContext = node.AncestorsAndSelf()
                .FirstOrDefault(ancestor => ParentDeclarationKinds.Contains(ancestor.Kind()));
            return GetEndInvokeList(parentContext, semantic).Count > 0;
        }

        private ExpressionSyntax GetCallbackArg(InvocationExpressionSyntax invocationExpression)
        {
            var callbackArgPos = invocationExpression.ArgumentList.Arguments.Count - 2;
            var callbackArg = GetArgumentExpressionByNameOrPosition(invocationExpression, "callback", callbackArgPos)
                ?.RemoveParentheses();
            return callbackArg;
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
            if (callback.IsKind(SyntaxKind.IdentifierName))
            {
                callback = LookupIdentifierInitializer((IdentifierNameSyntax)callback, semantic);
            }

            if (callback.IsKind(SyntaxKind.ObjectCreationExpression))
            {
                callback = LookupFirstArgument((ObjectCreationExpressionSyntax)callback);
                if (callback != null && semantic.GetSymbolInfo(callback).Symbol is IMethodSymbol methodSymbol)
                {
                    callback = LookupMethodDeclaration(methodSymbol);
                }
            }

            if (callback != null && ParentDeclarationKinds.Contains(callback.Kind()))
            {
                return GetEndInvokeList(callback, semantic).Count > 0;
            }

            return true;
        }

        private static SyntaxNode LookupFirstArgument(ObjectCreationExpressionSyntax objectCreation) =>
            objectCreation.ArgumentList.Arguments.Count == 1 ? objectCreation.ArgumentList.Arguments[0].Expression : null;

        private static SyntaxNode LookupMethodDeclaration(IMethodSymbol methodSymbol) =>
            methodSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

        private static SyntaxNode LookupIdentifierInitializer(IdentifierNameSyntax identifier, SemanticModel semantic)
        {
            var declaringReference = semantic.GetSymbolInfo(identifier)
                .Symbol?.DeclaringSyntaxReferences.FirstOrDefault() ?.GetSyntax();
            if (declaringReference is VariableDeclaratorSyntax variableDeclarator
                && variableDeclarator.Initializer is EqualsValueClauseSyntax equalsValueClause)
            {
                return equalsValueClause.Value.RemoveParentheses();
            }

            return null;
        }

        private ExpressionSyntax GetArgumentExpressionByNameOrPosition(InvocationExpressionSyntax invocationExpression,
            string argumentName, int argumentPosition)
        {
            var arguments = invocationExpression.ArgumentList.Arguments;
            var argumentByName = arguments.FirstOrDefault(a => a.NameColon?.Name.Identifier.Text == argumentName);
            if (argumentByName != null)
            {
                return argumentByName.Expression;
            }

            return argumentPosition < arguments.Count ? arguments[argumentPosition].Expression : null;
        }

        private static IMethodSymbol
            GetMethodSymbol(InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel) =>
            invocationExpression.GetMethodCallIdentifier() is SyntaxToken identifier &&
            semanticModel.GetSymbolInfo(identifier.Parent).Symbol is IMethodSymbol symbol
                ? symbol
                : null;

        private static bool IsDelegate(IMethodSymbol methodSymbol) => methodSymbol.ReceiverType.Is(TypeKind.Delegate);

        private static List<InvocationExpressionSyntax> GetEndInvokeList(SyntaxNode parentContext, SemanticModel semanticModel)
        {
            var endInvokeList = new List<InvocationExpressionSyntax>();
            var walker = new InvocationExpressionWalker(parentContext, invocationExpression =>
            {
                var methodSymbol = GetMethodSymbol(invocationExpression, semanticModel);
                if (methodSymbol?.Name == "EndInvoke" && IsDelegate(methodSymbol))
                {
                    endInvokeList.Add(invocationExpression);
                }
            });
            walker.SafeVisit(parentContext);
            return endInvokeList;
        }

        private class InvocationExpressionWalker : CSharpSyntaxWalker
        {
            private readonly SyntaxNode parentContext;

            private readonly Action<InvocationExpressionSyntax> consumer;

            public InvocationExpressionWalker(SyntaxNode parentContext, Action<InvocationExpressionSyntax> consumer)
            {
                this.parentContext = parentContext;
                this.consumer = consumer;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                this.consumer.Invoke(node);
                base.VisitInvocationExpression(node);
            }

            public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) =>
                OnlyOnParent(node, () => base.VisitAnonymousMethodExpression(node));

            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node) =>
                OnlyOnParent(node, () => base.VisitConstructorDeclaration(node));

            public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node) =>
                OnlyOnParent(node, () => base.VisitDestructorDeclaration(node));

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node) =>
                OnlyOnParent(node, () => base.VisitMethodDeclaration(node));

            public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) =>
                OnlyOnParent(node, () => base.VisitParenthesizedLambdaExpression(node));

            public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) =>
                OnlyOnParent(node, () => base.VisitSimpleLambdaExpression(node));

            private void OnlyOnParent<T>(T node, Action action) where T : SyntaxNode
            {
                if (this.parentContext == node)
                {
                    action.Invoke();
                }
            }
        }
    }
}
