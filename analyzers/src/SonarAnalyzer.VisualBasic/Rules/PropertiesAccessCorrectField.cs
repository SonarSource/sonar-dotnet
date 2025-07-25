﻿/*
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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class PropertiesAccessCorrectField : PropertiesAccessCorrectFieldBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override IEnumerable<FieldData> FindFieldAssignments(IPropertySymbol property, Compilation compilation)
    {
        if (property.SetMethod.GetFirstSyntaxRef() is not AccessorStatementSyntax setter)
        {
            return [];
        }

        // we only keep information for the first location of the symbol
        var assignments = new Dictionary<IFieldSymbol, FieldData>();
        FillAssignments(setter, true);
        // If there're no candidate variables, we'll try to inspect one local method invocation with value as argument
        if (assignments.Count == 0
            && SingleInvocation(setter) is { } expression
            && FindInvokedMethod(compilation, property.ContainingType, expression) is MethodBaseSyntax invokedMethod)
        {
            FillAssignments(invokedMethod, false);
        }

        return assignments.Values;

        void FillAssignments(SyntaxNode root, bool useFieldLocation)
        {
            // The ".Parent" is to go from the accessor statement to the accessor block
            foreach (var node in root.Parent.DescendantNodes())
            {
                FieldData? foundField = null;
                if (node is AssignmentStatementSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentStatement or (int)SyntaxKind.ConcatenateAssignmentStatement } assignment)
                {
                    foundField = assignment.Left.DescendantNodesAndSelf().OfType<ExpressionSyntax>()
                        .Select(x => ExtractFieldFromExpression(AccessorKind.Setter, x, compilation, useFieldLocation))
                        .FirstOrDefault(x => x is not null);
                }
                else if (node is ArgumentSyntax argument)
                {
                    foundField = ExtractFieldFromRefArgument(argument, compilation, useFieldLocation);
                }
                if (foundField.HasValue && !assignments.ContainsKey(foundField.Value.Field))
                {
                    assignments.Add(foundField.Value.Field, foundField.Value);
                }
            }
        }
    }

    protected override IEnumerable<FieldData> FindFieldReads(IPropertySymbol property, Compilation compilation)
    {
        // We don't handle properties with multiple returns that return different fields
        if (property.GetMethod.GetFirstSyntaxRef() is not AccessorStatementSyntax getter)
        {
            return [];
        }

        var reads = new Dictionary<IFieldSymbol, FieldData>();
        FillReads(getter, true);
        // If there're no candidate variables, we'll try inspect one return of local method invocation
        if (reads.Count == 0
            && SingleReturn(getter) is InvocationExpressionSyntax returnExpression
            && FindInvokedMethod(compilation, property.ContainingType, returnExpression) is MethodBaseSyntax invokedMethod)
        {
            FillReads(invokedMethod, false);
        }
        return reads.Values;

        void FillReads(SyntaxNode root, bool useFieldLocation)
        {
            var notAssigned = root.Parent.DescendantNodes().OfType<ExpressionSyntax>().Where(x => !IsLeftSideOfAssignment(x));
            // The ".Parent" is to go from the accessor statement to the accessor block
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

    protected override bool ShouldIgnoreAccessor(IMethodSymbol accessorMethod, Compilation compilation)
    {
        if (accessorMethod.GetFirstSyntaxRef() is not AccessorStatementSyntax accessor
            || accessor.Parent.ContainsGetOrSetOnDependencyProperty(compilation))
        {
            return true;
        }

        // Special case: ignore the accessor if the only statement/expression is a throw.
        return accessor.DescendantNodes(x => x is StatementSyntax).Take(2).Count() == 1
            && accessor.DescendantNodes(x => x is ThrowStatementSyntax).Take(2).Count() == 1;
    }

    protected override bool ImplementsExplicitGetterOrSetter(IPropertySymbol property) =>
        HasExplicitAccessor(property.SetMethod)
        || HasExplicitAccessor(property.GetMethod);

    private static ExpressionSyntax SingleReturn(StatementSyntax body)
    {
        var returns = body.Parent.DescendantNodes().OfType<ReturnStatementSyntax>().ToArray();
        return returns.Length == 1 ? returns.Single().Expression : null;
    }

    private static ExpressionSyntax SingleInvocation(StatementSyntax body)
    {
        var expressions = body.Parent.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(x => x.Expression).ToArray();
        if (expressions.Length == 1)
        {
            var expr = expressions.Single();
            if (expr is IdentifierNameSyntax or MemberAccessExpressionSyntax { Expression: MeExpressionSyntax })
            {
                return expr;
            }
        }
        return null;
    }

    private static FieldData? ExtractFieldFromRefArgument(ArgumentSyntax argument, Compilation compilation, bool useFieldLocation)
    {
        var model = compilation.GetSemanticModel(argument.SyntaxTree);
        if (model is not null && argument.Parent is ArgumentListSyntax argList)
        {
            var argumentIndex = argList.Arguments.IndexOf(argument);
            if (model.GetSymbolInfo(argList.Parent).Symbol is IMethodSymbol methodSymbol
                && argumentIndex < methodSymbol.Parameters.Length
                && methodSymbol.Parameters[argumentIndex]?.RefKind != RefKind.None)
            {
                return ExtractFieldFromExpression(AccessorKind.Setter, argument.GetExpression(), compilation, useFieldLocation);
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

        var strippedExpression = expression.RemoveParentheses();

        // Check for direct field access: "Foo"
        if (strippedExpression is IdentifierNameSyntax && IsFieldOrWithEvents(out var directSymbol))
        {
            return new FieldData(accessorKind, directSymbol, strippedExpression, useFieldLocation);
        }
        // Check for "Me.Foo"
        else if (strippedExpression is MemberAccessExpressionSyntax { Expression: MeExpressionSyntax } member
            && model.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol field)
        {
            return new FieldData(accessorKind, field, member.Name, useFieldLocation);
        }

        return null;

        bool IsFieldOrWithEvents(out IFieldSymbol fieldSymbol)
        {
            var symbol = model.GetSymbolInfo(strippedExpression).Symbol;
            if (symbol is IFieldSymbol strippedExpressionSymbol)
            {
                fieldSymbol = strippedExpressionSymbol;
                return true;
            }
            else if (symbol is IPropertySymbol { IsWithEvents: true } property)
            {
                fieldSymbol = property.ContainingType.GetMembers("_" + property.Name).OfType<IFieldSymbol>().SingleOrDefault();
                return fieldSymbol is not null;
            }
            else
            {
                fieldSymbol = null;
                return false;
            }
        }
    }

    private static bool IsLeftSideOfAssignment(ExpressionSyntax expression)
    {
        var strippedExpression = expression.RemoveParentheses();
        return strippedExpression.IsLeftSideOfAssignment()
            || (strippedExpression.Parent is ExpressionSyntax parent && parent.IsLeftSideOfAssignment()); // for Me.field
    }

    private static bool HasExplicitAccessor(ISymbol symbol) =>
        symbol.GetFirstSyntaxRef() is AccessorStatementSyntax accessor
        && accessor.Parent.DescendantNodes().Any();
}
