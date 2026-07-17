/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PropertiesAccessCorrectField : PropertiesAccessCorrectFieldBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override IEnumerable<FieldData> FindFieldAssignments(IPropertySymbol property, Compilation compilation)
    {
        if (property.SetMethod.FirstSyntaxRef is not AccessorDeclarationSyntax setter)
        {
            return [];
        }

        // we only keep information for the first location of the symbol
        var assignments = new Dictionary<IFieldSymbol, FieldData>();
        FillAssignments(assignments, compilation, setter, true);

        // If there're no candidate variables, we'll try to inspect one local method invocation with value as argument
        if (assignments.Count == 0
            && (setter.ExpressionBody?.Expression ?? SingleInvocation(setter.Body)) is { } expression
            && FindInvokedMethod(compilation, property.ContainingType, expression) is MethodDeclarationSyntax invokedMethod)
        {
            FillAssignments(assignments, compilation, invokedMethod, false);
        }

        return assignments.Values;
    }

    protected override IEnumerable<FieldData> FindFieldReads(IPropertySymbol property, Compilation compilation)
    {
        // A field is considered accessed if it appears anywhere in the getter body, including across multiple return paths.
        var getterSyntax = property.GetMethod.FirstSyntaxRef;
        if (getterSyntax is not (AccessorDeclarationSyntax or ArrowExpressionClauseSyntax))
        {
            return [];
        }

        var reads = new Dictionary<IFieldSymbol, FieldData>();
        FillReads(getterSyntax, true);

        // Indirect access through a helper method is recognized only when the getter has a single return.
        if (reads.Count == 0 && SingleReturnGetter(getterSyntax) is { } returnExpression
            && FindInvokedMethod(compilation, property.ContainingType, returnExpression) is MethodDeclarationSyntax invokedMethod)
        {
            FillReads(invokedMethod, false);
        }
        return reads.Values;

        void FillReads(SyntaxNode root, bool useFieldLocation)
        {
            var notAssigned = root.DescendantNodes().OfType<ExpressionSyntax>().Where(x => !IsLeftSideOfAssignment(x));
            foreach (var expression in notAssigned)
            {
                var readField = ExtractFieldFromExpression(AccessorKind.Getter, expression, compilation, useFieldLocation);
                // we only keep information for the first location of the symbol
                if (readField.HasValue && !reads.ContainsKey(readField.Value.Field))
                {
                    reads.Add(readField.Value.Field, readField.Value);
                }
            }
        }
    }

    protected override bool ShouldIgnoreAccessor(IMethodSymbol accessorMethod, Compilation compilation) =>
        accessorMethod.FirstSyntaxRef switch
        {
            ArrowExpressionClauseSyntax arrowClause => ThrowExpressionSyntaxWrapper.IsInstance(arrowClause.Expression),
            not AccessorDeclarationSyntax => true,
            AccessorDeclarationSyntax accessor when ((SyntaxNode)accessor.Body ?? accessor).ContainsGetOrSetOnDependencyProperty(compilation)
                || AccessesSelfBaseProperty(accessorMethod, accessor, compilation) => true,
            AccessorDeclarationSyntax { Body: null, ExpressionBody: { } expressionBody } => ThrowExpressionSyntaxWrapper.IsInstance(expressionBody.Expression),
            AccessorDeclarationSyntax { Body: { } body } => body.DescendantNodes().Count(x => x is StatementSyntax) == 1
                && body.DescendantNodes().Count(x => x is ThrowStatementSyntax) == 1,
            _ => false,
        };

    protected override bool ImplementsExplicitGetterOrSetter(IPropertySymbol property) =>
        HasExplicitAccessor(property.SetMethod)
        || HasExplicitAccessor(property.GetMethod)
        || property.GetMethod.FirstSyntaxRef is ArrowExpressionClauseSyntax; // Only getters can have property-level arrow expression bodies in C#

    private static void FillAssignments(IDictionary<IFieldSymbol, FieldData> assignments, Compilation compilation, SyntaxNode root, bool useFieldLocation)
    {
        foreach (var node in root.DescendantNodes())
        {
            FieldData? foundField = null;
            if (node is AssignmentExpressionSyntax assignment)
            {
                foundField = assignment.Left.DescendantNodesAndSelf().OfType<ExpressionSyntax>()
                    .Select(x => ExtractFieldFromExpression(AccessorKind.Setter, x, compilation, useFieldLocation))
                    .FirstOrDefault(x => x is not null);
            }
            else if (node is ArgumentSyntax argument && argument.RefOrOutKeyword.Kind() is SyntaxKind.RefKeyword or SyntaxKind.OutKeyword)
            {
                foundField = ExtractFieldFromExpression(AccessorKind.Setter, argument.Expression, compilation, useFieldLocation);
            }
            if (foundField.HasValue && !assignments.ContainsKey(foundField.Value.Field))
            {
                assignments.Add(foundField.Value.Field, foundField.Value);
            }
        }
    }

    private static InvocationExpressionSyntax SingleReturnGetter(SyntaxNode getterSyntax) =>
        getterSyntax switch
        {
            ArrowExpressionClauseSyntax arrow => arrow.Expression as InvocationExpressionSyntax,
            AccessorDeclarationSyntax accessor => (accessor.ExpressionBody?.Expression ?? SingleReturn(accessor.Body)) as InvocationExpressionSyntax,
            _ => null,
        };

    private static ExpressionSyntax SingleReturn(SyntaxNode body)
    {
        if (body is null)
        {
            return null;
        }

        var returns = body.DescendantNodes().OfType<ReturnStatementSyntax>().ToArray();
        return returns.Length == 1 ? returns.Single().Expression : null;
    }

    private static ExpressionSyntax SingleInvocation(SyntaxNode body)
    {
        if (body is null)
        {
            return null;
        }

        var expressions = body.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(x => x.Expression).ToArray();
        if (expressions.Length == 1)
        {
            var expr = expressions.Single();
            if (expr is IdentifierNameSyntax or MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax })
            {
                return expr;
            }
        }
        return null;
    }

    private static FieldData? ExtractFieldFromExpression(AccessorKind accessorKind, ExpressionSyntax expression, Compilation compilation, bool useFieldLocation)
    {
        var model = compilation.GetSemanticModel(expression.SyntaxTree);
        if (model is null)
        {
            return null;
        }

        var strippedExpression = expression.WithoutEnclosingParentheses;

        // Check for direct field access: "foo"
        if (strippedExpression is IdentifierNameSyntax
            && model.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol directAccessField)
        {
            return new FieldData(accessorKind, directAccessField, strippedExpression, useFieldLocation);
        }
        // Check for "this.foo"
        else if (strippedExpression is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } member && model.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol fieldAccessedWithThis)
        {
            return new FieldData(accessorKind, fieldAccessedWithThis, member.Name, useFieldLocation);
        }
        else if (strippedExpression is AssignmentExpressionSyntax assignmentExpression
            && assignmentExpression.Parent is ReturnStatementSyntax
            && model.GetSymbolInfo(assignmentExpression.Left).Symbol is IFieldSymbol fieldAssignedFromExpression)
        {
            return new FieldData(accessorKind, fieldAssignedFromExpression, assignmentExpression.Left, useFieldLocation);
        }

        return null;
    }

    private static bool IsLeftSideOfAssignment(ExpressionSyntax expression)
    {
        var strippedExpression = expression.WithoutEnclosingParentheses;
        return strippedExpression.IsLeftSideOfAssignment
            || (strippedExpression.Parent is ExpressionSyntax parent && parent.IsLeftSideOfAssignment); // for this.field
    }

    private static bool HasExplicitAccessor(ISymbol symbol) =>
        symbol.FirstSyntaxRef is AccessorDeclarationSyntax accessor
        && accessor.DescendantNodes().Any();
}
