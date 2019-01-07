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

        protected override FieldData? FindFieldAssignment(IPropertySymbol property, Compilation compilation)
        {
            if (property.SetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorDeclarationSyntax accessor &&
                // We assume that if there are multiple field assignments in a property
                // then they are all to the same field
                accessor.DescendantNodes().FirstOrDefault(n => n is ExpressionStatementSyntax) is ExpressionStatementSyntax expression &&
                expression.Expression is AssignmentExpressionSyntax assignment &&
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return ExtractFieldFromExpression(AccessorKind.Setter, assignment.Left, compilation);
            }

            return null;
        }

        protected override FieldData? FindReturnedField(IPropertySymbol property, Compilation compilation)
        {
            // We don't handle properties with multiple returns that return different fields
            if (property.GetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorDeclarationSyntax accessor &&
                accessor.DescendantNodes().FirstOrDefault(n => n.RawKind == (int)SyntaxKind.ReturnStatement) is ReturnStatementSyntax returnStatement &&
                returnStatement.Expression != null)
            {
                return ExtractFieldFromExpression(AccessorKind.Getter, returnStatement.Expression, compilation);
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
                    member.Expression is ThisExpressionSyntax thisExpression &&
                    semanticModel.GetSymbolInfo(strippedExpression).Symbol is IFieldSymbol field2)
                {
                    return new FieldData(accessorKind, field2, member.Name);
                }
            }

            return null;
        }

    }
}
