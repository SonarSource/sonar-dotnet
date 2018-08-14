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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotOverwriteCollectionElements : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4143";
        private const string MessageFormat = "Verify this is the {0} that was intended; a value has already been set for it.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var assignmentOrInvocation = (ExpressionSyntax)c.Node;

                var indexSetter = GetIndexSetter(assignmentOrInvocation, c.SemanticModel);

                if (!indexSetter.Found ||
                    IsItemRead(assignmentOrInvocation, c.SemanticModel, indexSetter.Collection, indexSetter.Key))
                {
                    return;
                }

                var previousIndexSetters = GetPreviousStatements(assignmentOrInvocation)
                    .OfType<ExpressionStatementSyntax>()
                    .Select(x => GetIndexSetter(x.Expression, c.SemanticModel));

                // When key is not a constant we break the lookup at the first line that
                // is not setting a value in the collection. This is to avoid FP when
                // the index variable is modified during the initialization sequence.
                previousIndexSetters = indexSetter.Key is ISymbol
                    ? previousIndexSetters.TakeWhile(x => x.Found)
                    : previousIndexSetters.Where(x => x.Found);

                var firstIndexSetter = previousIndexSetters
                    .LastOrDefault(x => indexSetter.Collection.Equals(x.Collection) && indexSetter.Key.Equals(x.Key));

                if (firstIndexSetter.Found)
                {
                    var keyOrIndexText = IsDictionary(indexSetter.Collection.GetSymbolType()) ? "key" : "index";
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, assignmentOrInvocation.GetLocation(), messageArgs: keyOrIndexText,
                        additionalLocations: new[] { firstIndexSetter.Expression.GetLocation() }));
                }
            },
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.InvocationExpression);
        }

        private static CollectionAndIndex GetIndexSetter(ExpressionSyntax assignmentOrInvocation, SemanticModel semanticModel)
        {
            ISymbol collection = null;
            object key = null;
            if (assignmentOrInvocation is AssignmentExpressionSyntax assignment)
            {
                if (assignment.Left is ElementAccessExpressionSyntax elementAccess &&
                    elementAccess.Expression != null &&
                    elementAccess.ArgumentList?.Arguments.Count == 1)
                {
                    collection = semanticModel.GetSymbolInfo(elementAccess.Expression).Symbol;
                    key = GetConstantOrSymbol(semanticModel, elementAccess.ArgumentList.Arguments[0].Expression);
                }
            }
            else if (assignmentOrInvocation is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                invocation.ArgumentList?.Arguments.Count > 0 &&
                IsDictionaryAdd(semanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol))
            {
                collection = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                key = GetConstantOrSymbol(semanticModel, invocation.ArgumentList.Arguments[0].Expression);
            }

            return new CollectionAndIndex(collection != null && key != null, assignmentOrInvocation, collection, key);
        }

        private static object GetConstantOrSymbol(SemanticModel semanticModel, SyntaxNode indexSyntax)
        {
            var constant = semanticModel.GetConstantValue(indexSyntax);
            if (constant.HasValue)
            {
                return constant;
            }

            var symbol = semanticModel.GetSymbolInfo(indexSyntax).Symbol;

            // Prevent 'dict[i++] = 5' from raising issues
            if (symbol != null &&
                symbol.Kind != SymbolKind.Method)
            {
                return symbol;
            }

            return null;
        }

        private static bool IsItemRead(ExpressionSyntax assignmentOrInvocation, SemanticModel semanticModel, ISymbol collection, object key)
        {
            if (assignmentOrInvocation is AssignmentExpressionSyntax assignment &&
                assignment.Right != null)
            {
                return assignment.Right.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>().Any(IsSameElement);
            }
            else if (assignmentOrInvocation is InvocationExpressionSyntax invocation &&
                invocation.ArgumentList != null)
            {
                return invocation.ArgumentList.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>().Any(IsSameElement);
            }
            return false;

            bool IsSameElement(ElementAccessExpressionSyntax elementAccess) =>
                elementAccess.Expression != null &&
                elementAccess.ArgumentList?.Arguments.Count == 1 &&
                collection.Equals(semanticModel.GetSymbolInfo(elementAccess.Expression).Symbol) &&
                key.Equals(GetConstantOrSymbol(semanticModel, elementAccess.ArgumentList.Arguments[0].Expression));
        }

        private static IEnumerable<StatementSyntax> GetPreviousStatements(ExpressionSyntax expression)
        {
            var statement = expression.FirstAncestorOrSelf<StatementSyntax>();
            return statement == null
                ? Enumerable.Empty<StatementSyntax>()
                : statement.Parent.ChildNodes().OfType<StatementSyntax>().TakeWhile(x => x != statement).Reverse();
        }

        private static bool IsDictionaryAdd(IMethodSymbol methodSymbol) =>
            methodSymbol != null &&
            methodSymbol.Name == nameof(IDictionary<object, object>.Add) &&
            methodSymbol.MethodKind == MethodKind.Ordinary &&
            methodSymbol.Parameters.Length == 2 &&
            IsDictionary(methodSymbol.ContainingType);

        private static bool IsDictionary(ISymbol symbol)
        {
            var symbolType = symbol.GetSymbolType();
            return symbolType != null
                && symbolType.OriginalDefinition.DerivesOrImplements(KnownType.System_Collections_Generic_IDictionary_TKey_TValue);
        }

        private struct CollectionAndIndex
        {
            public CollectionAndIndex(bool found, ExpressionSyntax expression, ISymbol collection, object key)
            {
                Found = found;
                Expression = expression;
                Collection = collection;
                Key = key;
            }

            public bool Found { get; }
            public ExpressionSyntax Expression { get; }
            public ISymbol Collection { get; }
            public object Key { get; }
        }
    }
}
