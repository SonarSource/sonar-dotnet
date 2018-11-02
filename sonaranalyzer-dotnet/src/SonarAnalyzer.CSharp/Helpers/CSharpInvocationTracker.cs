﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public class CSharpInvocationTracker : InvocationTracker<SyntaxKind>
    {
        public CSharpInvocationTracker(IAnalyzerConfiguration analysisConfiguration, DiagnosticDescriptor rule)
            : base(analysisConfiguration, rule)
        {
        }

        protected override SyntaxKind[] TrackedSyntaxKinds { get; } =
            new[] { SyntaxKind.InvocationExpression };

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            CSharp.GeneratedCodeRecognizer.Instance;

        protected override SyntaxNode GetIdentifier(SyntaxNode invocationExpression) =>
            ((InvocationExpressionSyntax)invocationExpression).Expression.GetIdentifier();

        #region Syntax-level checking methods

        public override InvocationCondition MatchSimpleNames(params MethodSignature[] methods)
        {
            return (context) => MethodSignatureHelper.IsMatch(context.Identifier as SimpleNameSyntax,
                    context.Model, context.InvokedMethodSymbol, methods);
        }

        public override bool FirstParameterIsStringAndIsNotConstant(InvocationContext context)
        {
            if (!base.FirstParameterIsString(context))
            {
                return false;
            }

            var argumentsSyntax = (context.Invocation as InvocationExpressionSyntax)?
                .ArgumentList?.Arguments.FirstOrDefault();

            return !IsConstantExpression(argumentsSyntax?.Expression, context.Model);
        }

        private static bool IsConstantExpression(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            if (expression == null)
            {
                return false;
            }

            var strippedExpression = expression.RemoveParentheses();
            if (strippedExpression is CastExpressionSyntax castExpression)
            {
                strippedExpression = castExpression.Expression.RemoveParentheses();
            }

            if (strippedExpression is LiteralExpressionSyntax)
            {
                return true;
            }

            var argumentSymbol = semanticModel.GetSymbolInfo(strippedExpression).Symbol;
            if (argumentSymbol == null)
            {
                return false; // can't tell - assume not constant
            }

            if ((argumentSymbol is ILocalSymbol local && local.IsConst) ||
                (argumentSymbol is IFieldSymbol field && field.IsConst))
            {
                return true; // is constant
            }

            return false;
        }

        #endregion
    }
}
