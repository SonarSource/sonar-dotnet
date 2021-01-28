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
            //FIXME: Coverage
            //FIXME: Module
            //SyntaxKind.AnonymousMethodExpression,
            //SyntaxKind.ClassBlock,
            SyntaxKind.CompilationUnit,
            SyntaxKind.FunctionBlock,
            //SyntaxKind.ConstructorBlock,
            //SyntaxKind.ConversionOperatorDeclaration,
            //SyntaxKind.DestructorDeclaration,
            //SyntaxKind.InterfaceDeclaration,
            //SyntaxKind.MethodDeclaration,
            SyntaxKind.SubBlock,
            //SyntaxKind.OperatorDeclaration,
            //SyntaxKind.ParenthesizedLambdaExpression,
            //SyntaxKind.PropertyDeclaration,
            //SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.SingleLineSubLambdaExpression,
            SyntaxKind.MultiLineSubLambdaExpression,
            //FIXME: MultiLine+test line 45
            //SyntaxKind.StructDeclaration,
            //SyntaxKindEx.LocalFunctionStatement,
        }.ToImmutableHashSet();

        public BeginInvokePairedWithEndInvoke() : base(RspecStrings.ResourceManager) { }

        protected override void VisitInvocation(EndInvokeContext context) =>
            new InvocationWalker(context).SafeVisit(context.Root);

        /// <summary>
        /// This method is looking for the callback code which can be:
        /// - in a identifier initializer (like a lambda)
        /// - passed directly as a lambda argument
        /// - passed as a new delegate instantiation (and the code can be inside the method declaration)
        /// - a mix of the above.
        /// </summary>
        protected override SyntaxNode FindCallback(SyntaxNode callbackArg, SemanticModel semanticModel)
        {
            var callback = callbackArg.RemoveParentheses();
            if (callback.IsKind(SyntaxKind.IdentifierName))
            {
                callback = LookupIdentifierInitializer((IdentifierNameSyntax)callback, semanticModel);
            }

            if (callback is ObjectCreationExpressionSyntax objectCreation)
            {
                callback = objectCreation.ArgumentList.Arguments.Count == 1 ? objectCreation.ArgumentList.Arguments.Single().GetExpression() : null;
                if (callback != null && semanticModel.GetSymbolInfo(callback).Symbol is IMethodSymbol methodSymbol)
                {
                    callback = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                }
            }

            return callback;
        }

        private static SyntaxNode LookupIdentifierInitializer(IdentifierNameSyntax identifier, SemanticModel semantic) =>
            semantic.GetSymbolInfo(identifier).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is VariableDeclaratorSyntax variableDeclarator
                && variableDeclarator.Initializer is EqualsValueSyntax equalsValue
                ? equalsValue.Value.RemoveParentheses()
                : null;

        private class InvocationWalker : VisualBasicSyntaxWalker
        {
            private static readonly SyntaxKind[] VisitOnlyOnParent = new[]
            {
                // FIXME: Coverage
                //SyntaxKind.ConstructorBlock,
                SyntaxKind.FunctionBlock,
                //SyntaxKind.MultiLineFunctionLambdaExpression,
                //SyntaxKind.MultiLineSubLambdaExpression,
                //SyntaxKind.SingleLineFunctionLambdaExpression,
                //SyntaxKind.SingleLineSubLambdaExpression,
                //SyntaxKind.SubBlock,
            };

            private readonly EndInvokeContext context;

            public InvocationWalker(EndInvokeContext context)
            {
                this.context = context;
            }

            public override void Visit(SyntaxNode node)
            {
                if (!context.ContainsEndInvoke  // Stop visiting once we found it
                    && (node == context.Root || !node.IsAnyKind(VisitOnlyOnParent)))
                {
                    base.Visit(node);
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (!context.VisitInvocationExpression(node))
                {
                    base.VisitInvocationExpression(node);
                }
            }
        }
    }
}
