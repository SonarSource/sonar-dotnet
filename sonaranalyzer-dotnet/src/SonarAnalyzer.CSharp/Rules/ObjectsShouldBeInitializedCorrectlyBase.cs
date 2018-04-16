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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ObjectsShouldBeInitializedCorrectlyBase<TExpectedValueType> : SonarDiagnosticAnalyzer
    {
        protected abstract DiagnosticDescriptor Rule { get; }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        internal abstract KnownType TrackedType { get; }

        protected abstract string TrackedPropertyName { get; }

        protected abstract TExpectedValueType ExpectedPropertyValue { get; }

        protected abstract bool ExpectedValueIsDefault { get; }

        protected abstract int CtorArgumentsCount { get; }

        protected abstract int CtorArgumentIndex { get; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectCreation = (ObjectCreationExpressionSyntax)c.Node;

                    if (IsTrackedType(objectCreation, c.SemanticModel) &&
                        !ObjectCreatedWithExpectedValue(objectCreation, c.SemanticModel) &&
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
                    if (assignment.FirstAncestorOrSelf<InitializerExpressionSyntax>() == null &&
                        IsTrackedPropertyName(assignment.Left) &&
                        IsPropertyOnTrackedType(assignment.Left, c.SemanticModel) &&
                        !IsExpectedValue(assignment.Right, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, assignment.GetLocation()));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression);

        }

        private bool ObjectCreatedWithExpectedValue(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel) =>
            IsInitializedAsExpected(objectCreation.Initializer, semanticModel) ||
                objectCreation.Initializer == null &&
                CtorHasExpectedArguments(objectCreation.ArgumentList, semanticModel);

        protected virtual bool IsExpectedValue(object constantValue) =>
            Equals(ExpectedPropertyValue, constantValue);

        private static ISymbol GetAssignedVariableSymbol(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            var variable = objectCreation.FirstAncestorOrSelf<AssignmentExpressionSyntax>()?.Left;
            if (variable != null)
            {
                return semanticModel.GetSymbolInfo(variable).Symbol;
            }

            var identifier = objectCreation.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
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

        protected virtual bool IsPropertyOnTrackedType(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Expression != null &&
            IsTrackedType(memberAccess.Expression, semanticModel);

        private bool IsLaterAssignedWithExpectedValue(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            var statement = objectCreation.FirstAncestorOrSelf<StatementSyntax>();
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
                && IsExpectedValue(assignment.Right, semanticModel);
        }

        protected bool IsTrackedType(ExpressionSyntax expression, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expression).Type
                .Is(TrackedType);

        protected virtual bool CtorHasExpectedArguments(ArgumentListSyntax argumentList, SemanticModel semanticModel) =>
            ExpectedValueIsDefault &&
                (argumentList == null ||
                argumentList.Arguments.Count != CtorArgumentsCount ||
                IsExpectedValue(argumentList.Arguments[CtorArgumentIndex].Expression, semanticModel));

        protected virtual bool IsInitializedAsExpected(InitializerExpressionSyntax initializer, SemanticModel semanticModel) =>
            initializer != null &&
            initializer.Expressions
                .OfType<AssignmentExpressionSyntax>()
                .Any(assignment => IsTrackedPropertyName(assignment?.Left) && IsExpectedValue(assignment?.Right, semanticModel));

        protected virtual bool IsTrackedPropertyName(ExpressionSyntax expression)
        {
            var memberAccess = expression as MemberAccessExpressionSyntax;
            var identifier = memberAccess?.Name?.Identifier ?? (expression as IdentifierNameSyntax)?.Identifier;
            return identifier.HasValue && IsTrackedPropertyName(identifier.Value.ValueText);
        }

        protected virtual bool IsTrackedPropertyName(string propertyName) =>
             Equals(propertyName, TrackedPropertyName);

        protected bool IsExpectedValue(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression != null &&
            semanticModel.GetConstantValue(expression).Value is TExpectedValueType constantValue &&
            IsExpectedValue(constantValue);

        private static IEnumerable<StatementSyntax> GetNextStatements(StatementSyntax statement) =>
            statement.Parent.ChildNodes().OfType<StatementSyntax>().SkipWhile(x => x != statement).Skip(1);
    }
}
