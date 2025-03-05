/*
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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterTypeShouldMatchRouteTypeConstraint : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6800";
    private const string MessageFormat = "{0}";
    private const string ImplicitStringPrimaryMessageFormat = "Parameter type '{0}' does not match route parameter implicit type constraint 'string' in route '{1}'.";
    private const string PrimaryMessageFormat = "Parameter type '{0}' does not match route parameter type constraint '{1}' in route '{2}'.";
    private const string MessageWithSecondaryLocationFormat = "Parameter type '{0}' does not match route parameter type constraint.";
    private const string SecondaryMessageFormat = "This route parameter has a '{0}' type constraint.";
    private const string ImplicitStringSecondaryMessage = "This route parameter has an implicit 'string' type constraint.";

    private const string ImplicitStringConstraint = "string";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    // https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-7.0#route-constraints
    private static readonly Dictionary<string, SupportedType> ConstraintMapping = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { "bool", new SupportedType(KnownType.System_Boolean) },
        { "datetime", new SupportedType(KnownType.System_DateTime) },
        { "decimal", new SupportedType(KnownType.System_Decimal) },
        { "double", new SupportedType(KnownType.System_Double) },
        { "float", new SupportedType(KnownType.System_Single) },
        { "guid", new SupportedType(KnownType.System_Guid) },
        { "int", new SupportedType(KnownType.System_Int32) },
        { "long", new SupportedType(KnownType.System_Int64) },
        // Implicit string constraint
        { ImplicitStringConstraint, new SupportedType(KnownType.System_String) }
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            // If we are not in a Blazor project, we don't need to go further.
            if (cc.Compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Components_RouteAttribute) is null)
            {
                return;
            }

            cc.RegisterSymbolAction(c =>
                {
                    foreach (var property in GetPropertyTypeMismatches((INamedTypeSymbol)c.Symbol, c.Compilation))
                    {
                        c.ReportIssue(Rule, property.Type.GetLocation(), property.ToSecondaryLocations(), property.ToPrimaryMessage());
                    }
                },
                SymbolKind.NamedType);
        });

    private static IReadOnlyList<PropertyTypeMismatch> GetPropertyTypeMismatches(INamedTypeSymbol classSymbol, Compilation compilation)
    {
        var routeParams = GetRouteParametersWithValidConstraint(classSymbol);

        if (routeParams.Count == 0)
        {
            return Array.Empty<PropertyTypeMismatch>();
        }

        return classSymbol
            .GetMembers()
            .Where(IsPropertyWithParameterAttributeInRoute)
            .SelectMany(property => routeParams[property.Name].Select(routeParam => new { RouteParam = routeParam, Property = property }))
            .Where(x => !IsTypeMatchRouteConstraint(x.Property.GetSymbolType(), x.RouteParam.Constraint, compilation))
            .SelectMany(x => x.Property.DeclaringSyntaxReferences
                .Where(r => r.GetSyntax() is PropertyDeclarationSyntax)
                .Select(r => new PropertyTypeMismatch(((PropertyDeclarationSyntax)r.GetSyntax()).Type, x.RouteParam.Constraint, x.RouteParam.FromRoute, x.RouteParam.RouteParamLocation)))
            .ToList();

        bool IsPropertyWithParameterAttributeInRoute(ISymbol member) =>
            member.Kind is SymbolKind.Property && member.HasAttribute(KnownType.Microsoft_AspNetCore_Components_ParameterAttribute) && routeParams.ContainsKey(member.Name);
    }

    private static bool IsTypeMatchRouteConstraint(ITypeSymbol type, string routeConstraintType, Compilation compilation)
    {
        if (type.IsNullableValueType())
        {
            type = ((INamedTypeSymbol)type).TypeArguments[0];
        }

        return ConstraintMapping.ContainsKey(routeConstraintType) && ConstraintMapping[routeConstraintType].Matches(type, compilation);
    }

    private static Dictionary<string, List<RouteParameter>> GetRouteParametersWithValidConstraint(INamedTypeSymbol classDeclaration)
    {
        var routeParameters = new Dictionary<string, List<RouteParameter>>(StringComparer.InvariantCultureIgnoreCase);

        var routeAttributeDataList = classDeclaration.GetAttributes()
            .Where(x => KnownType.Microsoft_AspNetCore_Components_RouteAttribute.Matches(x.AttributeClass));

        foreach (var routeAttributeData in routeAttributeDataList)
        {
            var routeNode = routeAttributeData.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax;

            if (!routeAttributeData.TryGetAttributeValue<string>("template", out var route) || routeNode is null)
            {
                continue;
            }

            var routeParams = route
                .Split('/')
                .Where(segment => segment.StartsWith("{") && segment.EndsWith("}"))
                .Select(x => new { Param = x, Parts = x.TrimStart('{').TrimEnd('}', '?').Split(':') })
                .Where(x => x.Parts.Length == 1 || (x.Parts.Length == 2 && ConstraintMapping.ContainsKey(x.Parts[1])));

            foreach (var routeParam in routeParams)
            {
                routeParameters
                    .GetOrAdd(routeParam.Parts[0], _ => new List<RouteParameter>(1))
                    .Add(new(routeParam.Parts.Length == 2 ? routeParam.Parts[1] : ImplicitStringConstraint,
                        route,
                        CalculateRouteParamLocation(routeNode.GetLocation(), routeNode, routeParam.Param)));
            }
        }

        return routeParameters;
    }

    private static Location CalculateRouteParamLocation(Location attributeLocation, AttributeSyntax routeNode, string routeParam)
    {
        if (GeneratedCodeRecognizer.IsRazorGeneratedFile(attributeLocation.SourceTree))
        {
            return null;
        }

        return routeNode.ArgumentList.Arguments[0].Expression.RemoveParentheses() switch
        {
            var expression when expression.ToString() is var route && route.IndexOf(routeParam, StringComparison.InvariantCulture) is >= 0 and var index =>
                CreateLocationFromRoute(expression.GetLocation(), index, routeParam.Length),
            var expression => expression.GetLocation()
        };

        Location CreateLocationFromRoute(Location routeLocation, int index, int lenght) =>
            Location.Create(routeLocation.SourceTree, new TextSpan(routeLocation.SourceSpan.Start + index, lenght));
    }

    private sealed class PropertyTypeMismatch
    {
        public TypeSyntax Type { get; }
        public string ConstraintType { get; }
        public string Route { get; }
        public Location RouteParamLocation { get; }

        private bool IsImplicitStringConstraint => ConstraintType.Equals("string", StringComparison.OrdinalIgnoreCase);
        private bool CanReportSecondaryLocation => RouteParamLocation is not null;

        public PropertyTypeMismatch(TypeSyntax type, string constraintType, string route, Location routeParamLocation)
        {
            Type = type;
            ConstraintType = constraintType;
            Route = route;
            RouteParamLocation = routeParamLocation;
        }

        public string ToPrimaryMessage()
        {
            if (CanReportSecondaryLocation)
            {
                return string.Format(MessageWithSecondaryLocationFormat, GetTypeName(Type));
            }
            else if (IsImplicitStringConstraint)
            {
                return string.Format(ImplicitStringPrimaryMessageFormat, GetTypeName(Type), Route);
            }
            else
            {
                return string.Format(PrimaryMessageFormat, GetTypeName(Type), ConstraintType.ToLower(), Route);
            }
        }

        private static string GetTypeName(TypeSyntax type) =>
            type switch
            {
                ArrayTypeSyntax _ => type.ToString(),
                PointerTypeSyntax _ => type.ToString(),
                _ => type.GetName()
            };

        public IEnumerable<SecondaryLocation> ToSecondaryLocations() =>
            CanReportSecondaryLocation ? [new(RouteParamLocation, ToSecondaryMessage())] : [];

        private string ToSecondaryMessage() =>
            IsImplicitStringConstraint ? ImplicitStringSecondaryMessage : string.Format(SecondaryMessageFormat, ConstraintType.ToLower());
    }

    private readonly record struct RouteParameter(string Constraint, string FromRoute, Location RouteParamLocation);

    private readonly record struct SupportedType(KnownType ConstraintKnownType)
    {
        public bool Matches(ITypeSymbol propertyType, Compilation compilation)
        {
            if (propertyType.Is(ConstraintKnownType))
            {
                return true;
            }

            var constraintTypeSymbol = compilation.GetTypeByMetadataName(ConstraintKnownType);

            var conversion = compilation.ClassifyConversion(constraintTypeSymbol, propertyType);

            return conversion.IsBoxing || conversion.IsEnumeration;
        }
    }
}
