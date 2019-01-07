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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ObjectShouldBeInitializedCorrectlyBase : HotspotDiagnosticAnalyzer
    {
        protected ObjectShouldBeInitializedCorrectlyBase()
            : base(AnalyzerConfiguration.AlwaysEnabled)
        {
        }

        protected ObjectShouldBeInitializedCorrectlyBase(IAnalyzerConfiguration analyzerConfiguration)
            : base(analyzerConfiguration)
        {
        }

        /// <summary>
        /// Gets the KnownType representing the type on which instances the rule will raise issues on.
        /// </summary>
        internal abstract KnownType TrackedType { get; }

        /// <summary>
        /// Gets the name of the property that has to be set with allowed value in
        /// order for the object to be initialized correctly.
        /// </summary>
        protected abstract string TrackedPropertyName { get; }

        /// <summary>
        /// Tests if the provided <paramref name="constantValue"/> is equal to allowed value.
        /// </summary>
        /// <returns>True when <paramref name="constantValue"/> is an allowed value, otherwise false.</returns>
        protected abstract bool IsAllowedValue(object constantValue);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                ccc =>
                {
                    if (!IsEnabled(ccc.Options))
                    {
                        return;
                    }

                    ccc.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var objectCreation = (ObjectCreationExpressionSyntax)c.Node;

                            if (IsTrackedType(objectCreation, c.SemanticModel) &&
                                !ObjectCreatedWithAllowedValue(objectCreation, c.SemanticModel) &&
                                !IsLaterAssignedWithAllowedValue(objectCreation, c.SemanticModel))
                            {
                                c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], objectCreation.GetLocation()));
                            }
                        },
                        SyntaxKind.ObjectCreationExpression);

                    ccc.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var assignment = (AssignmentExpressionSyntax)c.Node;

                            // Ignore assignments within object initializers, they are
                            // reported in the ObjectCreationExpression handler
                            if (assignment.FirstAncestorOrSelf<InitializerExpressionSyntax>() == null &&
                                IsTrackedPropertyName(assignment.Left) &&
                                IsPropertyOnTrackedType(assignment.Left, c.SemanticModel) &&
                                !IsAllowedValue(assignment.Right, c.SemanticModel))
                            {
                                c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], assignment.GetLocation()));
                            }
                        },
                        SyntaxKind.SimpleAssignmentExpression);
                });
        }

        /// <summary>
        /// Tests if the provided expression is a property of the <see cref="TrackedType"/>. Override this method
        /// when the <see cref="TrackedPropertyName"/> is a indexer for example.
        /// </summary>
        /// <returns>True when the parameter is a property of the <see cref="TrackedType"/>, otherwise false.</returns>
        protected virtual bool IsPropertyOnTrackedType(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Expression != null &&
            IsTrackedType(memberAccess.Expression, semanticModel);

        /// <summary>
        /// Tests it the provided <paramref name="argumentList"/> contains initialization of the <see cref="TrackedPropertyName"/>
        /// with an allowed value. Override this method when a constructor can initialize the property value with a allowed value.
        /// </summary>
        /// <returns>True when the <paramref name="argumentList"/> contains initialization of <see cref="TrackedPropertyName"/>
        /// with allowed value or when this constructor sets <see cref="TrackedPropertyName"/> with the
        /// allowed value by default, otherwise false.</returns>
        protected virtual bool CtorInitializesTrackedPropertyWithAllowedValue(ArgumentListSyntax argumentList, SemanticModel semanticModel) =>
            false;

        /// <summary>
        /// Tests if the <paramref name="expression"/> is <see cref="TrackedPropertyName"/>.
        /// </summary>
        /// <returns>True if the <paramref name="expression"/> is <see cref="TrackedPropertyName"/>,
        /// otherwise false.</returns>
        protected virtual bool IsTrackedPropertyName(ExpressionSyntax expression)
        {
            var memberAccess = expression as MemberAccessExpressionSyntax;
            var identifier = memberAccess?.Name?.Identifier ?? (expression as IdentifierNameSyntax)?.Identifier;
            return identifier.HasValue && IsTrackedPropertyName(identifier.Value.ValueText);
        }

        /// <summary>
        /// Tests if the expression is a constant equal to allowed value.
        /// </summary>
        /// <returns>True if the expression is a constant equal to allowed value,
        /// otherwise false.</returns>
        protected bool IsAllowedValue(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression != null &&
            IsAllowedValue(semanticModel.GetConstantValue(expression).Value);

        /// <summary>
        /// Tests if the provided expression is the <see cref="TrackedType"/> by calling GetTypeInfo.
        /// </summary>
        /// <returns>True when the expression if the <see cref="TrackedType"/>, otherwise false.</returns>
        protected bool IsTrackedType(ExpressionSyntax expression, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expression).Type
                .Is(TrackedType);

        private bool ObjectCreatedWithAllowedValue(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            var trackedPropertyAssignments = GetInitializerExpressions(objectCreation.Initializer)
                .OfType<AssignmentExpressionSyntax>()
                .Where(assignment => IsTrackedPropertyName(assignment.Left))
                .ToList();

            return trackedPropertyAssignments.Count != 0
                ? trackedPropertyAssignments.All(assignment => IsAllowedValue(assignment.Right, semanticModel))
                : CtorInitializesTrackedPropertyWithAllowedValue(objectCreation.ArgumentList, semanticModel);
        }

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

        private bool IsLaterAssignedWithAllowedValue(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
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
                    .Any(TrackedPropertySetWithAllowedValue);

            bool TrackedPropertySetWithAllowedValue(AssignmentExpressionSyntax assignment) =>
                variableSymbol.Equals(GetAssignedVariableSymbol(assignment, semanticModel))
                && IsTrackedPropertyName(assignment.Left)
                && IsAllowedValue(assignment.Right, semanticModel);
        }

        private IEnumerable<ExpressionSyntax> GetInitializerExpressions(InitializerExpressionSyntax initializer) =>
            initializer != null
                ? initializer.Expressions
                : Enumerable.Empty<ExpressionSyntax>();

        private bool IsTrackedPropertyName(string propertyName) =>
             Equals(propertyName, TrackedPropertyName);

        private static IEnumerable<StatementSyntax> GetNextStatements(StatementSyntax statement) =>
            statement.Parent.ChildNodes().OfType<StatementSyntax>().SkipWhile(x => x != statement).Skip(1);
    }
}
