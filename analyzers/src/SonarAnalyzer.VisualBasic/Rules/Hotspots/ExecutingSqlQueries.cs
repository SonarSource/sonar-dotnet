/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class ExecutingSqlQueries : ExecutingSqlQueriesBase<SyntaxKind, ExpressionSyntax, IdentifierNameSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language { get; } = VisualBasicFacade.Instance;

    public ExecutingSqlQueries() : this(AnalyzerConfiguration.Hotspot) { }

    internal /*for testing*/ ExecutingSqlQueries(IAnalyzerConfiguration configuration) : base(configuration) { }

    protected override ExpressionSyntax GetArgumentAtIndex(InvocationContext context, int index) =>
        context.Node is InvocationExpressionSyntax invocation
            ? invocation.ArgumentList.Get(index)
            : null;

    protected override ExpressionSyntax GetArgumentAtIndex(ObjectCreationContext context, int index) =>
        context.Node is ObjectCreationExpressionSyntax objectCreation
            ? objectCreation.ArgumentList.Get(index)
            : null;

    protected override ExpressionSyntax GetSetValue(PropertyAccessContext context) =>
        context.Node is MemberAccessExpressionSyntax setter && setter.IsLeftSideOfAssignment()
            ? ((AssignmentStatementSyntax)setter.GetSelfOrTopParenthesizedExpression().Parent).Right.RemoveParentheses()
            : null;

    protected override bool IsTracked(ExpressionSyntax expression, SyntaxBaseContext context) =>
        expression is not null && (IsSensitiveExpression(expression, context.Model) || IsTrackedVariableDeclaration(expression, context));

    protected override bool IsSensitiveExpression(ExpressionSyntax expression, SemanticModel semanticModel) =>
        IsConcatenation(expression, semanticModel)
        || expression.IsKind(SyntaxKind.InterpolatedStringExpression)
        || (expression is InvocationExpressionSyntax invocation && IsInvocationOfInterest(invocation, semanticModel));

    protected override Location SecondaryLocationForExpression(ExpressionSyntax node, string identifierNameToFind, out string identifierNameFound)
    {
        identifierNameFound = string.Empty;
        if (node is null)
        {
            return Location.None;
        }

        if (node.Parent is EqualsValueSyntax equalsValue
            && equalsValue.Parent is VariableDeclaratorSyntax declarationSyntax)
        {
            var identifier = declarationSyntax.Names.FirstOrDefault(x => x.Identifier.ValueText.Equals(identifierNameToFind, StringComparison.OrdinalIgnoreCase));

            if (identifier is null)
            {
                return Location.None;
            }
            else
            {
                identifierNameFound = identifier.Identifier.ValueText;
                return identifier.GetLocation();
            }
        }

        if (node.Parent is AssignmentStatementSyntax assignment)
        {
            identifierNameFound = assignment.Left.GetName();
            return assignment.Left.GetLocation();
        }

        return Location.None;
    }

    private static bool IsInvocationOfInterest(InvocationExpressionSyntax invocation, SemanticModel model) =>
        (invocation.IsMethodInvocation(KnownType.System_String, "Format", model) || invocation.IsMethodInvocation(KnownType.System_String, "Concat", model))
        && !AllConstants(invocation.ArgumentList.Arguments.ToList(), model);

    private static bool IsConcatenation(ExpressionSyntax expression, SemanticModel model) =>
        IsConcatenationOperator(expression)
        && expression is BinaryExpressionSyntax concatenation
        && !IsConcatenationOfConstants(concatenation, model);

    private static bool AllConstants(List<ArgumentSyntax> arguments, SemanticModel model) =>
        arguments.TrueForAll(x => x.GetExpression().HasConstantValue(model));

    private static bool IsConcatenationOperator(SyntaxNode node) =>
        node.IsKind(SyntaxKind.ConcatenateExpression)
        || node.IsKind(SyntaxKind.AddExpression);

    private static bool IsConcatenationOfConstants(BinaryExpressionSyntax binaryExpression, SemanticModel model)
    {
        if ((model.GetTypeInfo(binaryExpression).Type is ITypeSymbol) && binaryExpression.Right.HasConstantValue(model))
        {
            var nestedLeft = binaryExpression.Left;
            var nestedBinary = nestedLeft as BinaryExpressionSyntax;
            while (nestedBinary is not null)
            {
                if (nestedBinary.Right.HasConstantValue(model)
                    && (IsConcatenationOperator(nestedBinary) || nestedBinary.HasConstantValue(model)))
                {
                    nestedLeft = nestedBinary.Left;
                    nestedBinary = nestedLeft as BinaryExpressionSyntax;
                }
                else
                {
                    return false;
                }
            }
            return nestedLeft.HasConstantValue(model);
        }
        return false;
    }
}
