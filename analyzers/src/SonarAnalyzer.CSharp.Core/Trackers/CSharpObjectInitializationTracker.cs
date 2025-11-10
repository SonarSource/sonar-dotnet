/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Core.Trackers;

/// <summary>
/// Verifies the initialization of an object, whether one or more properties have been correctly set when the object was initialized.
/// </summary>
/// A correct initialization could consist of:
/// - EITHER invoking the constructor with specific parameters
/// - OR invoking the constructor and then setting some specific properties on the created object
public class CSharpObjectInitializationTracker
{
    /// <summary>
    /// By default, the constructor arguments are ignored.
    /// </summary>
    private const int DefaultTrackedConstructorArgumentIndex = -1;

    private static readonly Func<ISymbol, SyntaxNode, SemanticModel, bool> DefaultIsAllowedObject = (s, node, model) => false;

    /// <summary>
    /// Given the value of a literal (e.g. enum or boolean), returns true if it is an allowed value.
    /// </summary>
    private readonly Predicate<object> isAllowedConstantValue;

    /// <summary>
    /// Given the symbol of an object, the expression used to populate the value and the semantic model, returns true if it is allowed.
    /// </summary>
    private readonly Func<ISymbol, SyntaxNode, SemanticModel, bool> isAllowedObject;

    /// <summary>
    /// Given the name of a property, returns true if it is of interest for the rule verdict.
    /// </summary>
    private readonly Predicate<string> isTrackedPropertyName;

    /// <summary>
    /// An array of types for which this class tracks initializations.
    /// </summary>
    private readonly ImmutableArray<KnownType> trackedTypes;

    /// <summary>
    /// The index of a constructor argument that corresponds to a tracked property. It should be -1 if it should be ignored.
    /// </summary>
    private readonly int trackedConstructorArgumentIndex;

    public CSharpObjectInitializationTracker(Predicate<object> isAllowedConstantValue,
        ImmutableArray<KnownType> trackedTypes,
        Predicate<string> isTrackedPropertyName,
        Func<ISymbol, SyntaxNode, SemanticModel, bool> isAllowedObject = null,
        int trackedConstructorArgumentIndex = DefaultTrackedConstructorArgumentIndex)
    {
        this.isAllowedConstantValue = isAllowedConstantValue;
        this.trackedTypes = trackedTypes;
        this.isTrackedPropertyName = isTrackedPropertyName;
        this.isAllowedObject = isAllowedObject ?? DefaultIsAllowedObject;
        this.trackedConstructorArgumentIndex = trackedConstructorArgumentIndex;
    }

    public bool ShouldBeReported(IObjectCreation objectCreation, SemanticModel semanticModel, bool isDefaultConstructorSafe) =>
        IsTrackedType(objectCreation.Expression, semanticModel)
        && !ObjectCreatedWithAllowedValue(objectCreation, semanticModel, isDefaultConstructorSafe)
        && !IsLaterAssignedWithAllowedValue(objectCreation, semanticModel);

    public bool ShouldBeReported(AssignmentExpressionSyntax assignment, SemanticModel semanticModel)
    {
        var assignmentMap = assignment.MapAssignmentArguments();

        // Ignore assignments within object initializers, they are reported in the ObjectCreationExpression handler
        return assignment.FirstAncestorOrSelf<InitializerExpressionSyntax>() == null
               && assignmentMap.Any(x => IsTrackedPropertyName(x.Left)
                                         && IsPropertyOnTrackedType(x.Left, semanticModel)
                                         && !IsAllowedValue(x.Right, semanticModel));
    }

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
    private bool IsAllowedValue(SyntaxNode expression, SemanticModel semanticModel)
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
    private bool ObjectCreatedWithAllowedValue(IObjectCreation objectCreation, SemanticModel semanticModel, bool isDefaultConstructorSafe)
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
            return isDefaultConstructorSafe;
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
    private bool IsTrackedPropertyName(SyntaxNode expression)
    {
        var identifier = (expression as MemberAccessExpressionSyntax)?.Name?.Identifier ?? (expression as IdentifierNameSyntax)?.Identifier;
        return identifier.HasValue && IsTrackedPropertyName(identifier.Value.ValueText);
    }

    /// <summary>
    /// Returns true if the provided expression is a member of a tracked type.
    /// </summary>
    private bool IsPropertyOnTrackedType(SyntaxNode expression, SemanticModel semanticModel) =>
        expression is MemberAccessExpressionSyntax memberAccess
        && memberAccess.Expression != null
        && IsTrackedType(memberAccess.Expression, semanticModel);

    private bool IsLaterAssignedWithAllowedValue(IObjectCreation objectCreation, SemanticModel semanticModel)
    {
        var statement = objectCreation.Expression.FirstAncestorOrSelf<StatementSyntax>();
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

        bool TrackedPropertySetWithAllowedValue(AssignmentExpressionSyntax assignment)
        {
            var assignmentMap = assignment.MapAssignmentArguments();
            return assignmentMap.Any(x => IsTrackedPropertyName(x.Left)
                                          && variableSymbol.Equals(GetAssignedVariableSymbol(x.Left, semanticModel))
                                          && IsAllowedValue(x.Right, semanticModel));
        }
    }

    private static IEnumerable<ExpressionSyntax> GetInitializerExpressions(InitializerExpressionSyntax initializer) =>
        initializer?.Expressions ?? Enumerable.Empty<ExpressionSyntax>();

    private static ISymbol GetAssignedVariableSymbol(IObjectCreation objectCreation, SemanticModel semanticModel)
    {
        if (objectCreation.Expression.FirstAncestorOrSelf<AssignmentExpressionSyntax>()?.Left is { } variable)
        {
            return semanticModel.GetSymbolInfo(variable).Symbol;
        }

        return objectCreation.Expression.FirstAncestorOrSelf<VariableDeclaratorSyntax>() is { }  identifier
            ? semanticModel.GetDeclaredSymbol(identifier)
            : null;
    }

    private static ISymbol GetAssignedVariableSymbol(SyntaxNode node, SemanticModel semanticModel)
    {
        var identifier = node is MemberAccessExpressionSyntax memberAccess ? memberAccess.Expression : node as IdentifierNameSyntax;
        return semanticModel.GetSymbolInfo(identifier).Symbol;
    }

    private static IEnumerable<StatementSyntax> GetNextStatements(StatementSyntax statement) =>
        statement.Parent.ChildNodes().OfType<StatementSyntax>().SkipWhile(x => x != statement).Skip(1);

    private static IEnumerable<StatementSyntax> GetInnerStatements(StatementSyntax statement) =>
        statement.DescendantNodes().OfType<StatementSyntax>();
}
