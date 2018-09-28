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
    public sealed class DoNotOverwriteCollectionElements
        : DoNotOverwriteCollectionElementsBase<InvocationExpressionSyntax, IdentifierNameSyntax, StatementSyntax,
            MemberAccessExpressionSyntax, ThisExpressionSyntax, BaseExpressionSyntax, ExpressionStatementSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                new AssignmentVerifier(this).GetAnalysisAction(rule),
                SyntaxKind.SimpleAssignmentExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
               new InvocationVerifier(this).GetAnalysisAction(rule),
               SyntaxKind.InvocationExpression);
        }

        #region common methods

        protected override SyntaxNode GetName(MemberAccessExpressionSyntax memberAccess) => memberAccess.Name;

        protected override SyntaxNode GetExpression(MemberAccessExpressionSyntax memberAccess) => memberAccess.Expression;

        protected override SyntaxToken GetIdentifierToken(IdentifierNameSyntax invocation) => invocation.Identifier;

        protected override SyntaxNode GetExpression(InvocationExpressionSyntax invocation) => invocation.Expression;

        protected override SyntaxToken GetIdentifierToken(MemberAccessExpressionSyntax memberAccess) => memberAccess.Name.Identifier;

        protected override SyntaxToken GetIdentifierToken(ThisExpressionSyntax thisAccess) => thisAccess.Token;

        protected override SyntaxToken GetIdentifierToken(BaseExpressionSyntax baseAccess) => baseAccess.Token;

        #endregion common methods

        private class InvocationVerifier : InvocationVerifierBase
        {
            private readonly DoNotOverwriteCollectionElements analyzer;

            public InvocationVerifier(DoNotOverwriteCollectionElements analyzer)
                : base(analyzer.GetInvokedOnName, analyzer.GetPreviousStatements)
            {
                this.analyzer = analyzer;
            }

            internal override KnownType DictionaryType => KnownType.System_Collections_Generic_IDictionary_TKey_TValue;

            protected override bool HasNumberOfArguments(InvocationExpressionSyntax invocation, int number)
                => invocation.ArgumentList != null && invocation.ArgumentList.Arguments.Count == number;

            protected override SyntaxNode GetFirstArgument(InvocationExpressionSyntax invocation)
                => invocation.ArgumentList != null && invocation.ArgumentList.Arguments.Count >= 1
                    ? invocation.ArgumentList.Arguments[0]
                    : null;

            protected override SyntaxNode GetExpression(ExpressionStatementSyntax expression)
                => expression.Expression;

            protected override InvocationExpressionSyntax GetInvocation(SyntaxNode node)
                => (InvocationExpressionSyntax)node;

            protected override SyntaxNode GetExpression(MemberAccessExpressionSyntax memberAccess)
                => analyzer.GetExpression(memberAccess);

            protected override SyntaxNode GetExpression(InvocationExpressionSyntax memberAccess)
                => analyzer.GetExpression(memberAccess);

            protected override SyntaxToken GetIdentifierToken(MemberAccessExpressionSyntax memberAccess)
                => analyzer.GetIdentifierToken(memberAccess);

            protected override SyntaxNode GetName(MemberAccessExpressionSyntax memberAccess)
                => analyzer.GetName(memberAccess);
        }

        private class AssignmentVerifier : AssignmentVerifierBase<AssignmentExpressionSyntax, ElementAccessExpressionSyntax>
        {
            private readonly DoNotOverwriteCollectionElements analyzer;
            public AssignmentVerifier(DoNotOverwriteCollectionElements analyzer)
            {
                this.analyzer = analyzer;
            }

            protected override void Report(SyntaxNodeAnalysisContext c, DiagnosticDescriptor rule,
                ElementAccessExpressionSyntax elementAccess, ElementAccessExpressionSyntax previousAssignmentOfVariable)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, elementAccess.GetLocation(),
                    additionalLocations: new[] { previousAssignmentOfVariable.GetLocation() }));
            }

            protected override bool TryGetElementAccess(AssignmentExpressionSyntax assignment, out ElementAccessExpressionSyntax elementAccess)
            {
                elementAccess = null;
                if (assignment.Left is ElementAccessExpressionSyntax ea &&
                    ea.ArgumentList != null &&
                    ea.ArgumentList.Arguments.Count != 0)
                {
                    elementAccess = ea;
                    return true;
                }
                return false;
            }

            protected override bool TryGetCollectionIdentifier(ElementAccessExpressionSyntax elementAccess, out string collectionIdentifier)
            {
                collectionIdentifier = null;
                if ((elementAccess.Expression as IdentifierNameSyntax)?.Identifier.ValueText is string ci)
                {
                    collectionIdentifier = ci;
                    return true;
                }
                return false;
            }

            protected override bool TryGetPreviousAssignmentOfVariable(ElementAccessExpressionSyntax elementAccess, string collectionIdentifier,
                IEnumerable<string> arguments, out ElementAccessExpressionSyntax previousAssignmentOfVariable)
            {
                previousAssignmentOfVariable = null;
                ElementAccessExpressionSyntax result = analyzer.GetPreviousStatements(elementAccess)
                    .OfType<ExpressionStatementSyntax>()
                    .Select(ess => ess.Expression)
                    .TakeWhile(e => IsElementAccessAssignmentOnSameItem(e, collectionIdentifier))
                    .Cast<AssignmentExpressionSyntax>()
                    .Select(aes => aes.Left)
                    .Cast<ElementAccessExpressionSyntax>()
                    .FirstOrDefault(eaes => arguments.SequenceEqual(eaes.ArgumentList.Arguments.Select(a => a.ToString())));
                if (result != null)
                {
                    previousAssignmentOfVariable = result;
                    return true;
                }
                return false;
            }

            private static bool IsElementAccessAssignmentOnSameItem(ExpressionSyntax expression, string accessedOn) =>
                expression is AssignmentExpressionSyntax aes &&
                aes.Left is ElementAccessExpressionSyntax currentElementAccess &&
                currentElementAccess.ArgumentList != null &&
                currentElementAccess.Expression is IdentifierNameSyntax ins &&
                ins.Identifier.ValueText == accessedOn;

            protected override AssignmentExpressionSyntax GetAssignment(SyntaxNode node)
                => (AssignmentExpressionSyntax)node;

            protected override IEnumerable<string> GetArguments(ElementAccessExpressionSyntax elementAccess)
                => elementAccess.ArgumentList.Arguments.Select(a => a.ToString());
        }

    }
}
