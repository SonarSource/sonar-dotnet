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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class BeginInvokePairedWithEndInvokeBase<TSyntaxKind, TInvocationExpressionSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TInvocationExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S4583";
        private const string MessageFormat = "Pair this \"BeginInvoke\" with an \"EndInvoke\".";
        private const string BeginInvoke = "BeginInvoke";
        private const string EndInvoke = "EndInvoke";

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract TSyntaxKind InvocationExpressionKind { get; }
        protected abstract ISet<TSyntaxKind> ParentDeclarationKinds { get; }
        protected abstract string CallbackParameterName { get; }
        protected abstract void VisitInvocation(EndInvokeContext context);
        protected abstract SyntaxNode FindCallback(SyntaxNode callbackArg, SemanticModel semanticModel);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected BeginInvokePairedWithEndInvokeBase(System.Resources.ResourceManager rspecResources) =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, c =>
            {
                var invocation = (TInvocationExpressionSyntax)c.Node;
                if (Language.Syntax.NodeExpression(invocation).ToStringContains(BeginInvoke)
                    && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.Name == BeginInvoke
                    && IsDelegate(methodSymbol)
                    && methodSymbol.Parameters.SingleOrDefault(x => x.Name == CallbackParameterName) is { } parameter
                    && Language.MethodParameterLookup(invocation, methodSymbol).TryGetNonParamsSyntax(parameter, out var callbackArg)
                    && IsInvalidCallback(callbackArg, c.SemanticModel)
                    && !ParentMethodContainsEndInvoke(invocation, c.SemanticModel))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, Language.Syntax.InvocationIdentifier(invocation).Value.GetLocation()));
                }
            }, InvocationExpressionKind);

        protected static bool IsDelegate(IMethodSymbol methodSymbol) =>
            methodSymbol.ReceiverType.Is(TypeKind.Delegate);

        /// <returns>
        /// - true if callback code has been resolved and does not contain "EndInvoke".
        /// - false if callback code contains "EndInvoke" or callback code has not been resolved.
        /// </returns>
        private bool IsInvalidCallback(SyntaxNode callbackArg, SemanticModel semanticModel) =>
            FindCallback(callbackArg, semanticModel) is { } callback
            && (Language.Syntax.IsNullLiteral(callback) || !IsParentDeclarationWithEndInvoke(callback, semanticModel));

        private bool ParentMethodContainsEndInvoke(SyntaxNode node, SemanticModel semantic)
        {
            var parentContext = node.AncestorsAndSelf().FirstOrDefault(IsParentDeclaration);
            return IsParentDeclarationWithEndInvoke(parentContext, semantic);
        }

        private bool IsParentDeclarationWithEndInvoke(SyntaxNode node, SemanticModel semanticModel)
        {
            if (IsParentDeclaration(node))
            {
                var context = new EndInvokeContext(Language.Syntax, semanticModel, node);
                VisitInvocation(context);
                return context.ContainsEndInvoke;
            }
            else
            {
                return false;
            }
        }

        private bool IsParentDeclaration(SyntaxNode node) =>
            ParentDeclarationKinds.Contains(Language.Syntax.Kind(node));

        protected class EndInvokeContext
        {
            private readonly SyntaxFacade<TSyntaxKind> syntax;
            private readonly SemanticModel semanticModel;

            public SyntaxNode Root { get; }
            public bool ContainsEndInvoke { get; private set; }

            public EndInvokeContext(SyntaxFacade<TSyntaxKind> syntax, SemanticModel semanticModel, SyntaxNode root)
            {
                this.syntax = syntax;
                this.semanticModel = semanticModel;
                Root = root;
            }

            public bool VisitInvocationExpression(SyntaxNode node)
            {
                if (syntax.NodeExpression(node).ToStringContains(EndInvoke)
                    && semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.Name == EndInvoke
                    && IsDelegate(methodSymbol))
                {
                    ContainsEndInvoke = true;
                    return false;   // Stop visiting
                }
                return true;
            }
        }
    }
}
