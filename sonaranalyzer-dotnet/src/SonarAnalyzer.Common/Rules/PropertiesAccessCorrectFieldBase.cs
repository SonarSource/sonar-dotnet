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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class PropertiesAccessCorrectFieldBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S4275";
        protected const string MessageFormat = "Refactor this {0} so that it actually refers to the field '{1}'.";

        protected abstract DiagnosticDescriptor Rule { get; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            // We want to check the fields read and assigned in all properties in this class
            // so this is a symbol-level rule (also means the callback is called only once
            // for partial classes)
            context.RegisterSymbolAction(CheckType, SymbolKind.NamedType);
        }

        protected void CheckType(SymbolAnalysisContext context)
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

            var properties = GetExplictlyDeclaredProperties(symbol);
            if (!properties.Any())
            {
                return;
            }

            var propertyToFieldMatcher = new PropertyToFieldMatcher(fields);
            var allPropertyData = CollectPropertyData(properties, context.Compilation);

            // Check that if there is a single matching field name it is used by the property
            foreach (var data in allPropertyData)
            {
                var expectedField = propertyToFieldMatcher.GetSingleMatchingFieldOrNull(data.PropertySymbol);
                if (expectedField != null)
                {
                    if (!data.IgnoreGetter)
                    {
                        CheckExpectedFieldIsUsed(data.PropertySymbol.GetMethod, expectedField, data.ReadFields, context);
                    }
                    if (!data.IgnoreSetter)
                    {
                        CheckExpectedFieldIsUsed(data.PropertySymbol.SetMethod, expectedField, data.UpdatedFields, context);
                    }
                }
            }
        }

        protected IEnumerable<IPropertySymbol> GetExplictlyDeclaredProperties(INamedTypeSymbol symbol) =>
            symbol.GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>()
                .Where(p => ImplementsExplicitGetterOrSetter(p));

        protected void CheckExpectedFieldIsUsed(IMethodSymbol methodSymbol, IFieldSymbol expectedField, IEnumerable<FieldData> actualFields, SymbolAnalysisContext context)
        {
            var expectedFieldIsUsed = actualFields.Any(a => a.Field == expectedField);
            if (!expectedFieldIsUsed || !actualFields.Any())
            {
                var locationAndAccessorType = GetLocationAndAccessor(actualFields, methodSymbol);
                if (locationAndAccessorType.Item1 != null)
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(
                        Rule,
                        locationAndAccessorType.Item1,
                        locationAndAccessorType.Item2,
                        expectedField.Name
                        ));
                }
            }

            Tuple<Location, string> GetLocationAndAccessor(IEnumerable<FieldData> fields, IMethodSymbol method)
            {
                Location location = null;
                string accessorType = null;
                if (fields.Count() == 1)
                {
                    var fieldWithValue = fields.First();
                    location = fieldWithValue.LocationNode.GetLocation();
                    accessorType = fieldWithValue.AccessorKind == AccessorKind.Getter ? "getter" : "setter";
                }
                else
                {
                    Debug.Assert(method != null, "Method symbol should not be null at this point");
                    location = method?.Locations.First();
                    accessorType = method?.MethodKind == MethodKind.PropertyGet ? "getter" : "setter";
                }
                return Tuple.Create(location, accessorType);
            }
        }

        protected IList<PropertyData> CollectPropertyData(IEnumerable<IPropertySymbol> properties, Compilation compilation)
        {
            IList<PropertyData> allPropertyData = new List<PropertyData>();

            // Collect the list of fields read/written by each property
            foreach (var property in properties)
            {
                var readFields = FindFieldReads(property, compilation);
                var updatedFields = FindFieldAssignments(property, compilation);
                var ignoreGetter = ShouldIgnoreAccessor(property.GetMethod);
                var ignoreSetter = ShouldIgnoreAccessor(property.SetMethod);
                var data = new PropertyData(property, readFields, updatedFields, ignoreGetter, ignoreSetter);
                allPropertyData.Add(data);
            }
            return allPropertyData;
        }

        /**
         * Assignments can be done either
         * - directly via an assignment
         * - indirectly, when passed as 'out' or 'ref' parameter
         */
        protected abstract IEnumerable<FieldData> FindFieldAssignments(IPropertySymbol property, Compilation compilation);
        protected abstract IEnumerable<FieldData> FindFieldReads(IPropertySymbol property, Compilation compilation);
        protected abstract bool ImplementsExplicitGetterOrSetter(IPropertySymbol property);
        protected abstract bool ShouldIgnoreAccessor(IMethodSymbol accessorMethod);


        protected struct PropertyData
        {
            public PropertyData(IPropertySymbol propertySymbol, IEnumerable<FieldData> read, IEnumerable<FieldData> updated,
                bool ignoreGetter, bool ignoreSetter)
            {
                PropertySymbol = propertySymbol;
                ReadFields = read;
                UpdatedFields = updated;
                IgnoreGetter = ignoreGetter;
                IgnoreSetter = ignoreSetter;
            }

            public IPropertySymbol PropertySymbol { get; }

            public IEnumerable<FieldData> ReadFields { get; }

            public IEnumerable<FieldData> UpdatedFields { get; }

            public bool IgnoreGetter { get; }

            public bool IgnoreSetter { get; }
        }

        protected enum AccessorKind
        {
            Getter,
            Setter
        }

        protected struct FieldData
        {
            public FieldData(AccessorKind accessor, IFieldSymbol field, SyntaxNode locationNode)
            {
                AccessorKind = accessor;
                Field = field;
                LocationNode = locationNode;
            }

            public AccessorKind AccessorKind { get; }

            public IFieldSymbol Field { get; }

            public SyntaxNode LocationNode { get; }
        }

        /// <summary>
        /// The rule decides if a property is returning/settings the expected field.
        /// We decide what the expected field name should be based on a fuzzy match
        /// between the field name and the property name.
        /// This class hides the details of matching logic.
        /// </summary>
        protected class PropertyToFieldMatcher
        {
            private readonly IDictionary<IFieldSymbol, string> fieldToStandardNameMap;

            public PropertyToFieldMatcher(IEnumerable<IFieldSymbol> fields)
            {
                // Calcuate and cache the standardised versions of the field names to avoid
                // calculating them every time
                this.fieldToStandardNameMap = fields.ToDictionary(f => f, f => GetCanonicalFieldName(f.Name));
            }

            public IFieldSymbol GetSingleMatchingFieldOrNull(IPropertySymbol propertySymbol)
            {
                // We're not caching the property name as only expect to be called once per property
                var standardisedPropertyName = GetCanonicalFieldName(propertySymbol.Name);

                var matchingFields = this.fieldToStandardNameMap.Keys
                    .Where(k => AreCanonicalNamesEqual(this.fieldToStandardNameMap[k], standardisedPropertyName))
                    .ToList();

                if (matchingFields.Count != 1)
                {
                    return null;
                }
                return matchingFields[0];
            }

            private static string GetCanonicalFieldName(string name) =>
                name.Replace("_", string.Empty);

            private static bool AreCanonicalNamesEqual(string name1, string name2) =>
                name1.Equals(name2, System.StringComparison.OrdinalIgnoreCase);
        }

    }
}
