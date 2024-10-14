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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BlazorQueryParameterRoutableComponent : SonarDiagnosticAnalyzer
{
    [Obsolete("This rule has been deprecated since 9.25")]
    private const string NoRouteQueryDiagnosticId = "S6803";
    private const string NoRouteQueryMessageFormat = "Component parameters can only receive query parameter values in routable components.";

    private const string QueryTypeDiagnosticId = "S6797";
    private const string QueryTypeMessageFormat = "Query parameter type '{0}' is not supported.";

    private static readonly DiagnosticDescriptor S6803Rule = DescriptorFactory.Create(NoRouteQueryDiagnosticId, NoRouteQueryMessageFormat);
    private static readonly DiagnosticDescriptor S6797Rule = DescriptorFactory.Create(QueryTypeDiagnosticId, QueryTypeMessageFormat);

    private static readonly ISet<KnownType> SupportedQueryTypes = new HashSet<KnownType>
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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(S6803Rule, S6797Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(c =>
        {
            if (c.Compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Components_RouteAttribute) is not null)
            {
                context.RegisterSymbolAction(CheckQueryProperties, SymbolKind.Property);
            }
        });

    private static void CheckQueryProperties(SonarSymbolReportingContext c)
    {
        var property = (IPropertySymbol)c.Symbol;
        if (property.HasAttribute(KnownType.Microsoft_AspNetCore_Components_SupplyParameterFromQueryAttribute)
            && property.HasAttribute(KnownType.Microsoft_AspNetCore_Components_ParameterAttribute))
        {
            if (!property.ContainingType.HasAttribute(KnownType.Microsoft_AspNetCore_Components_RouteAttribute))
            {
                foreach (var location in property.Locations)
                {
                    c.ReportIssue(S6803Rule, location);
                }
            }
            else if (!SupportedQueryTypes.Any(x => IsSupportedType(property.Type, x)))
            {
                foreach (var propertyType in property.DeclaringSyntaxReferences.Select(x => ((PropertyDeclarationSyntax)x.GetSyntax()).Type))
                {
                    c.ReportIssue(S6797Rule, propertyType.GetLocation(), GetTypeName(propertyType));
                }
            }
        }
    }

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
        propertyType switch
        {
            GenericNameSyntax genericSyntax when propertyType.NameIs(KnownType.System_Nullable_T.TypeName) => genericSyntax.TypeArgumentList.Arguments[0].GetName(),
            {} tuple when TupleTypeSyntaxWrapper.IsInstance(tuple) => KnownType.System_ValueTuple.TypeName,
            _ => propertyType.GetName()
        };
}
