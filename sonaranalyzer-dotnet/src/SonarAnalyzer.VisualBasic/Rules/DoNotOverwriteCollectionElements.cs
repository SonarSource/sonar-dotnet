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
    public sealed class DoNotOverwriteCollectionElements
        : DoNotOverwriteCollectionElementsBase<InvocationExpressionSyntax, IdentifierNameSyntax, StatementSyntax,
            MemberAccessExpressionSyntax, MeExpressionSyntax, MyBaseExpressionSyntax, ExpressionStatementSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                new AssignmentVerifier(this).GetAnalysisAction(rule),
                SyntaxKind.SimpleAssignmentStatement);
            context.RegisterSyntaxNodeActionInNonGenerated(
               new InvocationVerifier(this).GetAnalysisAction(rule),
               SyntaxKind.InvocationExpression);
        }

        protected override SyntaxNode GetExpression(InvocationExpressionSyntax invocation) => invocation.Expression;

        protected override SyntaxNode GetExpression(MemberAccessExpressionSyntax memberAccess) => memberAccess.Expression;

        protected override SyntaxNode GetName(MemberAccessExpressionSyntax memberAccess) => memberAccess.Name;

        protected override SyntaxToken GetIdentifierToken(IdentifierNameSyntax invocation) => invocation.Identifier;

        protected override SyntaxToken GetIdentifierToken(MemberAccessExpressionSyntax memberAccess) => memberAccess.Name.Identifier;

        protected override SyntaxToken GetIdentifierToken(MeExpressionSyntax thisAccess) => thisAccess.Keyword;

        protected override SyntaxToken GetIdentifierToken(MyBaseExpressionSyntax baseAccess) => baseAccess.Keyword;

        private class InvocationVerifier : InvocationVerifierBase
        {
            private readonly DoNotOverwriteCollectionElements analyzer;

            public InvocationVerifier(DoNotOverwriteCollectionElements analyzer)
                : base(analyzer.GetInvokedOnName, analyzer.GetPreviousStatements)
            {
                this.analyzer = analyzer;
            }

            internal override KnownType DictionaryType => KnownType.System_Collections_Generic_IDictionary_TKey_TValue_VB;

            protected override bool HasNumberOfArguments(InvocationExpressionSyntax invocation, int number) =>
                invocation.ArgumentList != null && invocation.ArgumentList.Arguments.Count == number;

            protected override SyntaxNode GetFirstArgument(InvocationExpressionSyntax invocation) =>
                invocation.ArgumentList != null && invocation.ArgumentList.Arguments.Count >= 1
                    ? invocation.ArgumentList.Arguments[0]
                    : null;

            protected override SyntaxNode GetExpression(ExpressionStatementSyntax expression) =>
                expression.Expression;

            protected override InvocationExpressionSyntax GetInvocation(SyntaxNode node) =>
                (InvocationExpressionSyntax) node;

            protected override SyntaxNode GetExpression(MemberAccessExpressionSyntax memberAccess) =>
                analyzer.GetExpression(memberAccess);

            protected override SyntaxNode GetExpression(InvocationExpressionSyntax memberAccess) =>
                analyzer.GetExpression(memberAccess);

            protected override SyntaxToken GetIdentifierToken(MemberAccessExpressionSyntax memberAccess) =>
                analyzer.GetIdentifierToken(memberAccess);

            protected override SyntaxNode GetName(MemberAccessExpressionSyntax memberAccess) =>
                analyzer.GetName(memberAccess);
        }

        private class AssignmentVerifier : AssignmentVerifierBase<AssignmentStatementSyntax, InvocationExpressionSyntax>
        {
            private readonly DoNotOverwriteCollectionElements analyzer;
            public AssignmentVerifier(DoNotOverwriteCollectionElements analyzer)
            {
                this.analyzer = analyzer;
            }

            protected override void Report(SyntaxNodeAnalysisContext c, DiagnosticDescriptor rule,
                InvocationExpressionSyntax elementAccess, InvocationExpressionSyntax previousAssignmentOfVariable)
            {
                bool IsCollectionOrProperty(InvocationExpressionSyntax invocation) =>
                    (c.SemanticModel.GetSymbolInfo(invocation).Symbol is ISymbol symbol) &&
                    (IsCollection(symbol) || symbol.Kind == SymbolKind.Property);
                bool IsArray(ExpressionSyntax expression) =>
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

            protected override InvocationExpressionSyntax GetElementAccess(AssignmentStatementSyntax assignment)
            {
                if (assignment.Left is InvocationExpressionSyntax elementAccess &&
                    elementAccess.ArgumentList != null &&
                    elementAccess.ArgumentList.Arguments.Count != 0)
                {
                    return elementAccess;
                }
                return null;
            }

            protected override string GetCollectionIdentifier(InvocationExpressionSyntax elementAccess) =>
                analyzer.GetInvokedOnName(elementAccess);

            protected override InvocationExpressionSyntax GetPreviousAssignmentOfVariable(InvocationExpressionSyntax elementAccess,
                    string collectionIdentifier, IEnumerable<string> arguments) =>
                analyzer.GetPreviousStatements(elementAccess)
                .OfType<AssignmentStatementSyntax>()
                .TakeWhile(e => IsElementAccessAssignmentOnSameItem(e, collectionIdentifier))
                .Select(aes => aes.Left)
                .Cast<InvocationExpressionSyntax>()
                .WhereNotNull()
                .FirstOrDefault(eaes => arguments.SequenceEqual(
                    eaes.ArgumentList.Arguments.Select(a => a.ToString())));

            private bool IsElementAccessAssignmentOnSameItem(SyntaxNode expression, string accessedOn) =>
                expression is AssignmentStatementSyntax aes &&
                aes.Left is InvocationExpressionSyntax currentElementAccess &&
                analyzer.GetInvokedOnName(currentElementAccess) is string currentAccessedOn &&
                currentElementAccess.ArgumentList != null &&
                (currentElementAccess.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) ||
                currentElementAccess.Expression.IsKind(SyntaxKind.IdentifierName)) &&
                accessedOn != null &&
                currentAccessedOn == accessedOn;

            protected override AssignmentStatementSyntax GetAssignment(SyntaxNode node) =>
                (AssignmentStatementSyntax) node;

            protected override IEnumerable<string> GetArguments(InvocationExpressionSyntax elementAccess) =>
                elementAccess.ArgumentList.Arguments.Select(a => a.ToString());

            private bool IsCollection(ISymbol symbol) =>
                symbol.OriginalDefinition != null && symbol.OriginalDefinition.ContainingType != null &&
                symbol.OriginalDefinition.ContainingType
                    .DerivesOrImplements(KnownType.System_Collections_Generic_ICollection_T_VB);
        }
    }
}
