/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotOverwriteCollectionElements : DoNotOverwriteCollectionElementsBase<ExpressionStatementSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override DiagnosticDescriptor Rule { get; } = rule;

        private static readonly HashSet<SyntaxKind> identifierOrLiteral = new HashSet<SyntaxKind>
        {
            SyntaxKind.IdentifierName,
            SyntaxKind.StringLiteralExpression,
            SyntaxKind.NumericLiteralExpression,
            SyntaxKind.CharacterLiteralExpression,
            SyntaxKind.NullLiteralExpression,
            SyntaxKind.TrueLiteralExpression,
            SyntaxKind.FalseLiteralExpression,
        };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                AnalysisAction,
                SyntaxKind.ExpressionStatement);

        protected override SyntaxNode GetCollectionIdentifier(ExpressionStatementSyntax statement)
        {
            var assignmentOrInvocation = GetAssignmentOrInvocation(statement);

            switch (assignmentOrInvocation?.Kind())
            {
                case SyntaxKind.InvocationExpression:
                    return GetInvokedMethodContainer((InvocationExpressionSyntax)assignmentOrInvocation)
                        .RemoveParentheses();

                case SyntaxKind.SimpleAssignmentExpression:
                    var assignment = (AssignmentExpressionSyntax)assignmentOrInvocation;
                    var elementAccess = assignment.Left as ElementAccessExpressionSyntax;
                    return GetIdentifier(elementAccess?.Expression.RemoveParentheses())
                        .RemoveParentheses();

                default:
                    return null;
            }
        }

        protected override SyntaxNode GetIndexOrKey(ExpressionStatementSyntax statement) =>
            GetIndexOrKeyArgument(statement)?.Expression.RemoveParentheses();

        protected override bool IsIdentifierOrLiteral(SyntaxNode syntaxNode) =>
            syntaxNode.IsAnyKind(identifierOrLiteral);

        private static SyntaxNode GetAssignmentOrInvocation(StatementSyntax statement)
        {
            if (!(statement is ExpressionStatementSyntax expressionStatement))
            {
                return null;
            }

            var expression = expressionStatement.Expression;

            return expression.IsKind(SyntaxKind.ConditionalAccessExpression)
                ? ((ConditionalAccessExpressionSyntax)expression).WhenNotNull
                : expression;
        }

        private static ArgumentSyntax GetIndexOrKeyArgument(StatementSyntax statement)
        {
            var assignmentOrInvocation = GetAssignmentOrInvocation(statement);

            switch (assignmentOrInvocation?.Kind())
            {
                case SyntaxKind.InvocationExpression:
                    var invocation = (InvocationExpressionSyntax)assignmentOrInvocation;
                    return invocation.ArgumentList.Arguments.ElementAtOrDefault(0);

                case SyntaxKind.SimpleAssignmentExpression:
                    var assignment = (AssignmentExpressionSyntax)assignmentOrInvocation;
                    return assignment.Left is ElementAccessExpressionSyntax elementAccess
                        ? elementAccess.ArgumentList.Arguments.ElementAtOrDefault(0)
                        : null;

                default:
                    return null;
            }
        }

        private static SyntaxNode GetInvokedMethodContainer(InvocationExpressionSyntax invocation)
        {
            var expression = invocation.Expression.RemoveParentheses();
            switch (expression.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    // a.Add(x)
                    // InvocationExpression | a.Add(x)
                    //   Expression: SimpleMemberAccess
                    //                 Name: Add
                    //                 Expression: a  // we need this
                    var memberAccess = (MemberAccessExpressionSyntax)expression;
                    return memberAccess.Name.ToString() == "Add"
                        ? memberAccess.Expression
                        : null; // Ignore invocations that are on methods different than Add
                case SyntaxKind.MemberBindingExpression:
                    // a?.Add(x)
                    // ConditionalExpression | a?.Add(x)
                    //   Expression: a // <-- we need this
                    //   WhenTrue: InvocationExpression | ?.Add(x)
                    //               Expression: MemberBinding   // <-- we are here
                    //                              Identifier: Add
                    var conditional = expression.Parent.Parent as ConditionalAccessExpressionSyntax;
                    return conditional?.Expression;
                default:
                    return null;
            }
        }

        private static SyntaxNode GetIdentifier(ExpressionSyntax expression)
        {
            switch (expression?.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    // a.b[index]
                    // ElementAccess    // <-- we are here
                    //   Arguments: index
                    //   Expression: SimpleMemberAccess
                    //                 Expression: a
                    //                 Name: b  // <-- we need this
                    return ((MemberAccessExpressionSyntax)expression).Name;
                case SyntaxKind.IdentifierName:
                    // a[index]
                    // ElementAccess    // <-- we are here
                    //   Arguments: index
                    //   Expression: IdentifierName
                    //                 Name: a  // <-- we need this
                    return expression;
                default:
                    return null;
            }
        }
    }
}
