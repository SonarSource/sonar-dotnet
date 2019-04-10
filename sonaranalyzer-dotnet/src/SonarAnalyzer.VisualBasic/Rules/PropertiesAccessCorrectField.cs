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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class PropertiesAccessCorrectField : PropertiesAccessCorrectFieldBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override DiagnosticDescriptor Rule => rule;

        protected override IEnumerable<FieldData> FindFieldAssignments(IPropertySymbol property, Compilation compilation)
        {
            if (!(property.SetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorStatementSyntax setter))
            {
                return Enumerable.Empty<FieldData>();
            }

            // we only keep information for the first location of the symbol
            var assignments = new Dictionary<IFieldSymbol, FieldData>();

            // The ".Parent" is to go from the accessor statement to the accessor block
            foreach (var node in setter.Parent.DescendantNodes())
            {
                FieldData? foundField = null;
                if (node is AssignmentStatementSyntax assignment && assignment.IsKind(SyntaxKind.SimpleAssignmentStatement))
                {
                    foundField = ExtractFieldFromExpression(AccessorKind.Setter, assignment.Left, compilation);
                }
                else if (node is ArgumentSyntax argument)
                {
                    foundField = ExtractFieldFromRefArgument(argument, compilation);
                }
                if (foundField.HasValue && !assignments.ContainsKey(foundField.Value.Field))
                {
                    assignments.Add(foundField.Value.Field, foundField.Value);
                }
            }
            return assignments.Values;
        }

        protected override IEnumerable<FieldData> FindFieldReads(IPropertySymbol property, Compilation compilation)
        {
            // We don't handle properties with multiple returns that return different fields
            if (!(property.GetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorStatementSyntax getter))
            {
                return Enumerable.Empty<FieldData>();
            }

            var reads = new Dictionary<IFieldSymbol, FieldData>();
            var notAssigned = getter.Parent.DescendantNodes().Select(n => n as ExpressionSyntax)
                .WhereNotNull().Where(n => !IsLeftSideOfAssignment(n));
            // The ".Parent" is to go from the accessor statement to the accessor block
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

        private static FieldData? ExtractFieldFromRefArgument(ArgumentSyntax argument, Compilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(argument.SyntaxTree);
            if (semanticModel != null && argument.Parent is ArgumentListSyntax argList)
            {
                var argumentIndex = argList.Arguments.IndexOf(argument);
                if (semanticModel.GetSymbolInfo(argList.Parent).Symbol is IMethodSymbol methodSymbol &&
                    argumentIndex < methodSymbol?.Parameters.Length &&
                    methodSymbol?.Parameters[argumentIndex]?.RefKind != RefKind.None)
                {
                    return ExtractFieldFromExpression(AccessorKind.Setter, argument.GetExpression(), compilation);
                }
            }
            return null;
        }

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
            else
            {
                // Check for "this.foo"
                if (strippedExpression is MemberAccessExpressionSyntax member &&
                    member.Expression is MeExpressionSyntax thisExpression &&
                    semanticModel.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol field2)
                {
                    return new FieldData(accessorKind, field2, member.Name);
                }
            }

            return null;
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
