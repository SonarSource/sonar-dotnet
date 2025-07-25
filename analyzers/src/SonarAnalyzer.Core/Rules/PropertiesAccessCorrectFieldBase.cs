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

namespace SonarAnalyzer.Core.Rules;

public abstract class PropertiesAccessCorrectFieldBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S4275";

    /**
     * Assignments can be done either
     * - directly via an assignment
     * - indirectly, when passed as 'out' or 'ref' parameter
     */
    protected enum AccessorKind
    {
        Getter,
        Setter
    }

    protected abstract IEnumerable<FieldData> FindFieldAssignments(IPropertySymbol property, Compilation compilation);
    protected abstract IEnumerable<FieldData> FindFieldReads(IPropertySymbol property, Compilation compilation);
    protected abstract bool ImplementsExplicitGetterOrSetter(IPropertySymbol property);
    protected abstract bool ShouldIgnoreAccessor(IMethodSymbol accessorMethod, Compilation compilation);

    protected override string MessageFormat => "Refactor this {0} so that it actually refers to the field '{1}'.";

    protected PropertiesAccessCorrectFieldBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        // We want to check the fields read and assigned in all properties in this class
        // so this is a symbol-level rule (also means the callback is called only once
        // for partial classes)
        context.RegisterSymbolAction(CheckType, SymbolKind.NamedType);

    protected static SyntaxNode FindInvokedMethod(Compilation compilation, INamedTypeSymbol containingType, SyntaxNode expression) =>
        compilation.GetSemanticModel(expression.SyntaxTree) is { } semanticModel
        && semanticModel.GetSymbolInfo(expression).Symbol is { } invocationSymbol
        && invocationSymbol.ContainingType.Equals(containingType)
        && invocationSymbol.DeclaringSyntaxReferences.Length == 1
        && invocationSymbol.DeclaringSyntaxReferences.Single().GetSyntax() is { } invokedMethod
            ? invokedMethod
            : null;

    private void CheckType(SonarSymbolReportingContext context)
    {
        var symbol = (INamedTypeSymbol)context.Symbol;
        if (!symbol.TypeKind.Equals(TypeKind.Class)
            && !symbol.TypeKind.Equals(TypeKind.Structure))
        {
            return;
        }

        var fields = SelfAndBaseTypesFieldSymbols(symbol);
        if (!fields.Any())
        {
            return;
        }

        var properties = ExplicitlyDeclaredProperties(symbol);
        if (!properties.Any())
        {
            return;
        }

        var propertyToFieldMatcher = new PropertyToFieldMatcher(fields);
        var allPropertyData = CollectPropertyData(properties, context.Compilation);

        // Check that if there is a single matching field name it is used by the property
        foreach (var data in allPropertyData)
        {
            var expectedField = propertyToFieldMatcher.SingleMatchingFieldOrNull(data.PropertySymbol);
            if (expectedField is not null)
            {
                if (!data.IgnoreGetter)
                {
                    CheckExpectedFieldIsUsed(context, data.PropertySymbol.GetMethod, expectedField, data.ReadFields);
                }
                if (!data.IgnoreSetter)
                {
                    CheckExpectedFieldIsUsed(context, data.PropertySymbol.SetMethod, expectedField, data.UpdatedFields);
                }
            }
        }
    }

    private static IEnumerable<IFieldSymbol> SelfAndBaseTypesFieldSymbols(INamedTypeSymbol typeSymbol)
    {
        var fieldSymbols = Enumerable.Empty<IFieldSymbol>();
        var selfAndBaseTypesSymbols = typeSymbol.GetSelfAndBaseTypes();
        foreach (var symbol in selfAndBaseTypesSymbols)
        {
            fieldSymbols = fieldSymbols.Concat(symbol.GetMembers().Where(x => x.Kind.Equals(SymbolKind.Field)).OfType<IFieldSymbol>());
        }
        return fieldSymbols;
    }

    private IEnumerable<IPropertySymbol> ExplicitlyDeclaredProperties(INamedTypeSymbol symbol) =>
        symbol.GetMembers()
            .Where(x => x.Kind.Equals(SymbolKind.Property))
            .SelectMany(x => x.AllPartialParts())
            .OfType<IPropertySymbol>()
            .Where(ImplementsExplicitGetterOrSetter);

    private void CheckExpectedFieldIsUsed(SonarSymbolReportingContext context, IMethodSymbol methodSymbol, IFieldSymbol expectedField, ImmutableArray<FieldData> actualFields)
    {
        var expectedFieldIsUsed = actualFields.Any(x => x.Field.Equals(expectedField));
        if (!expectedFieldIsUsed || !actualFields.Any())
        {
            var locationAndAccessorType = GetLocationAndAccessor(actualFields, methodSymbol);
            if (locationAndAccessorType.Item1 is not null)
            {
                context.ReportIssue(Language.GeneratedCodeRecognizer, Rule, locationAndAccessorType.Item1, locationAndAccessorType.Item2, expectedField.Name);
            }
        }

        static Tuple<Location, string> GetLocationAndAccessor(ImmutableArray<FieldData> fields, IMethodSymbol method)
        {
            Location location;
            string accessorType;
            if (fields.Count(x => x.UseFieldLocation) == 1)
            {
                var fieldWithValue = fields.First();
                location = fieldWithValue.LocationNode.GetLocation();
                accessorType = fieldWithValue.AccessorKind.Equals(AccessorKind.Getter) ? "getter" : "setter";
            }
            else
            {
                Debug.Assert(method is not null, "Method symbol should not be null at this point");
                location = method?.Locations.First();
                accessorType = method?.MethodKind == MethodKind.PropertyGet ? "getter" : "setter";
            }
            return Tuple.Create(location, accessorType);
        }
    }

    private IList<PropertyData> CollectPropertyData(IEnumerable<IPropertySymbol> properties, Compilation compilation)
    {
        IList<PropertyData> allPropertyData = [];

        // Collect the list of fields read/written by each property
        foreach (var property in properties)
        {
            var readFields = FindFieldReads(property, compilation);
            var updatedFields = FindFieldAssignments(property, compilation);
            var ignoreGetter = ShouldIgnoreAccessor(property.GetMethod, compilation);
            var ignoreSetter = ShouldIgnoreAccessor(property.SetMethod, compilation);
            var data = new PropertyData(property, readFields, updatedFields, ignoreGetter, ignoreSetter);
            allPropertyData.Add(data);
        }
        return allPropertyData;
    }

    private readonly struct PropertyData
    {
        public IPropertySymbol PropertySymbol { get; }

        public ImmutableArray<FieldData> ReadFields { get; }

        public ImmutableArray<FieldData> UpdatedFields { get; }

        public bool IgnoreGetter { get; }

        public bool IgnoreSetter { get; }

        public PropertyData(IPropertySymbol propertySymbol, IEnumerable<FieldData> read, IEnumerable<FieldData> updated, bool ignoreGetter, bool ignoreSetter)
        {
            PropertySymbol = propertySymbol;
            ReadFields = read.ToImmutableArray();
            UpdatedFields = updated.ToImmutableArray();
            IgnoreGetter = ignoreGetter;
            IgnoreSetter = ignoreSetter;
        }
    }

    protected readonly struct FieldData
    {
        public AccessorKind AccessorKind { get; }

        public IFieldSymbol Field { get; }

        public SyntaxNode LocationNode { get; }

        public bool UseFieldLocation { get; }

        public FieldData(AccessorKind accessor, IFieldSymbol field, SyntaxNode locationNode, bool useFieldLocation)
        {
            AccessorKind = accessor;
            Field = field;
            LocationNode = locationNode;
            UseFieldLocation = useFieldLocation;
        }
    }

    /// <summary>
    /// The rule decides if a property is returning/settings the expected field.
    /// We decide what the expected field name should be based on a fuzzy match
    /// between the field name and the property name.
    /// This class hides the details of matching logic.
    /// </summary>
    private class PropertyToFieldMatcher
    {
        private readonly IDictionary<IFieldSymbol, string> fieldToStandardNameMap;

        public PropertyToFieldMatcher(IEnumerable<IFieldSymbol> fields) =>
            // Calculate and cache the standardised versions of the field names to avoid
            // calculating them every time
            fieldToStandardNameMap = fields.ToDictionary(x => x, x => CanonicalName(x.Name));

        public IFieldSymbol SingleMatchingFieldOrNull(IPropertySymbol propertySymbol)
        {
            var matchingFields = fieldToStandardNameMap.Keys
                .Where(x => FieldMatchesTheProperty(x, propertySymbol))
                .Take(2)
                .ToList();

            return matchingFields.Count != 1
                ? null
                : matchingFields[0];
        }

        private static string CanonicalName(string name) =>
            name.Replace("_", string.Empty);

        private static bool AreCanonicalNamesEqual(string name1, string name2) =>
            name1.Equals(name2, StringComparison.OrdinalIgnoreCase);

        private bool FieldMatchesTheProperty(IFieldSymbol field, IPropertySymbol property) =>
            // We're not caching the property name as only expect to be called once per property
            !field.IsConst
            && ((property.IsStatic && field.IsStatic) || (!property.IsStatic && !field.IsStatic))
            && field.Type.Equals(property.Type)
            && AreCanonicalNamesEqual(fieldToStandardNameMap[field], CanonicalName(property.Name));
    }
}
