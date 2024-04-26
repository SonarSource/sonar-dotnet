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
        KnownType.System_Boolean,
        KnownType.System_Byte,
        KnownType.System_SByte,
        KnownType.System_Int16,
        KnownType.System_UInt16,
        KnownType.System_Int32,
        KnownType.System_UInt32,
        KnownType.System_Int64,
        KnownType.System_UInt64,
        KnownType.System_Char,
        KnownType.System_Single,
        KnownType.System_Double,
        KnownType.System_Decimal,
        KnownType.System_String,
        KnownType.System_Object,
        KnownType.Microsoft_AspNetCore_Http_IFormCollection,
        KnownType.Microsoft_AspNetCore_Http_IFormFile,
        KnownType.Microsoft_AspNetCore_Http_IFormFileCollection);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
        {
            if (!compilationStart.Compilation.ReferencesControllers())
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
            && method.IsControllerMethod())
        {
            var modelParameterTypes = method.Parameters
                .Select(x => x.Type)
                .OfType<INamedTypeSymbol>()
                .Where(x => !IgnoreType(x))
                .SelectMany(x => RelatedTypesToExamine(x))
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
        var invalidProperties = declaredProperties.Where(x => !CanBeNull(x.Type)
            && !x.HasAttribute(KnownType.System_ComponentModel_DataAnnotations_RequiredAttribute)
            && !x.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ModelBinding_Validation_ValidateNeverAttribute));
        foreach (var property in invalidProperties)
        {
            var propertySyntax = property.DeclaringSyntaxReferences[0].GetSyntax();
            context.ReportIssue(Rule, propertySyntax.GetLocation());
        }
    }

    private static bool IgnoreType(ITypeSymbol type) =>
        type.IsAny(IgnoredTypes)
        || (!type.DeclaringSyntaxReferences.Any()
            && !type.Is(KnownType.System_Collections_Generic_IEnumerable_T)
            && !type.Implements(KnownType.System_Collections_Generic_IEnumerable_T));

    private static bool CanBeNull(ITypeSymbol type) =>
        type is ITypeParameterSymbol { HasValueTypeConstraint: false }
        || (type.IsReferenceType && type.NullableAnnotation() != NullableAnnotation.NotAnnotated)
        || type.IsNullableValueType();

    private static void GetAllDeclaredProperties(ITypeSymbol type, ConcurrentDictionary<ITypeSymbol, bool> examinedTypes, List<IPropertySymbol> declaredProperties)
    {
        if (type is INamedTypeSymbol namedType && examinedTypes.TryAdd(namedType, true))
        {
            var properties = namedType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.GetEffectiveAccessibility() == Accessibility.Public
                    && x.SetMethod?.DeclaredAccessibility is Accessibility.Public);
            foreach (var property in properties.Where(x => x.DeclaringSyntaxReferences.Length > 0))
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

    private static IEnumerable<INamedTypeSymbol> RelatedTypesToExamine(INamedTypeSymbol type) =>
        type.DerivesOrImplements(KnownType.System_Collections_Generic_IEnumerable_T)
            ? type.TypeArguments.OfType<INamedTypeSymbol>()
            : [type];
}
