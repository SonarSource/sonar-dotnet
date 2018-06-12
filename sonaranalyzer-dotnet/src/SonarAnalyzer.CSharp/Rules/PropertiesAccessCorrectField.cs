/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
    public sealed class PropertiesAccessCorrectField : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4275";
        private const string MessageFormat = "Refactor this {0} so that it actually refers to the field '{1}'";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            // We want to check the fields read and assigned in all properties in this class
            // so this is a symbol-level rule (also means the callback is called only once
            // for partial classes)
            context.RegisterSymbolAction(c => CheckType(c),
                SymbolKind.NamedType);
        }

        private void CheckType(SymbolAnalysisContext context)
        {
            var symbol = (INamedTypeSymbol)context.Symbol;
            if (symbol.TypeKind != TypeKind.Class &&
                symbol.TypeKind != TypeKind.Structure)
            {
                return;
            }

            var fields = symbol.GetMembers().Where(m => m.Kind == SymbolKind.Field).OfType<IFieldSymbol>();

            if (!fields.Any())
            {
                return;
            }

            var properties = symbol.GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsImplicitlyDeclared);
            if (!properties.Any())
            {
                return;
            }

            var fieldToStandardNameMap = new Dictionary<IFieldSymbol, string>();
            foreach (var field in fields)
            {
                fieldToStandardNameMap[field] = GetCanonicalFieldName(field.Name);
            }

            var allPropertyData = CollectPropertyData(properties, context.Compilation);

            // Check that if there is a single matching field name it is used by the property
            foreach (var data in allPropertyData)
            {
                var matchingFields = fieldToStandardNameMap.Keys.Where(k => AreCanonicalNamesEqual(fieldToStandardNameMap[k], data.CanonicalName));
                if (matchingFields.Count() == 1)
                {
                    var field = matchingFields.Single();

                    if (data.FieldReturned.HasValue && data.FieldReturned.Value.Field != field)
                    {
                        context.ReportDiagnosticWhenActive(Diagnostic.Create(
                            rule,
                            data.FieldReturned.Value.Location,
                            "getter",
                            field.Name
                            ));
                    }

                    if (data.FieldUpdated.HasValue && data.FieldUpdated.Value.Field != field)
                    {
                        context.ReportDiagnosticWhenActive(Diagnostic.Create(
                            rule,
                            data.FieldUpdated.Value.Location,
                            "setter",
                            field.Name
                            ));
                    }
                }
            }
        }

        private static string GetCanonicalFieldName(string name)
            => name.Replace("_", string.Empty);

        private static bool AreCanonicalNamesEqual(string name1, string name2)
            => name1.Equals(name2, System.StringComparison.OrdinalIgnoreCase);

        private static IList<PropertyData> CollectPropertyData(IEnumerable<IPropertySymbol> properties, Compilation compilation)
        {
            IList<PropertyData> allPropertyData = new List<PropertyData>();

            // Collect the list of fields read/written by each property
            foreach (var property in properties)
            {
                var returned = FindReturnedField(property, compilation);
                var updated = FindFieldAssignment(property, compilation);
                var data = new PropertyData(property, returned, updated);
                allPropertyData.Add(data);
            }
            return allPropertyData;
        }

        private static FieldData? FindFieldAssignment(IPropertySymbol property, Compilation compilation)
        {
            if (property.SetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorDeclarationSyntax accessor
                //// We assume that if there are multiple field assignments in a property
                //// then they are all to the same field
                && accessor.DescendantNodes().FirstOrDefault(n => n is ExpressionStatementSyntax) is ExpressionStatementSyntax expression
                && expression.Expression is AssignmentExpressionSyntax assignment
                && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return ExtractFieldFromExpression(assignment.Left, compilation);
            }

            return null;
        }

        private static FieldData? FindReturnedField(IPropertySymbol property, Compilation compilation)
        {
            // We don't handle properties with multiple returns that return different fields
            if (property.GetMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is AccessorDeclarationSyntax accessor
                && accessor.DescendantNodes().FirstOrDefault(n => n is ReturnStatementSyntax) is ReturnStatementSyntax returnStatement
                && returnStatement.Expression != null)
            {
                return ExtractFieldFromExpression(returnStatement.Expression, compilation);
            }
            return null;
        }

        private static FieldData? ExtractFieldFromExpression(ExpressionSyntax expression,
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
                return new FieldData(field, strippedExpression.GetLocation());
            }
            else
            {
                // Check for "this.foo"
                if (strippedExpression is MemberAccessExpressionSyntax member
                  && member.Expression is ThisExpressionSyntax thisExpression
                  && semanticModel.GetSymbolInfo(expression).Symbol is IFieldSymbol field2)
                {
                    return new FieldData(field2, member.Name.GetLocation());
                }
            }

            return null;
        }

        private struct PropertyData
        {
            public PropertyData(IPropertySymbol propertySymbol, FieldData? returned, FieldData? updated)
            {
                this.PropertySymbol = propertySymbol;
                this.CanonicalName = GetCanonicalFieldName(propertySymbol.Name);
                this.FieldReturned = returned;
                this.FieldUpdated = updated;
            }

            public IPropertySymbol PropertySymbol { get; }

            public string CanonicalName { get; }

            public FieldData? FieldReturned { get; }

            public FieldData? FieldUpdated { get; }
        }

        private struct FieldData
        {
            public FieldData(IFieldSymbol field, Location location)
            {
                this.Field = field;
                this.Location = location;
            }

            public IFieldSymbol Field { get; }

            public Location Location { get; }
        }
    }
}
