/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Collections.Concurrent;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnderPosting : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6964";
    private const string MessageFormat = "Value type property used as input in a controller action should be nullable, required or annotated with the JsonRequiredAttribute to avoid under-posting.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableArray<KnownType> IgnoredTypes = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Http_IFormCollection,
        KnownType.Microsoft_AspNetCore_Http_IFormFile,
        KnownType.Microsoft_AspNetCore_Http_IFormFileCollection);

    private static readonly ImmutableArray<KnownType> IgnoredAttributes = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_ModelBinding_BindNeverAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_ModelBinding_BindRequiredAttribute,
        KnownType.Newtonsoft_Json_JsonIgnoreAttribute,
        KnownType.Newtonsoft_Json_JsonRequiredAttribute,
        KnownType.System_ComponentModel_DataAnnotations_RangeAttribute,
        KnownType.System_Text_Json_Serialization_JsonIgnoreAttribute,
        KnownType.System_Text_Json_Serialization_JsonRequiredAttribute);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
            {
                if (!compilationStart.Compilation.ReferencesNetCoreControllers())
                {
                    return;
                }
                var examinedTypes = new ConcurrentDictionary<ITypeSymbol, bool>();

                compilationStart.RegisterSymbolStartAction(symbolStart =>
                    {
                        var type = (INamedTypeSymbol)symbolStart.Symbol;
                        if (type.IsControllerType())
                        {
                            symbolStart.RegisterSyntaxNodeAction(nodeContext => ProcessControllerMethods(nodeContext, examinedTypes), SyntaxKind.MethodDeclaration);
                        }
                    },
                    SymbolKind.NamedType);
            });

    private static void ProcessControllerMethods(SonarSyntaxNodeReportingContext context, ConcurrentDictionary<ITypeSymbol, bool> examinedTypes)
    {
        if (context.Model.GetDeclaredSymbol(context.Node) is IMethodSymbol method
            && method.IsControllerActionMethod())
        {
            var modelParameterTypes = method.Parameters
                .Where(x => !HasValidateNeverAttribute(x))
                .SelectMany(x => RelatedTypesToExamine(x.Type, method.ContainingType))
                .Distinct();
            foreach (var modelParameterType in modelParameterTypes)
            {
                CheckInvalidProperties(modelParameterType, context, examinedTypes);
            }
        }
    }

    private static void CheckInvalidProperties(INamedTypeSymbol parameterType, SonarSyntaxNodeReportingContext context, ConcurrentDictionary<ITypeSymbol, bool> examinedTypes)
    {
        var declaredProperties = new List<IPropertySymbol>();
        GetAllDeclaredProperties(parameterType, examinedTypes, declaredProperties);
        var invalidProperties = declaredProperties
            .Where(x => !IsExcluded(x))
            .Select(x => x.GetFirstSyntaxRef())
            .Where(x => !IsInitialized(x));
        foreach (var property in invalidProperties)
        {
            context.ReportIssue(Rule, property.GetIdentifier()?.GetLocation());
        }

        static bool IsExcluded(IPropertySymbol property) =>
            CanBeNull(property.Type)
            || property.HasAnyAttribute(IgnoredAttributes)
            || IsNewtonsoftJsonPropertyRequired(property)
            || property.IsRequired();

        static bool IsNewtonsoftJsonPropertyRequired(IPropertySymbol property) =>
            property.GetAttributes(KnownType.Newtonsoft_Json_JsonPropertyAttribute).FirstOrDefault() is { } attribute
            && attribute.TryGetAttributeValue("Required", out int required)
            && (required is 1 or 2); // Required.AllowNull = 1,   Required.Always = 2, https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Required.htm
    }

    private static bool IgnoreType(ITypeSymbol type) =>
        type is not INamedTypeSymbol namedType      // e.g. dynamic type
        || namedType.IsAny(IgnoredTypes)
        || !CanBeUsedInModelBinding(namedType);

    private static bool CanBeUsedInModelBinding(INamedTypeSymbol type) =>
        !type.IsTupleType()                                             // Tuples are not supported (unless a custom Model Binder is used)
        && (type.Constructors.Any(x => x.Parameters.Length == 0)        // The type must have a parameterless constructor, unless
            || type.IsValueType                                         // - it's a value type
            || type.IsRecord()                                          // - it's a record type
            || type.IsInterface()                                       // - it's an interface (although the type that implements will be actually used)
            || type.Is(KnownType.System_String));                       // - it has a custom Model Binder (e.g. System.String has one)

    private static bool CanBeNull(ITypeSymbol type) =>
        type is ITypeParameterSymbol { HasValueTypeConstraint: false }
        || type.IsReferenceType
        || type.IsNullableValueType();

    private static void GetAllDeclaredProperties(ITypeSymbol type, ConcurrentDictionary<ITypeSymbol, bool> examinedTypes, List<IPropertySymbol> declaredProperties)
    {
        if (type is INamedTypeSymbol namedType
            && examinedTypes.TryAdd(namedType, true)
            && !IgnoreType(namedType)
            && !HasValidateNeverAttribute(type))
        {
            var properties = namedType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.GetEffectiveAccessibility() == Accessibility.Public
                            && x.SetMethod?.DeclaredAccessibility is Accessibility.Public
                            && !HasValidateNeverAttribute(x)
                            && x.DeclaringSyntaxReferences.Length > 0
                            && !IgnoreType(x.Type));
            foreach (var property in properties)
            {
                declaredProperties.Add(property);
                if (property.Type.DeclaringSyntaxReferences.Length > 0)
                {
                    GetAllDeclaredProperties(property.Type, examinedTypes, declaredProperties);
                }
            }
            ITypeSymbol[] relatedTypes = [namedType.BaseType, .. namedType.TypeArguments];
            foreach (var relatedType in relatedTypes)
            {
                GetAllDeclaredProperties(relatedType, examinedTypes, declaredProperties);
            }
        }
    }

    // We only consider Model types that are in the same assembly as the Controller, because Roslyn can't raise an issue when the location is in a different assembly than the one being analyzed.
    private static IEnumerable<INamedTypeSymbol> RelatedTypesToExamine(ITypeSymbol type, ITypeSymbol controllerType) =>
        type switch
        {
            IArrayTypeSymbol array => RelatedTypesToExamine(array.ElementType, controllerType),
            INamedTypeSymbol collection when collection.DerivesOrImplements(KnownType.System_Collections_Generic_IEnumerable_T) =>
                collection.TypeArguments.SelectMany(x => RelatedTypesToExamine(x, controllerType)),
            INamedTypeSymbol namedType when type.IsInSameAssembly(controllerType)  && !type.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ModelBinding_BindNeverAttribute) =>
             [namedType],
            _ => []
        };

    private static bool HasValidateNeverAttribute(ISymbol symbol) =>
        symbol.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ModelBinding_Validation_ValidateNeverAttribute);

    private static bool IsInitialized(SyntaxNode node) =>
        node is ParameterSyntax { Default: not null } or PropertyDeclarationSyntax { Initializer: not null };
}
