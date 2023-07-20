/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class BeginInvokePairedWithEndInvokeBase<TSyntaxKind, TInvocationExpressionSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TInvocationExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S4583";
        protected const string EndInvoke = "EndInvoke";
        private const string BeginInvoke = "BeginInvoke";

        protected abstract TSyntaxKind InvocationExpressionKind { get; }
        protected abstract ISet<TSyntaxKind> ParentDeclarationKinds { get; }
        protected abstract string CallbackParameterName { get; }
        protected abstract void VisitInvocation(EndInvokeContext context);

        protected override string MessageFormat => "Pair this \"BeginInvoke\" with an \"EndInvoke\".";

        /// <returns>
        /// - true if callback code has been resolved and does not contain "EndInvoke".
        /// - false if callback code contains "EndInvoke" or callback code has not been resolved.
        /// </returns>
        protected abstract bool IsInvalidCallback(SyntaxNode callbackArg, SemanticModel semanticModel);

        protected BeginInvokePairedWithEndInvokeBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
            {
                var invocation = (TInvocationExpressionSyntax)c.Node;
                if (Language.Syntax.NodeExpression(invocation) is { } expression
                    && expression.ToStringContains(BeginInvoke)
                    && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.Name == BeginInvoke
                    && IsDelegate(methodSymbol)
                    && methodSymbol.Parameters.SingleOrDefault(x => x.Name == CallbackParameterName) is { } parameter
                    && Language.MethodParameterLookup(invocation, methodSymbol).TryGetNonParamsSyntax(parameter, out var callbackArg)
                    && IsInvalidCallback(callbackArg, c.SemanticModel)
                    && !ParentMethodContainsEndInvoke(invocation, c.SemanticModel))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, Language.Syntax.InvocationIdentifier(invocation).Value.GetLocation()));
                }
            }, InvocationExpressionKind);

        protected static bool IsDelegate(IMethodSymbol methodSymbol) =>
            methodSymbol.ReceiverType.Is(TypeKind.Delegate);

        protected bool IsParentDeclarationWithEndInvoke(SyntaxNode node, SemanticModel semanticModel)
        {
            if (IsParentDeclaration(node))
            {
                var context = new EndInvokeContext(this, semanticModel, node);
                VisitInvocation(context);
                return context.ContainsEndInvoke;
            }
            else
            {
                return false;
            }
        }

        private bool ParentMethodContainsEndInvoke(SyntaxNode node, SemanticModel model) =>
            node.AncestorsAndSelf().FirstOrDefault(IsParentDeclaration) is { } parentContext
               && IsParentDeclarationWithEndInvoke(parentContext, model);

        private bool IsParentDeclaration(SyntaxNode node)
            => node is not null && ParentDeclarationKinds.Contains(Language.Syntax.Kind(node));

        protected class EndInvokeContext
        {
            private readonly BeginInvokePairedWithEndInvokeBase<TSyntaxKind, TInvocationExpressionSyntax> rule;
            private readonly SemanticModel semanticModel;

            public SyntaxNode Root { get; }
            public bool ContainsEndInvoke { get; private set; }

            public EndInvokeContext(BeginInvokePairedWithEndInvokeBase<TSyntaxKind, TInvocationExpressionSyntax> rule, SemanticModel semanticModel, SyntaxNode root)
            {
                this.rule = rule;
                this.semanticModel = semanticModel;
                Root = root;
            }

            public bool Visit(SyntaxNode node) =>
                !ContainsEndInvoke;  // Stop visiting once we found it

            public bool VisitInvocationExpression(SyntaxNode node)
            {
                if (rule.Language.Syntax.NodeExpression(node).ToStringContains(EndInvoke)
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
