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

using System;
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
    public sealed class DoNotOverwriteCollectionElements
        : DoNotOverwriteCollectionElementsBase<InvocationExpressionSyntax, IdentifierNameSyntax,
            StatementSyntax, MemberAccessExpressionSyntax, MeExpressionSyntax, MyBaseExpressionSyntax, ExpressionStatementSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        internal override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => VerifyAssignment(c),
                SyntaxKind.SimpleAssignmentStatement);
            context.RegisterSyntaxNodeActionInNonGenerated(
               c => VerifyInvocationExpression(c),
               SyntaxKind.InvocationExpression);
        }

        internal override KnownType DictionaryType => KnownType.System_Collections_Generic_IDictionary_TKey_TValue_VB;

        protected override SyntaxNode GetExpression(ExpressionStatementSyntax expression) => expression.Expression;

        protected override SyntaxNode GetExpression(InvocationExpressionSyntax invocation) => invocation.Expression;

        protected override SyntaxNode GetExpression(MemberAccessExpressionSyntax memberAccess) => memberAccess.Expression;

        protected override SyntaxNode GetName(MemberAccessExpressionSyntax memberAccess) => memberAccess.Name;

        protected override SyntaxToken GetIdentifierToken(IdentifierNameSyntax invocation) => invocation.Identifier;

        protected override SyntaxToken GetIdentifierToken(MemberAccessExpressionSyntax memberAccess) => memberAccess.Name.Identifier;

        protected override SyntaxToken GetIdentifierToken(MeExpressionSyntax thisAccess) => thisAccess.Keyword;

        protected override SyntaxToken GetIdentifierToken(MyBaseExpressionSyntax baseAccess) => baseAccess.Keyword;

        protected override bool HasNumberOfArguments(InvocationExpressionSyntax invocation, int number)
            => invocation.ArgumentList != null && invocation.ArgumentList.Arguments.Count == number;

        protected override SyntaxNode GetFirstArgument(InvocationExpressionSyntax invocation)
            => invocation.ArgumentList != null && invocation.ArgumentList.Arguments.Count >= 1
                ? invocation.ArgumentList.Arguments[0]
                : null;

        private void VerifyAssignment(SyntaxNodeAnalysisContext c)
        {
            var assignment = (AssignmentStatementSyntax)c.Node;

            if (!(assignment.Left is InvocationExpressionSyntax elementAccess) ||
                elementAccess.ArgumentList == null ||
                elementAccess.ArgumentList.Arguments.Count == 0 ||
                !(GetInvokedOnName(elementAccess) is string accessedOn))
            {
                return;
            }

            var arguments = elementAccess.ArgumentList.Arguments.Select(a => a.ToString());

            var previousAssignmentOfVariable = GetPreviousStatements(assignment)
                .OfType<AssignmentStatementSyntax>()
                .TakeWhile(e => IsElementAccessAssignmentOnSameItem(e, accessedOn))
                .Select(aes => aes.Left)
                .Cast<InvocationExpressionSyntax>()
                .WhereNotNull()
                .FirstOrDefault(eaes => arguments.SequenceEqual(eaes.ArgumentList.Arguments.Select(a => a.ToString())));

            if (previousAssignmentOfVariable != null)
            {
                Report(c, elementAccess, previousAssignmentOfVariable);
            }
        }

        private void Report(SyntaxNodeAnalysisContext c, InvocationExpressionSyntax elementAccess,
            InvocationExpressionSyntax previousAssignmentOfVariable)
        {
            Func<InvocationExpressionSyntax, bool> IsCollectionOrProperty = invocation =>
                (c.SemanticModel.GetSymbolInfo(invocation).Symbol is ISymbol symbol) &&
                (IsCollection(symbol) || symbol.Kind == SymbolKind.Property);
            Func<ExpressionSyntax, bool> IsArray = expression =>
                (c.SemanticModel.GetSymbolInfo(expression).Symbol is ISymbol symbol) &&
                symbol.GetSymbolType().DerivesOrImplements(KnownType.System_Array);

            if (IsCollectionOrProperty(elementAccess) && IsCollectionOrProperty(previousAssignmentOfVariable))
            {
                c.ReportDiagnosticWhenActive(
                    Diagnostic.Create(rule, elementAccess.GetLocation(),
                        additionalLocations: new[] { previousAssignmentOfVariable.GetLocation() }));
                return;
            }
            if (IsArray(elementAccess.Expression) && IsArray(previousAssignmentOfVariable.Expression))
            {
                c.ReportDiagnosticWhenActive(
                    Diagnostic.Create(rule, elementAccess.GetLocation(),
                        additionalLocations: new[] { previousAssignmentOfVariable.GetLocation() }));
            }
        }

        private bool IsElementAccessAssignmentOnSameItem(SyntaxNode expression, string accessedOn) =>
            expression is AssignmentStatementSyntax aes &&
            aes.Left is InvocationExpressionSyntax currentElementAccess &&
            GetInvokedOnName(currentElementAccess) is string currentAccessedOn &&
            currentElementAccess.ArgumentList != null &&
            (currentElementAccess.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) ||
            currentElementAccess.Expression.IsKind(SyntaxKind.IdentifierName)) &&
            accessedOn != null &&
            currentAccessedOn == accessedOn;

        private bool IsCollection(ISymbol symbol) =>
            symbol.OriginalDefinition != null && symbol.OriginalDefinition.ContainingType != null &&
            symbol.OriginalDefinition.ContainingType.DerivesOrImplements(KnownType.System_Collections_Generic_ICollection_T_VB);
    }
}
