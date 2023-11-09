/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BlazorQueryParameterTypeShouldBeSupported : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6797";
    private const string MessageFormat = "Query parameter type '{0}' is not supported.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ISet<KnownType> SupportedTypes = new HashSet<KnownType>
    {
        KnownType.System_Boolean,
        KnownType.System_DateTime,
        KnownType.System_Decimal,
        KnownType.System_Double,
        KnownType.System_Single,
        KnownType.System_Int32,
        KnownType.System_Int64,
        KnownType.System_String,
        KnownType.System_Guid
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            if (cc.Compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Components_RouteAttribute) is null)
            {
                return;
            }

            cc.RegisterSymbolAction(c =>
            {
                var componentClass = (INamedTypeSymbol)c.Symbol;
                if (HasRouteAttribute(componentClass))
                {
                    foreach (var propertyType in GetPropertyTypeMismatches(componentClass))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, propertyType.GetLocation(), GetTypeName(propertyType)));
                    }
                }
            },
            SymbolKind.NamedType);
        });

    private static bool HasRouteAttribute(INamedTypeSymbol componentClass) =>
        componentClass.GetAttributes().Any(x => KnownType.Microsoft_AspNetCore_Components_RouteAttribute.Matches(x.AttributeClass));

    private static IEnumerable<TypeSyntax> GetPropertyTypeMismatches(INamedTypeSymbol componentClass) =>
        componentClass
            .GetMembers()
            .Where(IsPropertyTypeMismatch)
            .SelectMany(x => x.DeclaringSyntaxReferences.Select(r => ((PropertyDeclarationSyntax)r.GetSyntax()).Type));

    private static bool IsPropertyTypeMismatch(ISymbol member) =>
        member is IPropertySymbol property
        && property.HasAttribute(KnownType.Microsoft_AspNetCore_Components_SupplyParameterFromQueryAttribute)
        && !SupportedTypes.Any(x => IsSupportedType(property.Type, x));

    private static bool IsSupportedType(ITypeSymbol type, KnownType supportType)
    {
        if (type is IArrayTypeSymbol arrayTypeSymbol)
        {
            type = arrayTypeSymbol.ElementType;
        }

        if (KnownType.System_Nullable_T.Matches(type))
        {
            type = ((INamedTypeSymbol)type).TypeArguments[0];
        }

        return supportType.Matches(type);
    }

    private static string GetTypeName(TypeSyntax propertyType) =>
        propertyType.NameIs(KnownType.System_Nullable_T.TypeName) && propertyType is GenericNameSyntax syntax
            ? syntax.TypeArgumentList.Arguments[0].GetName()
            : propertyType.GetName();
}
