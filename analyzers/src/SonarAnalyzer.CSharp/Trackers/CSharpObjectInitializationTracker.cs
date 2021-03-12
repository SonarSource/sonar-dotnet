/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.Helpers.Trackers
{
    /// <summary>
    /// Verifies the initialization of an object, whether one or more properties have been correctly set when the object was initialized.
    /// </summary>
    /// A correct initialization could consist of:
    /// - EITHER invoking the constructor with specific parameters
    /// - OR invoking the constructor and then setting some specific properties on the created object
    public class CSharpObjectInitializationTracker
    {
        /// <summary>
        /// By default, we consider constructors unsafe.
        /// </summary>
        private const bool DefaultIsConstructorSafe = false;

        /// <summary>
        /// By default, the constructor arguments are ignored.
        /// </summary>
        private const int DefaultTrackedConstructorArgumentIndex = -1;

        private static readonly Func<ISymbol, ExpressionSyntax, SemanticModel, bool> DefaultIsAllowedObject = (s, node, model) => false;

        /// <summary>
        /// Given the value of a literal (e.g. enum or boolean), returns true if it is an allowed value.
        /// </summary>
        private readonly Predicate<object> isAllowedConstantValue;

        /// <summary>
        /// Given the symbol of an object, the expression used to populate the value and the semantic model, returns true if it is allowed.
        /// </summary>
        private readonly Func<ISymbol, ExpressionSyntax, SemanticModel, bool> isAllowedObject;

        /// <summary>
        /// Given the name of a property, returns true if it is of interest for the rule verdict.
        /// </summary>
        private readonly Predicate<string> isTrackedPropertyName;

        /// <summary>
        /// An array of types for which this class tracks initializations.
        /// </summary>
        private readonly ImmutableArray<KnownType> trackedTypes;

        /// <summary>
        /// True if the constructor is safe by design (when we don't have more information about relevant properties to be set).
        /// </summary>
        private readonly bool constructorIsSafe;

        /// <summary>
        /// The index of a constructor argument that corresponds to a tracked property. It should be -1 if it should be ignored.
        /// </summary>
        private readonly int trackedConstructorArgumentIndex;

        internal CSharpObjectInitializationTracker(Predicate<object> isAllowedConstantValue,
            ImmutableArray<KnownType> trackedTypes,
            Predicate<string> isTrackedPropertyName,
            Func<ISymbol, ExpressionSyntax, SemanticModel, bool> isAllowedObject = null,
            bool constructorIsSafe = DefaultIsConstructorSafe,
            int trackedConstructorArgumentIndex = DefaultTrackedConstructorArgumentIndex)
        {
            this.isAllowedConstantValue = isAllowedConstantValue;
            this.trackedTypes = trackedTypes;
            this.isTrackedPropertyName = isTrackedPropertyName;
            this.isAllowedObject = isAllowedObject ?? DefaultIsAllowedObject;
            this.constructorIsSafe = constructorIsSafe;
            this.trackedConstructorArgumentIndex = trackedConstructorArgumentIndex;
        }

        internal bool ShouldBeReported(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, Func<bool> isDefaultConstructorSafe = null) =>
            IsTrackedType(objectCreation, semanticModel)
            && !ObjectCreatedWithAllowedValue(objectCreation, semanticModel, isDefaultConstructorSafe)
            && !IsLaterAssignedWithAllowedValue(objectCreation, semanticModel);

        internal bool ShouldBeReported(AssignmentExpressionSyntax assignment, SemanticModel semanticModel, Func<bool> isDefaultConstructorSafe = null) =>
            // Ignore assignments within object initializers, they are reported in the ObjectCreationExpression handler
            assignment.FirstAncestorOrSelf<InitializerExpressionSyntax>() == null
            && IsTrackedPropertyName(assignment.Left)
            && IsPropertyOnTrackedType(assignment.Left, semanticModel)
            && !IsAllowedValue(assignment.Right, semanticModel);

        /// <summary>
        /// Tests if the provided <paramref name="constantValue"/> is equal to an allowed constant (literal) value.
        /// </summary>
        private bool IsAllowedConstantValue(object constantValue) =>
            isAllowedConstantValue(constantValue);

        private bool IsTrackedType(ExpressionSyntax expression, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expression).Type.IsAny(trackedTypes);

        /// <summary>
        /// Tests if the expression is an allowed value. The implementation of checks is provided by the derived class.
        /// </summary>
        /// <returns>True if the expression is an allowed value, otherwise false.</returns>
        private bool IsAllowedValue(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            if (expression == null)
            {
                return false;
            }
            else if (expression.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return true;
            }
            else if (expression.FindConstantValue(semanticModel) is { } constantValue)
            {
                return IsAllowedConstantValue(constantValue);
            }
            else if (semanticModel.GetSymbolInfo(expression).Symbol is { } symbol)
            {
                return isAllowedObject(symbol, expression, semanticModel);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies that the properties are assigned with allowed values. Otherwise, verifies the constructor invocation.
        /// </summary>
        /// There are multiple ways of verifying:
        /// - implicit constructor with property setting
        /// var x = new X { Prop = new Y() };
        /// - constructor with passing a list of arguments, where we care about a specific argument
        /// <remarks>
        /// Currently we do not handle the situation with default and named arguments.
        /// </remarks>
        private bool ObjectCreatedWithAllowedValue(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, Func<bool> isDefaultConstructorSafe = null)
        {
            var trackedPropertyAssignments = GetInitializerExpressions(objectCreation.Initializer)
                .OfType<AssignmentExpressionSyntax>()
                .Where(assignment => IsTrackedPropertyName(assignment.Left))
                .ToList();
            if (trackedPropertyAssignments.Any())
            {
                return trackedPropertyAssignments.All(assignment => IsAllowedValue(assignment.Right, semanticModel));
            }
            else if (trackedConstructorArgumentIndex != -1)
            {
                var argumentList = objectCreation.ArgumentList;
                return argumentList == null
                    || argumentList.Arguments.Count != trackedConstructorArgumentIndex + 1
                    || IsAllowedValue(argumentList.Arguments[trackedConstructorArgumentIndex].Expression, semanticModel);
            }
            else
            {
                // if no tracked properties are being explicitly set or passed as arguments, check if the default constructor is safe
                return isDefaultConstructorSafe != null ? isDefaultConstructorSafe() : constructorIsSafe;
            }
        }

        /// <summary>
        /// Returns true if <paramref name="propertyName"/> is the name of a tracked property.
        /// </summary>
        private bool IsTrackedPropertyName(string propertyName) =>
            isTrackedPropertyName(propertyName);

        /// <summary>
        /// Returns true if the <paramref name="expression"/> has the name of a tracked property.
        /// </summary>
        private bool IsTrackedPropertyName(ExpressionSyntax expression)
        {
            var identifier = (expression as MemberAccessExpressionSyntax)?.Name?.Identifier ?? (expression as IdentifierNameSyntax)?.Identifier;
            return identifier.HasValue && IsTrackedPropertyName(identifier.Value.ValueText);
        }

        /// <summary>
        /// Returns true if the provided expression is a member of a tracked type.
        /// </summary>
        private bool IsPropertyOnTrackedType(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Expression != null
            && IsTrackedType(memberAccess.Expression, semanticModel);

        private bool IsLaterAssignedWithAllowedValue(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            var statement = objectCreation.FirstAncestorOrSelf<StatementSyntax>();
            if (statement == null)
            {
                return false;
            }

            var variableSymbol = GetAssignedVariableSymbol(objectCreation, semanticModel);
            var nextStatements = GetNextStatements(statement);
            var innerStatements = GetInnerStatements(statement);

            return variableSymbol != null
                && nextStatements.Union(innerStatements)
                    .OfType<ExpressionStatementSyntax>()
                    .Select(x => x.Expression)
                    .OfType<AssignmentExpressionSyntax>()
                    .Any(TrackedPropertySetWithAllowedValue);

            bool TrackedPropertySetWithAllowedValue(AssignmentExpressionSyntax assignment) =>
                variableSymbol.Equals(GetAssignedVariableSymbol(assignment, semanticModel))
                && IsTrackedPropertyName(assignment.Left)
                && IsAllowedValue(assignment.Right, semanticModel);
        }

        private static IEnumerable<ExpressionSyntax> GetInitializerExpressions(InitializerExpressionSyntax initializer) =>
            initializer?.Expressions ?? Enumerable.Empty<ExpressionSyntax>();

        private static ISymbol GetAssignedVariableSymbol(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            if (objectCreation.FirstAncestorOrSelf<AssignmentExpressionSyntax>()?.Left is { } variable)
            {
                return semanticModel.GetSymbolInfo(variable).Symbol;
            }

            return objectCreation.FirstAncestorOrSelf<VariableDeclaratorSyntax>() is { }  identifier
                ? semanticModel.GetDeclaredSymbol(identifier)
                : null;
        }

        private static ISymbol GetAssignedVariableSymbol(AssignmentExpressionSyntax assignment, SemanticModel semanticModel)
        {
            var identifier = (assignment.Left as MemberAccessExpressionSyntax)?.Expression ?? assignment.Left as IdentifierNameSyntax;
            return identifier != null
                ? semanticModel.GetSymbolInfo(identifier).Symbol
                : null;
        }

        private static IEnumerable<StatementSyntax> GetNextStatements(StatementSyntax statement) =>
            statement.Parent.ChildNodes().OfType<StatementSyntax>().SkipWhile(x => x != statement).Skip(1);

        private static IEnumerable<StatementSyntax> GetInnerStatements(StatementSyntax statement) =>
            statement.DescendantNodes().OfType<StatementSyntax>();
    }
}
