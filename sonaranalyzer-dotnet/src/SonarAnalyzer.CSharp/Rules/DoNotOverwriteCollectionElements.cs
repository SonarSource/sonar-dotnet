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
        private const string MessageFormat = "Verify this is the index/key that was intended; a value has already been set for it.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;

                    if (!(assignment.Left is ElementAccessExpressionSyntax elementAccess) ||
                        elementAccess.ArgumentList == null ||
                        elementAccess.ArgumentList.Arguments.Count == 0)
                    {
                        return;
                    }

                    var accessedOn = (elementAccess.Expression as IdentifierNameSyntax)?.Identifier.ValueText;
                    if (accessedOn == null)
                    {
                        return;
                    }

                    var arguments = elementAccess.ArgumentList.Arguments.Select(a => a.ToString());

                    var previousAssignmentOfVariable = GetPreviousStatements(assignment)
                        .OfType<ExpressionStatementSyntax>()
                        .Select(ess => ess.Expression)
                        .TakeWhile(e => IsElementAccessAssignmentOnSameItem(e, accessedOn))
                        .Cast<AssignmentExpressionSyntax>()
                        .Select(aes => aes.Left)
                        .Cast<ElementAccessExpressionSyntax>()
                        .FirstOrDefault(eaes => arguments.SequenceEqual(eaes.ArgumentList.Arguments.Select(a => a.ToString())));

                    if (previousAssignmentOfVariable != null)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, elementAccess.GetLocation(),
                            additionalLocations: new[] { previousAssignmentOfVariable.GetLocation() }));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
               c =>
               {
                   var invocation = (InvocationExpressionSyntax)c.Node;

                   if (invocation.ArgumentList == null ||
                       invocation.ArgumentList.Arguments.Count != 2)
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

                   var keyValue = invocation.ArgumentList.Arguments[0].ToString();

                   var previousAddInvocationOnVariable = GetPreviousStatements(invocation)
                        .OfType<ExpressionStatementSyntax>()
                        .Select(ess => ess.Expression)
                        .TakeWhile(e => IsInvocationOnSameItem(e, invokedOn))
                        .Cast<InvocationExpressionSyntax>()
                        .FirstOrDefault(ies => ies.ArgumentList.Arguments[0].ToString() == keyValue &&
                            IsDictionaryAdd(c.SemanticModel.GetSymbolInfo(ies).Symbol as IMethodSymbol));

                   if (previousAddInvocationOnVariable != null)
                   {
                       c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, invocation.GetLocation(),
                            additionalLocations: new[] { previousAddInvocationOnVariable.GetLocation() }));
                   }
               },
               SyntaxKind.InvocationExpression);
        }

        private static bool IsElementAccessAssignmentOnSameItem(ExpressionSyntax expression, string accessedOn) =>
            expression is AssignmentExpressionSyntax aes &&
            aes.Left is ElementAccessExpressionSyntax currentElementAccess &&
            currentElementAccess.ArgumentList != null &&
            currentElementAccess.Expression is IdentifierNameSyntax ins &&
            ins.Identifier.ValueText == accessedOn;

        private static bool IsInvocationOnSameItem(ExpressionSyntax expression, string invokedOn) =>
            expression is InvocationExpressionSyntax ies &&
            ies.ArgumentList?.Arguments.Count == 2 &&
            GetMethodName(ies) == "Add" &&
            GetInvokedOnName(ies) == invokedOn;

        private static string GetMethodName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.ValueText;
            }

            return null;
        }

        private static string GetInvokedOnName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                switch (memberAccessExpression.Expression)
                {
                    case IdentifierNameSyntax identifierName:
                        return identifierName.Identifier.ValueText;

                    case MemberAccessExpressionSyntax memberAccess:
                        return memberAccess.Name.Identifier.ValueText;

                    default:
                        return null;
                }
            }

            return null;
        }

        private static IEnumerable<StatementSyntax> GetPreviousStatements(ExpressionSyntax expression)
        {
            var statement = expression.FirstAncestorOrSelf<StatementSyntax>();
            return statement == null
                ? Enumerable.Empty<StatementSyntax>()
                : statement.Parent.ChildNodes().OfType<StatementSyntax>().TakeWhile(x => x != statement).Reverse();
        }

        private static bool IsDictionaryAdd(IMethodSymbol methodSymbol)
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
                    && symbolType.OriginalDefinition.DerivesOrImplements(KnownType.System_Collections_Generic_IDictionary_TKey_TValue);
            }
        }
    }
}
