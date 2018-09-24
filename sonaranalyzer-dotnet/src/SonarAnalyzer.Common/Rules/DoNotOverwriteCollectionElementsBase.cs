/*
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotOverwriteCollectionElementsBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S4143";
        protected const string MessageFormat = "Verify this is the index/key that was intended; a value has already been set for it.";
    }

    public abstract class DoNotOverwriteCollectionElementsBase
        <TInvocationSyntax, TIdentifierSyntax, TStatementSyntax, TMemberAccessSyntax, TThisSyntax, TBaseSyntax, TExpressionSyntax>
        : DoNotOverwriteCollectionElementsBase
        where TStatementSyntax : SyntaxNode
        where TInvocationSyntax : SyntaxNode
        where TIdentifierSyntax : SyntaxNode
        where TMemberAccessSyntax : SyntaxNode
        where TThisSyntax : SyntaxNode
        where TBaseSyntax : SyntaxNode
        where TExpressionSyntax : SyntaxNode
    {
        internal abstract KnownType DictionaryType { get; }
        internal abstract DiagnosticDescriptor Rule { get; }

        protected abstract SyntaxNode GetExpression(TExpressionSyntax expression);

        protected abstract SyntaxNode GetExpression(TInvocationSyntax invocation);

        protected abstract SyntaxNode GetExpression(TMemberAccessSyntax memberAccess);

        protected abstract SyntaxNode GetName(TMemberAccessSyntax memberAccess);

        protected abstract SyntaxToken GetIdentifierToken(TIdentifierSyntax invocation);

        protected abstract SyntaxToken GetIdentifierToken(TMemberAccessSyntax memberAccess);

        protected abstract SyntaxToken GetIdentifierToken(TThisSyntax thisAccess);

        protected abstract SyntaxToken GetIdentifierToken(TBaseSyntax baseAccess);

        // this will return null if there's no argument in the invocation
        // the check must be done beforehand
        protected abstract SyntaxNode GetFirstArgument(TInvocationSyntax invocation);

        protected abstract bool HasNumberOfArguments(TInvocationSyntax invocation, int number);

        protected void VerifyInvocationExpression(SyntaxNodeAnalysisContext c)
        {
            var invocation = (TInvocationSyntax)c.Node;

            if (!HasNumberOfArguments(invocation, 2))
            {
                return;
            }

            var methodName = GetMethodName(invocation);
            var invokedOn = GetInvokedOnName(invocation);

            if (methodName == null ||
                methodName != "Add" ||
                invokedOn == null ||
                !IsDictionaryAdd(c.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol))
            {
                return;
            }

            var keyValue = GetFirstArgument(invocation).ToString();

            var previousAddInvocationOnVariable = GetPreviousStatements(invocation)
                 .OfType<TExpressionSyntax>()
                 .Select(ess => GetExpression(ess))
                 .TakeWhile(e => IsInvocationOnSameItem(e, invokedOn))
                 .Cast<TInvocationSyntax>()
                 .FirstOrDefault(ies => GetFirstArgument(ies).ToString() == keyValue &&
                     IsDictionaryAdd(c.SemanticModel.GetSymbolInfo(ies).Symbol as IMethodSymbol));

            if (previousAddInvocationOnVariable != null)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, invocation.GetLocation(),
                     additionalLocations: new[] { previousAddInvocationOnVariable.GetLocation() }));
            }
        }

        private bool IsInvocationOnSameItem(SyntaxNode expression, string invokedOn) =>
            expression is TInvocationSyntax ies &&
            HasNumberOfArguments(ies, 2) &&
            GetMethodName(ies) == "Add" &&
            GetInvokedOnName(ies) == invokedOn;

        protected string GetMethodName(TInvocationSyntax invocation) =>
            GetExpression(invocation) is TMemberAccessSyntax memberAccess
            ? GetIdentifierToken(memberAccess).ValueText
            : null;

        protected string GetInvokedOnName(TInvocationSyntax invocation)
        {
            if (GetExpression(invocation) is TMemberAccessSyntax memberAccessExpression)
            {
                var left = string.Empty;
                var right = GetName(memberAccessExpression);
                switch (GetExpression(memberAccessExpression))
                {
                    case TIdentifierSyntax identifier:
                        left = GetIdentifierToken(identifier).ValueText;
                        break;

                    case TMemberAccessSyntax memberAccess:
                        left = GetIdentifierToken(memberAccess).ValueText;
                        break;

                    case TThisSyntax thisAccess:
                        left = GetIdentifierToken(thisAccess).ValueText;
                        break;

                    case TBaseSyntax baseAccess:
                        left = GetIdentifierToken(baseAccess).ValueText;
                        break;

                    default:
                        left = string.Empty;
                        break;
                }
                return string.IsNullOrEmpty(left)
                    ? null
                    : $"{left}.{right}";
            }
            else if (GetExpression(invocation) is TIdentifierSyntax identifier)
            {
                return GetIdentifierToken(identifier).ValueText;
            }

            return null;
        }

        protected IEnumerable<TStatementSyntax> GetPreviousStatements(SyntaxNode expression) =>
            expression.FirstAncestorOrSelf<TStatementSyntax>() is TStatementSyntax statement
            ? statement.Parent.ChildNodes().OfType<TStatementSyntax>().TakeWhile(x => x != statement).Reverse()
            : Enumerable.Empty<TStatementSyntax>();

        protected bool IsDictionaryAdd(IMethodSymbol methodSymbol)
        {
            return methodSymbol != null &&
                methodSymbol.Name == nameof(IDictionary<object, object>.Add) &&
                methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.Parameters.Length == 2 &&
                IsDictionary(methodSymbol.ContainingType);

            bool IsDictionary(ISymbol symbol)
            {
                var symbolType = symbol.GetSymbolType();
                return symbolType != null
                    && symbolType.OriginalDefinition.DerivesOrImplements(DictionaryType);
            }
        }
    }
}
