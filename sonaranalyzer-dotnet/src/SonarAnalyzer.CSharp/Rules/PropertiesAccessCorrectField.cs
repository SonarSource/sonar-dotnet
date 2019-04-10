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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class PropertiesAccessCorrectField : PropertiesAccessCorrectFieldBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override DiagnosticDescriptor Rule => rule;

        protected override IEnumerable<FieldData> FindFieldAssignments(IPropertySymbol property, Compilation compilation)
        {
            if (!(property.SetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorDeclarationSyntax setter))
            {
                return Enumerable.Empty<FieldData>();
            }

            // we only keep information for the first location of the symbol
            var assignments = new Dictionary<IFieldSymbol, FieldData>();
            foreach (var node in setter.DescendantNodes())
            {
                FieldData? foundField = null;
                if (node is AssignmentExpressionSyntax assignment && node.IsKind(SyntaxKind.SimpleAssignmentExpression))
                {
                    foundField = ExtractFieldFromExpression(AccessorKind.Setter, assignment.Left, compilation);
                }
                else if (node is ArgumentSyntax argument && IsRefOrOut(argument.RefOrOutKeyword))
                {
                    foundField = ExtractFieldFromExpression(AccessorKind.Setter, argument.Expression, compilation);
                }
                if (foundField.HasValue && !assignments.ContainsKey(foundField.Value.Field))
                {
                    assignments.Add(foundField.Value.Field, foundField.Value);
                }
            }
            return assignments.Values;

            bool IsRefOrOut(SyntaxToken node) => node.IsKind(SyntaxKind.RefKeyword) || node.IsKind(SyntaxKind.OutKeyword);
        }

        protected override IEnumerable<FieldData> FindFieldReads(IPropertySymbol property, Compilation compilation)
        {
            // We don't handle properties with multiple returns that return different fields
            if (!(property.GetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorDeclarationSyntax getter))
            {
                return Enumerable.Empty<FieldData>();
            }

            var reads = new Dictionary<IFieldSymbol, FieldData>();
            var notAssigned = getter.DescendantNodes().Select(n => n as ExpressionSyntax)
                .WhereNotNull().Where(n => !IsLeftSideOfAssignment(n));
            foreach (var expression in notAssigned)
            {
                var readField = ExtractFieldFromExpression(AccessorKind.Getter, expression, compilation);
                // we only keep information for the first location of the symbol
                if (readField.HasValue && !reads.ContainsKey(readField.Value.Field))
                {
                    reads.Add(readField.Value.Field, readField.Value);
                }
            }
            return reads.Values;
        }

        protected override bool ShouldIgnoreAccessor(IMethodSymbol accessorMethod)
        {
            if (!(accessorMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorDeclarationSyntax accessor))
            {
                // no accessor
                return true;
            }
            // Special case: ignore the accessor if the only statement/expression is a throw.
            if (accessor.Body == null)
            {
                // Expression-bodied syntax
                return accessor.DescendantNodes().FirstOrDefault() is ArrowExpressionClauseSyntax arrowClause
                    && ShimLayer.CSharp.ThrowExpressionSyntaxWrapper.IsInstance(arrowClause.Expression);
            }
            // Statement-bodied syntax
            return (accessor.Body.DescendantNodes().Count(n => n is StatementSyntax) == 1 &&
                accessor.Body.DescendantNodes().Count(n => n is ThrowStatementSyntax) == 1);
        }

        protected override bool ImplementsExplicitGetterOrSetter(IPropertySymbol property) =>
            (property.SetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorDeclarationSyntax setter &&
            setter.DescendantNodes().Any()) ||
            (property.GetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorDeclarationSyntax getter &&
            getter.DescendantNodes().Any());

        private static FieldData? ExtractFieldFromExpression(AccessorKind accessorKind,
            ExpressionSyntax expression,
            Compilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(expression.SyntaxTree);
            if (semanticModel == null)
            {
                return null;
            }

            var strippedExpression = expression.RemoveParentheses();

            // Check for direct field access: "foo"
            if (strippedExpression is IdentifierNameSyntax &&
                semanticModel.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol field)
            {
                return new FieldData(accessorKind, field, strippedExpression);
            }
            // Check for "this.foo"
            else if (strippedExpression is MemberAccessExpressionSyntax member &&
                member.Expression is ThisExpressionSyntax thisExpression &&
                semanticModel.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol field2)
            {
                return new FieldData(accessorKind, field2, member.Name);
            }

            return null;
        }

        private static bool IsLeftSideOfAssignment(ExpressionSyntax expression)
        {
            var strippedExpression = expression.RemoveParentheses();
            return strippedExpression.IsLeftSideOfAssignment() ||
                // for this.field
                (strippedExpression.Parent is ExpressionSyntax parent && parent.IsLeftSideOfAssignment());
        }
    }
}
