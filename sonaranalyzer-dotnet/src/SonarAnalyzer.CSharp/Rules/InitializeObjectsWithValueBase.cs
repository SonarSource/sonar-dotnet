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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class InitializeObjectsWithValueBase : SonarDiagnosticAnalyzer
    {
        protected abstract DiagnosticDescriptor Rule { get; }

        internal abstract KnownType TrackedType { get; }

        protected abstract string TrackedPropertyName { get; }

        protected abstract object ExpectedPropertyValue { get; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectCreation = (ObjectCreationExpressionSyntax)c.Node;

                    if (IsTrackedType(objectCreation.Type, c.SemanticModel) &&
                        !IsInitializedAsExpected(objectCreation, c.SemanticModel) &&
                        !IsLaterAssignedWithExpectedValue(objectCreation, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, objectCreation.GetLocation()));
                    }
                },
                SyntaxKind.ObjectCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;

                    // Ignore assignments within object initializers, they are
                    // reported in the ObjectCreationExpression handler
                    if (FirstAncestorOfType<InitializerExpressionSyntax>(assignment) == null &&
                        IsTrackedPropertyName(assignment.Left) &&
                        IsPropertyOnTrackedType(assignment.Left, c.SemanticModel) &&
                        !ValueIsExpected(assignment, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, assignment.GetLocation()));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression);
        }

        private static ISymbol GetAssignedVariableSymbol(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            var variable = FirstAncestorOfType<AssignmentExpressionSyntax>(objectCreation)?.Left;
            if (variable != null)
            {
                return semanticModel.GetSymbolInfo(variable).Symbol;
            }

            var identifier = FirstAncestorOfType<VariableDeclaratorSyntax>(objectCreation);
            return identifier != null
                ? semanticModel.GetDeclaredSymbol(identifier)
                : null;
        }

        private static ISymbol GetAssignedVariableSymbol(AssignmentExpressionSyntax assignment, SemanticModel semanticModel)
        {
            var identifier = (assignment.Left as MemberAccessExpressionSyntax)?.Expression
                ?? assignment.Left as IdentifierNameSyntax;
            return identifier != null
                ? semanticModel.GetSymbolInfo(identifier).Symbol
                : null;
        }

        private bool IsPropertyOnTrackedType(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Expression != null &&
            IsTrackedType(memberAccess.Expression, semanticModel);

        private bool IsLaterAssignedWithExpectedValue(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            var statement = FirstAncestorOfType<StatementSyntax>(objectCreation);
            if (statement == null)
            {
                return false;
            }

            var variableSymbol = GetAssignedVariableSymbol(objectCreation, semanticModel);

            return variableSymbol != null
                && GetNextStatements(statement)
                    .OfType<ExpressionStatementSyntax>()
                    .Select(x => x.Expression)
                    .OfType<AssignmentExpressionSyntax>()
                    .Any(TrackedPropertySetWithExpectedValue);

            bool TrackedPropertySetWithExpectedValue(AssignmentExpressionSyntax assignment) =>
                variableSymbol.Equals(GetAssignedVariableSymbol(assignment, semanticModel))
                && IsTrackedPropertyName(assignment.Left)
                && ValueIsExpected(assignment, semanticModel);
        }

        private bool IsTrackedType(ExpressionSyntax expression, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(expression).Symbol
                .GetSymbolType()
                .Is(TrackedType);

        private bool IsInitializedAsExpected(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel) =>
            objectCreation.Initializer != null &&
            objectCreation.Initializer.Expressions
                .OfType<AssignmentExpressionSyntax>()
                .Any(assignment => IsTrackedPropertyName(assignment?.Left) && ValueIsExpected(assignment, semanticModel));

        private bool IsTrackedPropertyName(ExpressionSyntax expression)
        {
            var memberAccess = expression as MemberAccessExpressionSyntax;
            var identifier = memberAccess?.Name?.Identifier ?? (expression as IdentifierNameSyntax)?.Identifier;
            return identifier.HasValue && identifier.Value.ValueText == TrackedPropertyName;
        }

        private bool ValueIsExpected(AssignmentExpressionSyntax assignment, SemanticModel semanticModel) =>
            assignment?.Right != null &&
            semanticModel.GetConstantValue(assignment.Right).Value is object constantValue &&
            ExpectedPropertyValue.Equals(constantValue);

        private static T FirstAncestorOfType<T>(SyntaxNode node) =>
            node.Ancestors().OfType<T>().FirstOrDefault();

        private static IEnumerable<StatementSyntax> GetNextStatements(StatementSyntax statement) =>
            statement.Parent.ChildNodes().OfType<StatementSyntax>().SkipWhile(x => x != statement).Skip(1);
    }
}
