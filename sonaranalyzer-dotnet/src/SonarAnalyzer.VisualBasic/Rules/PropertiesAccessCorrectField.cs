/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class PropertiesAccessCorrectField : PropertiesAccessCorrectFieldBase
    {
        public PropertiesAccessCorrectField() : base(RspecStrings.ResourceManager) { }

        protected override IEnumerable<FieldData> FindFieldAssignments(IPropertySymbol property, Compilation compilation)
        {
            if (!(property.SetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorStatementSyntax setter))
            {
                return Enumerable.Empty<FieldData>();
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
                    if (node is AssignmentStatementSyntax assignment && assignment.IsKind(SyntaxKind.SimpleAssignmentStatement))
                    {
                        foundField = assignment.Left.DescendantNodesAndSelf().OfType<ExpressionSyntax>()
                            .Select(x => ExtractFieldFromExpression(AccessorKind.Setter, x, compilation, useFieldLocation))
                            .FirstOrDefault(x => x != null);
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
            if (!(property.GetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorStatementSyntax getter))
            {
                return Enumerable.Empty<FieldData>();
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
                var notAssigned = root.Parent.DescendantNodes().OfType<ExpressionSyntax>().Where(n => !IsLeftSideOfAssignment(n));
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

        protected override bool ShouldIgnoreAccessor(IMethodSymbol accessorMethod)
        {
            if (!(accessorMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorStatementSyntax accessor))
            {
                // no accessor
                return true;
            }
            // Special case: ignore the accessor if the only statement/expression is a throw.
            return accessor.DescendantNodes(n => n is StatementSyntax).Count() == 1 &&
                accessor.DescendantNodes(n => n is ThrowStatementSyntax).Count() == 1;
        }

        protected override bool ImplementsExplicitGetterOrSetter(IPropertySymbol property) =>
            (property.SetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorStatementSyntax setter &&
            setter.Parent.DescendantNodes().Any()) ||
            (property.GetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorStatementSyntax getter &&
            getter.Parent.DescendantNodes().Any());

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
                if (expr is IdentifierNameSyntax
                    || (expr is MemberAccessExpressionSyntax member && member.Expression is MeExpressionSyntax))
                {
                    return expr;
                }
            }
            return null;
        }

        private static FieldData? ExtractFieldFromRefArgument(ArgumentSyntax argument, Compilation compilation, bool useFieldLocation)
        {
            var semanticModel = compilation.GetSemanticModel(argument.SyntaxTree);
            if (semanticModel != null && argument.Parent is ArgumentListSyntax argList)
            {
                var argumentIndex = argList.Arguments.IndexOf(argument);
                if (semanticModel.GetSymbolInfo(argList.Parent).Symbol is IMethodSymbol methodSymbol &&
                    argumentIndex < methodSymbol?.Parameters.Length &&
                    methodSymbol?.Parameters[argumentIndex]?.RefKind != RefKind.None)
                {
                    return ExtractFieldFromExpression(AccessorKind.Setter, argument.GetExpression(), compilation, useFieldLocation);
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

            // Check for direct field access: "Foo"
            if (strippedExpression is IdentifierNameSyntax && IsFieldOrWithEvents(out var directSymbol))
            {
                return new FieldData(accessorKind, directSymbol, strippedExpression, useFieldLocation);
            }
            // Check for "Me.Foo"
            else if (strippedExpression is MemberAccessExpressionSyntax member &&
                    member.Expression is MeExpressionSyntax &&
                    semanticModel.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol field)
            {
                return new FieldData(accessorKind, field, member.Name, useFieldLocation);
            }

            return null;

            bool IsFieldOrWithEvents(out IFieldSymbol fieldSymbol)
            {
                var symbol = semanticModel.GetSymbolInfo(strippedExpression).Symbol;
                if (symbol is IFieldSymbol)
                {
                    fieldSymbol = symbol as IFieldSymbol;
                    return true;
                }
                else if (symbol is IPropertySymbol property && property.IsWithEvents)
                {
                    fieldSymbol = property.ContainingType.GetMembers("_" + property.Name).OfType<IFieldSymbol>().SingleOrDefault();
                    return fieldSymbol != null;
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
            return strippedExpression.IsLeftSideOfAssignment() ||
                // for Me.field
                (strippedExpression.Parent is ExpressionSyntax parent && parent.IsLeftSideOfAssignment());
        }
    }
}
