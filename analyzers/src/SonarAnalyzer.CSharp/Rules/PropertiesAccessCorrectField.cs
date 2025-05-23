﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PropertiesAccessCorrectField : PropertiesAccessCorrectFieldBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override IEnumerable<FieldData> FindFieldAssignments(IPropertySymbol property, Compilation compilation)
        {
            if (property.SetMethod.GetFirstSyntaxRef() is not AccessorDeclarationSyntax setter)
            {
                return Enumerable.Empty<FieldData>();
            }

            // we only keep information for the first location of the symbol
            var assignments = new Dictionary<IFieldSymbol, FieldData>();
            FillAssignments(assignments, compilation, setter, true);

            // If there're no candidate variables, we'll try to inspect one local method invocation with value as argument
            if (assignments.Count == 0
                && (setter.ExpressionBody()?.Expression ?? SingleInvocation(setter.Body)) is { } expression
                && FindInvokedMethod(compilation, property.ContainingType, expression) is MethodDeclarationSyntax invokedMethod)
            {
                FillAssignments(assignments, compilation, invokedMethod, false);
            }

            return assignments.Values;
        }

        protected override IEnumerable<FieldData> FindFieldReads(IPropertySymbol property, Compilation compilation)
        {
            // We don't handle properties with multiple returns that return different fields
            if (property.GetMethod.GetFirstSyntaxRef() is not AccessorDeclarationSyntax getter)
            {
                return Enumerable.Empty<FieldData>();
            }

            var reads = new Dictionary<IFieldSymbol, FieldData>();
            FillReads(getter, true);

            // If there're no candidate variables, we'll try inspect one return of local method invocation
            if (reads.Count == 0
                && (getter.ExpressionBody()?.Expression ?? SingleReturn(getter.Body)) is InvocationExpressionSyntax returnExpression
                && FindInvokedMethod(compilation, property.ContainingType, returnExpression) is MethodDeclarationSyntax invokedMethod)
            {
                FillReads(invokedMethod, false);
            }
            return reads.Values;

            void FillReads(SyntaxNode root, bool useFieldLocation)
            {
                var notAssigned = root.DescendantNodes().OfType<ExpressionSyntax>().Where(n => !IsLeftSideOfAssignment(n));
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
            if (accessorMethod.GetFirstSyntaxRef() is not AccessorDeclarationSyntax accessor
                || ((SyntaxNode)accessor.Body ?? accessor).ContainsGetOrSetOnDependencyProperty(compilation))
            {
                return true;
            }

            // Special case: ignore the accessor if the only statement/expression is a throw.
            if (accessor.Body == null)
            {
                // Expression-bodied syntax
                return accessor.DescendantNodes().FirstOrDefault() is ArrowExpressionClauseSyntax arrowClause
                       && ThrowExpressionSyntaxWrapper.IsInstance(arrowClause.Expression);
            }

            // Statement-bodied syntax
            return  accessor.Body.DescendantNodes().Count(n => n is StatementSyntax) == 1
                    && accessor.Body.DescendantNodes().Count(n => n is ThrowStatementSyntax) == 1;
        }

        protected override bool ImplementsExplicitGetterOrSetter(IPropertySymbol property) =>
            HasExplicitAccessor(property.SetMethod)
            || HasExplicitAccessor(property.GetMethod);

        private static void FillAssignments(IDictionary<IFieldSymbol, FieldData> assignments, Compilation compilation, SyntaxNode root, bool useFieldLocation)
        {
            foreach (var node in root.DescendantNodes())
            {
                FieldData? foundField = null;
                if (node is AssignmentExpressionSyntax assignment)
                {
                    foundField = assignment.Left.DescendantNodesAndSelf().OfType<ExpressionSyntax>()
                        .Select(x => ExtractFieldFromExpression(AccessorKind.Setter, x, compilation, useFieldLocation))
                        .FirstOrDefault(x => x != null);
                }
                else if (node is ArgumentSyntax argument && argument.RefOrOutKeyword.IsAnyKind(SyntaxKind.RefKeyword,SyntaxKind.OutKeyword))
                {
                    foundField = ExtractFieldFromExpression(AccessorKind.Setter, argument.Expression, compilation, useFieldLocation);
                }
                if (foundField.HasValue && !assignments.ContainsKey(foundField.Value.Field))
                {
                    assignments.Add(foundField.Value.Field, foundField.Value);
                }
            }
        }

        private static ExpressionSyntax SingleReturn(SyntaxNode body)
        {
            if (body == null)
            {
                return null;
            }

            var returns = body.DescendantNodes().OfType<ReturnStatementSyntax>().ToArray();
            return returns.Length == 1 ? returns.Single().Expression : null;
        }

        private static ExpressionSyntax SingleInvocation(SyntaxNode body)
        {
            if (body == null)
            {
                return null;
            }

            var expressions = body.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(x => x.Expression).ToArray();
            if (expressions.Length == 1)
            {
                var expr = expressions.Single();
                if (expr is IdentifierNameSyntax
                    || (expr is MemberAccessExpressionSyntax {Expression: ThisExpressionSyntax _}))
                {
                    return expr;
                }
            }
            return null;
        }

        private static FieldData? ExtractFieldFromExpression(AccessorKind accessorKind, ExpressionSyntax expression, Compilation compilation, bool useFieldLocation)
        {
            var semanticModel = compilation.GetSemanticModel(expression.SyntaxTree);
            if (semanticModel == null)
            {
                return null;
            }

            var strippedExpression = expression.RemoveParentheses();

            // Check for direct field access: "foo"
            if (strippedExpression is IdentifierNameSyntax
                && semanticModel.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol directAccessField)
            {
                return new FieldData(accessorKind, directAccessField, strippedExpression, useFieldLocation);
            }
            // Check for "this.foo"
            else if (strippedExpression is MemberAccessExpressionSyntax {Expression: ThisExpressionSyntax _} member
                     && semanticModel.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol fieldAccessedWithThis)
            {
                return new FieldData(accessorKind, fieldAccessedWithThis, member.Name, useFieldLocation);
            }
            else if (strippedExpression is AssignmentExpressionSyntax assignmentExpression
                     && assignmentExpression.Parent is ReturnStatementSyntax
                     && semanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol is IFieldSymbol fieldAssignedFromExpression)
            {
                return new FieldData(accessorKind, fieldAssignedFromExpression, assignmentExpression.Left, useFieldLocation);
            }

            return null;
        }

        private static bool IsLeftSideOfAssignment(ExpressionSyntax expression)
        {
            var strippedExpression = expression.RemoveParentheses();
            return strippedExpression.IsLeftSideOfAssignment()
                   || (strippedExpression.Parent is ExpressionSyntax parent && parent.IsLeftSideOfAssignment()); // for this.field
        }

        private static bool HasExplicitAccessor(ISymbol symbol) =>
            symbol.GetFirstSyntaxRef() is AccessorDeclarationSyntax accessor
            && accessor.DescendantNodes().Any();
    }
}
