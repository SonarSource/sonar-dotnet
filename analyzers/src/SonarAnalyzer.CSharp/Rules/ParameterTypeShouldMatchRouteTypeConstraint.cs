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

using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterTypeShouldMatchRouteTypeConstraint : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6800";
    private const string MessageFormat = "{0}";
    private const string MessageNoSecondaryLocationFormat = "Parameter type '{0}' does not match route parameter type constraint '{1}' in route '{2}'.";
    private const string MessageWithSecondaryLocationFormat = "Parameter type '{0}' does not match route parameter type constraint.";
    private const string SecondaryMessageFormat = "This route parameter has a '{0}' type constraint.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    // https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-7.0#route-constraints
    private static readonly Dictionary<string, KnownType> ConstraintMapping = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { "bool", KnownType.System_Boolean },
        { "datetime", KnownType.System_DateTime },
        { "decimal", KnownType.System_Decimal },
        { "double", KnownType.System_Double },
        { "float", KnownType.System_Single },
        { "guid", KnownType.System_Guid },
        { "int", KnownType.System_Int32 },
        { "long", KnownType.System_Int64 },
        { "string", KnownType.System_String }
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

            cc.RegisterNodeAction(c =>
                {
                    foreach (var property in GetPropertyTypeMismatches((ClassDeclarationSyntax)c.Node, c.SemanticModel))
                    {
                        if (property.RouteParamLocation is null)
                        {
                            c.ReportIssue(Diagnostic.Create(Rule,
                                property.Type.GetLocation(),
                                string.Format(MessageNoSecondaryLocationFormat, GetTypeName(property.Type), property.ConstraintType.ToLower(), property.Route)));
                        }
                        else
                        {
                            c.ReportIssue(Diagnostic.Create(Rule,
                                property.Type.GetLocation(),
                                new[] { property.RouteParamLocation },
                                new[] { property.RouteParamLocation }.ToProperties(string.Format(SecondaryMessageFormat, property.ConstraintType)),
                                string.Format(MessageWithSecondaryLocationFormat, GetTypeName(property.Type))));
                        }
                    }
                },
                SyntaxKind.ClassDeclaration);
        });

    private static string GetTypeName(TypeSyntax type) =>
        type switch
        {
            ArrayTypeSyntax _ => type.ToString(),
            PointerTypeSyntax _ => type.ToString(),
            _ => type.GetName()
        };

    private static IList<PropertyTypeMismatch> GetPropertyTypeMismatches(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        var routeParams = GetRouteParametersWithValidConstraint(classDeclaration);

        if (routeParams.Count == 0)
        {
            return Array.Empty<PropertyTypeMismatch>();
        }

        return semanticModel
            .GetDeclaredSymbol(classDeclaration)
            .GetMembers()
            .Where(IsPropertyWithParameterAttributeInRoute)
            .SelectMany(property => routeParams[property.Name].Select(routeParam => new { RouteParam = routeParam, Property = property }))
            .Where(x => !IsTypeMatchRouteConstraint(x.Property.GetSymbolType(), x.RouteParam.Constraint))
            .SelectMany(x => x.Property.DeclaringSyntaxReferences
                .Where(r => r.GetSyntax() is PropertyDeclarationSyntax)
                .Select(r => new PropertyTypeMismatch(((PropertyDeclarationSyntax)r.GetSyntax()).Type, x.RouteParam.Constraint, x.RouteParam.FromRoute, x.RouteParam.RouteParamLocation)))
            .ToList();

        bool IsPropertyWithParameterAttributeInRoute(ISymbol member) =>
            member.Kind is SymbolKind.Property && member.HasAttribute(KnownType.Microsoft_AspNetCore_Components_ParameterAttribute) && routeParams.ContainsKey(member.Name);
    }

    private static bool IsTypeMatchRouteConstraint(ITypeSymbol type, string routeConstraintType)
    {
        if (type.IsNullableValueType())
        {
            type = ((INamedTypeSymbol)type).TypeArguments[0];
        }

        return ConstraintMapping.ContainsKey(routeConstraintType) && type.Is(ConstraintMapping[routeConstraintType]);
    }

    private static Dictionary<string, List<RouteParameter>> GetRouteParametersWithValidConstraint(ClassDeclarationSyntax classDeclaration)
    {
        var routeParameters = new Dictionary<string, List<RouteParameter>>(StringComparer.InvariantCultureIgnoreCase);

        var routes = classDeclaration.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.IsSameShortName(KnownType.Microsoft_AspNetCore_Components_RouteAttribute)
                        && x.ArgumentList.Arguments.Count > 0
                        && x.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax)
            .Select(x => (LiteralExpressionSyntax)x.ArgumentList.Arguments[0].Expression);

        foreach (var route in routes)
        {
            var routeParams = route.Token.ValueText
                .Split('/')
                .Where(segment => segment.StartsWith("{") && segment.EndsWith("}"))
                .Select(x => new { Param = x, Parts = x.TrimStart('{').TrimEnd('}', '?').Split(':') })
                .Where(x => x.Parts.Length == 2 && ConstraintMapping.ContainsKey(x.Parts[1]));

            foreach (var routeParam in routeParams)
            {
                routeParameters
                    .GetOrAdd(routeParam.Parts[0], _ => new List<RouteParameter>(1))
                    .Add(new(routeParam.Parts[1], route.Token.ValueText, CalculateRouteParamLocation(route.GetLocation(), route.Token.ValueText, routeParam.Param)));
            }
        }

        return routeParameters;
    }

    private static Location CalculateRouteParamLocation(Location routeLocation, string route, string routeParam) =>
        !GeneratedCodeRecognizer.IsRazorGeneratedFile(routeLocation.SourceTree)
            ? Location.Create(routeLocation.SourceTree, new TextSpan(routeLocation.SourceSpan.Start + route.IndexOf(routeParam, StringComparison.InvariantCulture) + 1, routeParam.Length))
            : null;

    private sealed record PropertyTypeMismatch(TypeSyntax Type, string ConstraintType, string Route, Location RouteParamLocation);

    private sealed record RouteParameter(string Constraint, string FromRoute, Location RouteParamLocation);
}
