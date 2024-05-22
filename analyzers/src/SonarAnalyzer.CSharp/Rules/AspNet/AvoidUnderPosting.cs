/*
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

using System.Collections.Concurrent;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnderPosting : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6964";
    private const string MessageFormat = "Property used as input in a controller action should be nullable or annotated with the Required attribute to avoid under-posting.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly ImmutableArray<KnownType> IgnoredTypes = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Http_IFormCollection,
        KnownType.Microsoft_AspNetCore_Http_IFormFile,
        KnownType.Microsoft_AspNetCore_Http_IFormFileCollection);
    private static readonly ImmutableArray<KnownType> ValidationAttributes = ImmutableArray.Create(
        KnownType.System_ComponentModel_DataAnnotations_RequiredAttribute,
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
            }, SymbolKind.NamedType);
        });

    private static void ProcessControllerMethods(SonarSyntaxNodeReportingContext context, ConcurrentDictionary<ITypeSymbol, bool> examinedTypes)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is IMethodSymbol method
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
            .Where(x => !CanBeNull(x.Type) && !x.HasAnyAttribute(ValidationAttributes))
            .Select(x => x.DeclaringSyntaxReferences[0].GetSyntax())
            .Where(x => !x.GetModifiers().Any(x => x.IsKind(SyntaxKindEx.RequiredKeyword))); // ToDo: Check with IProperty.IsRequired once available
        foreach (var property in invalidProperties)
        {
            context.ReportIssue(Rule, property.GetLocation());
        }
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
            INamedTypeSymbol namedType when type.IsInSameAssembly(controllerType) => [namedType],
            _ => []
        };

    private static bool HasValidateNeverAttribute(ISymbol symbol) =>
        symbol.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ModelBinding_Validation_ValidateNeverAttribute);
}
